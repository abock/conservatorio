//
// User.cs
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

using Newtonsoft.Json;

namespace Conservatorio.Rdio
{
	[JsonObject]
	public class User : RdioObject
	{
		[JsonProperty ("firstName")]
		public string FirstName { get; set; }

		[JsonProperty ("lastName")]
		public string LastName { get; set; }

		[JsonProperty ("gender")]
		public string Gender { get; set; }

		[JsonProperty ("url")]
		public string Url { get; set; }

		[JsonProperty ("libraryVersion")]
		public int LibraryVersion { get; set; }

		[JsonProperty ("isProtected")]
		public bool IsProtected { get; set; }

		[JsonProperty ("icon")]
		public string Icon { get; set; }

		[JsonProperty ("icon250")]
		public string Icon250 { get; set; }

		[JsonProperty ("icon500")]
		public string Icon500 { get; set; }

		[JsonProperty ("baseIcon")]
		public string BaseIcon { get; set; }

		[JsonProperty ("dynamicIcon")]
		public string DynamicIcon { get; set; }

		[JsonProperty ("username"), IsExtra]
		public string VanityName { get; set; }

		[JsonProperty ("displayName"), IsExtra]
		public string DisplayName { get; set; }

		[JsonProperty ("registrationDate"), IsExtra]
		public string RegistrationDate { get; set; }

		[JsonProperty ("trackCount"), IsExtra]
		public int TrackCount { get; set; }

		[JsonProperty ("albumCount"), IsExtra]
		public int AlbumCount { get; set; }

		[JsonProperty ("lastSourcePlayed"), IsExtra]
		[JsonConverter (typeof (RdioObject.JsonConverter))]
		public RdioObject LastSourcePlayed { get; set; }

		[JsonProperty ("lastSongPlayTime"), IsExtra]
		public string LastSongPlayTime { get; set; }

		public override void AcceptVisitor (IRdioObjectKeyVisitor visitor)
		{
		}
	}
}