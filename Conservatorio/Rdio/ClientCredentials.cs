//
// ClientCredentials.cs
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
using System.Text;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace Conservatorio.Rdio
{
	[JsonObject]
	public sealed class ClientCredentials
	{
		const string ResourceName = "Conservatorio.Rdio.ClientCredentials.json";

		[JsonProperty ("client_id")]
		public string Id { get; set; }

		[JsonProperty ("client_secret")]
		public string Secret { get; set; }

		public AuthenticationHeaderValue ToHttpBasicAuthentication ()
		{
			var authBytes = Encoding.ASCII.GetBytes ($"{Id}:{Secret}");
			return new AuthenticationHeaderValue ("Basic",  Convert.ToBase64String (authBytes));
		}

		public static ClientCredentials Load ()
		{
			using (var stream = typeof(ClientCredentials).Assembly.GetManifestResourceStream (ResourceName))
				return Load (new StreamReader (stream));
		}

		public static ClientCredentials Load (TextReader reader)
		{
			return new JsonSerializer ().Deserialize<ClientCredentials> (new JsonTextReader (reader));
		}
	}
}