//
// RdioUserKeyStore.cs
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
using System.Collections.Generic;
using System.Threading;

namespace Conservatorio.Rdio
{
	public class RdioUserKeyStore
	{
		readonly ReaderWriterLockSlim mutex = new ReaderWriterLockSlim (LockRecursionPolicy.NoRecursion);

		readonly HashSet<string> allKeys = new HashSet<string> ();
		public IEnumerable<string> AllKeys {
			get {
				try {
					mutex.EnterReadLock ();
					return allKeys;
				} finally {
					mutex.ExitReadLock ();
				}
			}
		}

		public int TotalKeys {
			get {
				try {
					mutex.EnterReadLock ();
					return allKeys.Count;
				} finally {
					mutex.ExitReadLock ();
				}
			}
		}

		readonly List<string> syncedKeys = new List<string> ();
		public IReadOnlyList<string> SyncedKeys {
			get {
				try {
					mutex.EnterReadLock ();
					// should clone, but realistically this won't get used until
					// after a sync is complete and no further writes are coming
					return syncedKeys;
				} finally {
					mutex.ExitReadLock ();
				}
			}
		}

		readonly List<string> favoritesKeys = new List<string> ();
		public IReadOnlyList<string> FavoritesKeys {
			get {
				try {
					mutex.EnterReadLock ();
					// should clone, but realistically this won't get used until
					// after a sync is complete and no further writes are coming
					return favoritesKeys;
				} finally {
					mutex.ExitReadLock ();
				}
			}
		}

		readonly Dictionary<string, List<string>> playlistsKeys = new Dictionary<string, List<string>> ();
		public Dictionary<string, List<string>> PlaylistsKeys {
			get {
				try {
					mutex.EnterReadLock ();
					// should clone, but realistically this won't get used until
					// after a sync is complete and no further writes are coming
					return playlistsKeys;
				} finally {
					mutex.ExitReadLock ();
				}
			}
		}

		public User User { get; }

		public RdioUserKeyStore (User user)
		{
			if (user == null)
				throw new ArgumentNullException (nameof (user));

			User = user;
		}

		void AddKeys (List<string> target, IEnumerable<string> source)
		{
			if (source == null)
				return;

			try {
				mutex.EnterWriteLock ();
				foreach (var key in source) {
					allKeys.Add (key);
					target.Add (key);
				}
			} finally {
				mutex.ExitWriteLock ();
			}
		}

		public void AddSyncedKeys (IEnumerable<string> keys)
		{
			AddKeys (syncedKeys, keys);
		}

		public void AddFavoritesKeys (IEnumerable<string> keys)
		{
			AddKeys (favoritesKeys, keys);
		}

		public void AddPlaylists (string kind, IEnumerable<string> keys)
		{
			List<string> target;
			if (!playlistsKeys.TryGetValue (kind, out target))
				playlistsKeys.Add (kind, target = new List<string> ());
			AddKeys (target, keys);
		}
	}
}