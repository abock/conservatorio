//
// Exporter.cs
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2015 Xamarin Inc. All rights reserved.
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
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Conservatorio.Rdio;

namespace Conservatorio
{
	public class Exporter
	{
		public RdioObjectStore ObjectStore { get; set; }
		public IEnumerable<RdioUserKeyStore> UserKeyStores { get; set; }

		public void Export (string path)
		{
			using (var stream = new FileStream (path, FileMode.Create, FileAccess.Write))
				Export (stream);
		}

		public void Export (Stream stream)
		{
			Export (new StreamWriter (stream));
		}

		public void Export (TextWriter writer)
		{
			new JsonSerializer {
				NullValueHandling = NullValueHandling.Include,
				DefaultValueHandling = DefaultValueHandling.Include,
				Formatting = Formatting.Indented
			}.Serialize (writer, new ExportRoot {
				Users = new List<ExportUser> (UserKeyStores.Select (userKeyStore => 
					new ExportUser {
						UserKey = userKeyStore.User.Key,
						SyncedKeys = userKeyStore.SyncedKeys.ToArray (),
						FavoritesKeys = userKeyStore.FavoritesKeys.ToArray (),
						PlaylistsKeys = userKeyStore.PlaylistsKeys
					}
				)),
				Objects = ObjectStore.Export ()
			});

			writer.Flush ();
		}

		public async Task ExportAsync (string path)
		{
			await Task.Run (() => Export (path));
		}
	}
}