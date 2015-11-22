//
// Track.cs
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
	public sealed class Track : Source
	{
		[JsonProperty ("album")]
		public string AlbumName { get; set; }

		[JsonProperty ("albumKey")]
		public string AlbumKey { get; set; }

		[JsonProperty ("albumUrl")]
		public string AlbumUrl { get; set; }

		[JsonProperty ("albumArtist")]
		public string AlbumArtistName { get; set; }

		[JsonProperty ("albumArtistKey")]
		public string AlbumArtistKey { get; set; }

		[JsonProperty ("trackNum")]
		public int TrackNumber { get; set; }

		[JsonProperty ("isInCollection"), IsExtra]
		public bool IsInCollection { get; set; }

		[JsonProperty ("isOnCompilation"), IsExtra]
		public bool IsOnCompilation { get; set; }

		[JsonProperty ("playCount"), IsExtra]
		public int PlayCount { get; set; }

		public override void AcceptVisitor (IRdioObjectKeyVisitor visitor)
		{
			base.AcceptVisitor (visitor);

			if (AlbumKey != null)
				visitor.VisitObjectKey (AlbumKey);

			if (AlbumArtistKey != null)
				visitor.VisitObjectKey (AlbumArtistKey);
		}
	}
}