//
// RdioObject.cs
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
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conservatorio.Rdio
{
	public abstract class RdioObject
	{
		internal class JsonConverter : Newtonsoft.Json.JsonConverter
		{
			public override object ReadJson (JsonReader reader, Type objectType, object existingValue,
				JsonSerializer serializer)
			{
				return FromJson (JObject.Load (reader));
			}

			public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
			{
				throw new NotImplementedException ();
			}

			public override bool CanConvert (Type objectType)
			{
				throw new NotImplementedException ();
			}
		}

		static readonly Dictionary<string, Type> typeMap = new Dictionary<string, Type> {
			["s"] = typeof(User),
			["r"] = typeof(Artist),
			["a"] = typeof(Album),
			["t"] = typeof(Track),
			["p"] = typeof(Playlist)
		};

		static readonly Dictionary<Type, List<string>> extrasMap = new Dictionary<Type, List<string>> ();
		static HashSet<string> allExtras;

		public static IEnumerable<string> GetExtraKeys ()
		{
			lock (typeMap) {
				if (allExtras != null)
					return allExtras;
				
				allExtras = new HashSet<string> (typeMap.Values.SelectMany (GetExtraKeys));
				return allExtras;
			}
		}

		public static IEnumerable<string> GetExtraKeys (string objectKey)
		{
			lock (typeMap) {
				Type clrType;
				if (typeMap.TryGetValue (objectKey [0].ToString (), out clrType))
					return GetExtraKeys (clrType);
				return new string [0];
			}
		}

		public static IEnumerable<string> GetExtraKeys<T> () where T : RdioObject
		{
			return GetExtraKeys (typeof(T));
		}

		public static IEnumerable<string> GetExtraKeys (Type type)
		{
			lock (typeMap) {
				List<string> extras;
				if (extrasMap.TryGetValue (type, out extras))
					return extras;
			
				extras = new List<string> ();

				foreach (var prop in type.GetProperties ()) {
					if (prop.GetCustomAttribute<IsExtraAttribute> () != null) {
						var jsonProp = prop.GetCustomAttribute<JsonPropertyAttribute> ();
						extras.Add (jsonProp.PropertyName);
					}
				}

				extrasMap.Add (type, extras);
				return extras;
			}
		}

		public static RdioObject FromJson (JToken json)
		{
			string jsonType;
			try {
				jsonType = json ["type"].Value<string> ();
			} catch {
				return null;
			}

			Type clrType;
			if (!typeMap.TryGetValue (jsonType, out clrType))
				clrType = typeof(UnknownRdioObject);

			RdioObject obj;

			try {
				obj = (RdioObject)json.ToObject (clrType, new JsonSerializer {
					NullValueHandling = NullValueHandling.Ignore,
				});
				obj.IsPopulatedFromSource = true;
			} catch (Exception e) {
				Console.WriteLine (e);
				obj = (RdioObject)Activator.CreateInstance (clrType);
			}

			obj.SourceJson = json;
			return obj;
		}

		public static T FromJson<T> (JToken json) where T : RdioObject
		{
			return (T)FromJson (json);
		}

		public bool IsPopulatedFromSource { get; set; }
		public JToken SourceJson { get; set; }

		[JsonProperty ("key")]
		public string Key { get; set; }

		[JsonProperty ("type")]
		public string Type { get; set; }

		public abstract void AcceptVisitor (IRdioObjectKeyVisitor visitor);
	}
}