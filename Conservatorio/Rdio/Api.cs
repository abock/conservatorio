//
// RdioApi.cs
//
// Author:
//   Aaron Bockover <aaron.bockover@gmail.com>
//
// Copyright 2015 Aaron Bockover. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conservatorio.Rdio
{
	public sealed class Api
	{
		const int MaxRetryAttempts = 5;

		static readonly Uri baseAddress = new Uri ("https://services.rdio.com");

		static HttpClient CreateHttp ()
		{
			return new HttpClient {
				BaseAddress = baseAddress,
				Timeout = TimeSpan.FromSeconds (30)
			};
		}

		static FormUrlEncodedContent CreateHttpContent (params string [] keyValuePairs)
		{
			if (keyValuePairs.Length % 2 != 0)
				throw new ArgumentException (
					"must have an even number of elements to form proper key/value pairs",
					nameof (keyValuePairs));

			var content = new KeyValuePair<string, string> [keyValuePairs.Length / 2];
			for (int i = 0, j = 0; i < keyValuePairs.Length; i += 2, j++)
				content [j] = new KeyValuePair<string, string> (
					keyValuePairs [i],
					keyValuePairs [i + 1]
				);

			return new FormUrlEncodedContent (content);
		}

		readonly ClientCredentials clientCredentials;
		HttpClient apiClient;
		TokenEndpointResponse tokenEndpointResponse;

		public CancellationToken CancellationToken { get; set; }

		public Api () : this (ClientCredentials.Load ())
		{
		}

		public Api (ClientCredentials clientCredentials)
		{
			if (clientCredentials == null)
				throw new ArgumentNullException (nameof (clientCredentials));

			this.clientCredentials = clientCredentials;
		}

		async Task RefreshAccessTokenAsync ()
		{
			var client = CreateHttp ();
			client.DefaultRequestHeaders.Authorization = clientCredentials.ToHttpBasicAuthentication ();

			var response = await client.PostAsync (
				"/oauth2/token",
				CreateHttpContent ("grant_type", "client_credentials")
			);

			tokenEndpointResponse = await response.Content.ReadAsJsonAsync<TokenEndpointResponse> ();

			if (tokenEndpointResponse?.ErrorCode != null)
				throw new TokenEndpointException (tokenEndpointResponse);

			if (!response.IsSuccessStatusCode)
				throw new HttpRequestException ("invalid HTTP request with no TokenEndpointResponse");
		}

		async Task EnsureApiClientAsync ()
		{
			if (apiClient != null)
				return;

			if (tokenEndpointResponse == null)
				await RefreshAccessTokenAsync ();

			apiClient = CreateHttp ();
			apiClient.DefaultRequestHeaders.Authorization
				= tokenEndpointResponse.ToHttpAuthenticationHeader ();
		}

		public async Task<T> CallAsync<T> (string method, IDictionary<string, string> parameters = null)
			where T : RdioObject
		{
			return RdioObject.FromJson<T> (await CallAsync (method, parameters));
		}

		public Task<JToken> CallAsync (string method, IDictionary<string, string> parameters = null)
		{
			var dict = new Dictionary<string, string> ((parameters?.Count).GetValueOrDefault () + 1) {
				{ "method", method }
			};

			if (parameters != null) {
				foreach (var param in parameters)
					dict.Add (param.Key, param.Value);
			}

			return CallInternalAsync (new FormUrlEncodedContent (dict), 0);
		}

		async Task<JToken> CallInternalAsync (FormUrlEncodedContent content, int attemptNumber)
		{
			CancellationToken.ThrowIfCancellationRequested ();

			await EnsureApiClientAsync ();

			try {
				var response = await apiClient.PostAsync ("/api/1", content, CancellationToken);
				if (!response.IsSuccessStatusCode)
					throw new HttpRequestException ("invalid HTTP request");

				var stream = await response.Content.ReadAsStreamAsync ();
				var json = (JObject)JToken.ReadFrom (new JsonTextReader (new StreamReader (stream)));

				if (json ["status"].Value<string> () != "ok") {
					Console.Error.WriteLine (json);
					throw new HttpRequestException ("JSON result is not status==ok");
				}

				return json ["result"];
			} catch (Exception e) when (!(e is HttpRequestException)) {
				apiClient = null;
				tokenEndpointResponse = null;

				if (attemptNumber >= MaxRetryAttempts)
					throw;

				return await CallInternalAsync (content, attemptNumber++);
			}
		}

		public async Task<User> FindUserAsync (string email = null, string vanityName = null)
		{
			var parameters = new Dictionary<string, string> (1);

			if (email != null)
				parameters.Add ("email", email);
			else if (vanityName != null)
				parameters.Add ("vanityName", vanityName);

			parameters.Add ("extras", String.Join (",", RdioObject.GetExtraKeys<User> ()));

			return await CallAsync<User> ("findUser", parameters);
		}

		public async Task LoadFavoritesAndSyncedKeysAsync (RdioUserKeyStore targetUserStore)
		{
			var result = (JObject)await CallAsync ("getKeysInFavoritesAndSynced",
				new Dictionary<string, string> {
					["user"] = targetUserStore.User.Key
				}
			);

			var synced = result ["syncedKeys"] as JArray;
			if (synced != null)
				targetUserStore.AddSyncedKeys (synced.Values<string> ());

			var favorites = result ["favoritesKeys"] as JArray;
			if (favorites != null)
				targetUserStore.AddFavoritesKeys (favorites.Values<string> ());
		}

		public async Task LoadUserPlaylists (string kind, RdioUserKeyStore targetUserStore)
		{
			int offset = 0;
			var keys = new List<string> ();

			while (true) {
				var result = await CallAsync ("getUserPlaylists", new Dictionary<string, string> {
					["user"] = targetUserStore.User.Key,
					["kind"] = kind,
					["start"] = offset.ToString (),
					["extras"] = "-*,key"
				}) as JArray;

				if (result?.Count == 0)
					break;

				offset += result.Count;

				if (result != null)
					targetUserStore.AddPlaylists (kind,
						result.Select (p => p ["key"].Value<string> ()));
			}
		}

		async Task<JObject> CoreGetObjectsAsync (IEnumerable<string> keys)
		{
			return (JObject)await CallAsync ("get", new Dictionary<string, string> {
				["keys"] = String.Join (",", keys),
				["extras"] = String.Join (",", RdioObject.GetExtraKeys ())
			});
		}

		public async Task<T> GetObjectAsync<T> (string key) where T : RdioObject
		{;
			foreach (var property in await CoreGetObjectsAsync (new [] { key }))
				return RdioObject.FromJson (property.Value) as T;
			return null;
		}

		public async Task<IEnumerable<RdioObject>> GetObjectsAsync (IEnumerable<string> keys)
		{
			var objects = new List<RdioObject> ();
			foreach (var property in await CoreGetObjectsAsync (keys))
				objects.Add (RdioObject.FromJson (property.Value));
			return objects;
		}

		public async Task LoadObjectsAsync (RdioObjectStore targetStore, IEnumerable<string> keys,
			Action<RdioObject> objectLoaded = null)
		{
			foreach (var property in await CoreGetObjectsAsync (keys)) {
				var obj = RdioObject.FromJson (property.Value);
				targetStore.Add (obj);
				objectLoaded?.Invoke (obj);
			}
		}

		public Task<IReadOnlyList<string>> GetUserFollowingAsync (User user)
		{
			return GetUserFollowingAsync (user.Key);
		}

		public async Task<IReadOnlyList<string>> GetUserFollowingAsync (string userKey)
		{
			int offset = 0;
			var keys = new List<string> ();

			while (true) {
				var result = await CallAsync ("userFollowing", new Dictionary<string, string> {
					["user"] = userKey,
					["sort"] = "recent",
					["start"] = offset.ToString (),
					["count"] = "100",
					["extras"] = "-*,key"
				}) as JArray;

				if (result?.Count == 0)
					return keys;

				offset += result.Count;

				foreach (var key in result)
					keys.Add (key ["key"].Value<string> ());
			}
		}
	}
}