//
// SyncController.cs
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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Conservatorio.Rdio;

namespace Conservatorio
{
	public class UserSyncController : IRdioObjectKeyVisitor
	{
		readonly Api api = new Api ();

		ItemsProcessor<string> keysProcessor;

		public string UserIdentifier { get; }
		public RdioObjectStore ObjectStore { get; }
		public RdioUserKeyStore UserKeyStore { get; private set; }

		public SyncState SyncState { get; private set; }
		public int TotalObjects { get; private set; }
		public int SyncedObjects { get; private set; }

		CancellationTokenSource cts = new CancellationTokenSource ();
		public CancellationToken CancellationToken {
			get { return cts.Token; }
		}

		public string FileName {
			get {
				return UserIdentifier == null
					? "UnknownRdioUser.json"
					: String.Format ("Conservatorio_RdioExport_{0}.json", UserIdentifier);
			}
		}

		public UserSyncController (string userIdentifier, RdioObjectStore sharedObjectStore)
		{
			if (String.IsNullOrEmpty (userIdentifier))
				throw new ArgumentException ("must not be null or empty",
					nameof (userIdentifier));

			if (sharedObjectStore == null)
				throw new ArgumentNullException (nameof (sharedObjectStore));

			UserIdentifier = userIdentifier;
			ObjectStore = sharedObjectStore;

			api.CancellationToken = CancellationToken;
		}

		public void Cancel ()
		{
			cts.Cancel (true);
		}

		public async Task<SyncState> SyncStepAsync (Action syncProgressChanged)
		{
			switch (SyncState) {
			case SyncState.Start:
				SyncState = SyncState.FindingUser;
				break;
			case SyncState.FindingUser:
				UserKeyStore = new RdioUserKeyStore (await FindUserAsync ());
				SyncState = SyncState.FoundUser;
				break;
			case SyncState.FoundUser:
				SyncState = SyncState.SyncingUserKeys;
				break;
			case SyncState.SyncingUserKeys:
				foreach (var kind in new [] { "owned", "favorites", "subscribed", "collab" })
					await api.LoadUserPlaylists (kind, UserKeyStore);
				await api.LoadFavoritesAndSyncedKeysAsync (UserKeyStore);
				SyncState = SyncState.SyncedUserKeys;
				break;
			case SyncState.SyncedUserKeys:
				SyncState = SyncState.SyncingObjects;
				break;
			case SyncState.SyncingObjects:
				keysProcessor = new ItemsProcessor<string> (
					UserKeyStore.AllKeys.Where (k => !ObjectStore.ContainsKey (k)),
					KeyProcessorAsync,
					chunkCount => {
						TotalObjects = keysProcessor.TotalEnqueued;
						SyncedObjects = keysProcessor.TotalProcessed;
						syncProgressChanged?.Invoke ();
					}
				);

				TotalObjects = keysProcessor.TotalEnqueued;
				SyncedObjects = 0;

				await keysProcessor.ProcessAsync (CancellationToken);

				SyncState = SyncState.SyncedObjects;
				break;
			case SyncState.SyncedObjects:
				SyncState = SyncState.Finished;
				break;
			}

			return SyncState;
		}

		static readonly Regex userKeyRegex = new Regex (@"^s\d+$");

		async Task<User> FindUserAsync ()
		{
			string key = null;
			string email = null;
			string vanityName = null;

			if (userKeyRegex.IsMatch (UserIdentifier))
				key = UserIdentifier;
			else if (UserIdentifier.IndexOf ('@') > 0)
				email = UserIdentifier;
			else
				vanityName = UserIdentifier;

			User user = null;

			if (key != null) {
				user = await api.GetObjectAsync<User> (key);
				if (user == null)
					vanityName = key;
			}

			if (user == null) {
				user = await api.FindUserAsync (email, vanityName);
				if (user == null)
					throw new UserNotFoundException (UserIdentifier);
			}

			if (user.IsProtected)
				throw new UserIsProtectedException (user);

			return user;
		}

		async Task KeyProcessorAsync (IEnumerable<string> keys)
		{
			await api.LoadObjectsAsync (
				ObjectStore,
				keys.Where (k => !ObjectStore.ContainsKey (k)),
				obj => obj.AcceptVisitor (this)
			);
		}

		void IRdioObjectKeyVisitor.VisitObjectKey (string objectKey)
		{
			if (keysProcessor != null &&
				keysProcessor.IsProcessing &&
				!ObjectStore.ContainsKey (objectKey))
				keysProcessor.Add (objectKey);
		}

		public Exporter CreateExporter ()
		{
			return new Exporter {
				ObjectStore = ObjectStore,
				UserKeyStores = new [] { UserKeyStore }
			};
		}
	}
}