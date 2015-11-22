//
// RdioObjectStore.cs
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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Newtonsoft.Json.Linq;

namespace Conservatorio.Rdio
{
	public class RdioObjectStore : IReadOnlyDictionary<string, RdioObject>
	{
		readonly ConcurrentDictionary<string, RdioObject> objects
			= new ConcurrentDictionary<string, RdioObject> ();

		public int Count {
			get { return objects.Count; }
		}

		public IEnumerable<string> Keys {
			get { return objects.Keys; }
		}

		public IEnumerable<RdioObject> Values {
			get { return objects.Values; }
		}

		public RdioObject this [string key] {
			get {
				RdioObject o;
				objects.TryGetValue (key, out o);
				return o;
			}

			set { objects [value.Key] = value; }
		}

		public void Add (RdioObject o)
		{
			objects [o.Key] = o;
		}

		public bool ContainsKey (string key)
		{
			return objects.ContainsKey (key);
		}

		public bool TryGetValue (string key, out RdioObject value)
		{
			return objects.TryGetValue (key, out value);
		}

		public IEnumerator<RdioObject> GetEnumerator ()
		{
			foreach (var item in objects)
				yield return item.Value;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		IEnumerator<KeyValuePair<string, RdioObject>> IEnumerable<KeyValuePair<string, RdioObject>>.GetEnumerator ()
		{
			return objects.GetEnumerator ();
		}

		public Dictionary<string, JToken> Export ()
		{
			var dict = new Dictionary<string, JToken> (objects.Count);
			foreach (var o in objects)
				dict.Add (o.Key, o.Value.SourceJson);
			return dict;
		}
	}
}