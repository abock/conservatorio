//
// Album.cs
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
using Newtonsoft.Json.Linq;

namespace Conservatorio.Rdio
{
	[JsonObject]
	public sealed class Album : Source
	{
		[JsonProperty ("trackKeys")]
		public string [] TrackKeys { get; set; }

		[JsonProperty ("displayDate")]
		public string DisplayReleaseDate { get; set; }

		[JsonProperty ("releaseDate")]
		public string ReleaseDate { get; set; }

		[JsonProperty ("label"), IsExtra]
		[JsonConverter (typeof (RdioObject.JsonConverter))]
		public Label Label { get; set; }

		[JsonProperty ("hasListened"), IsExtra]
		public bool HasListened { get; set; }

		[JsonProperty ("isCompilation"), IsExtra]
		public bool IsCompilation { get; set; }

		[JsonProperty ("dominantColor"), IsExtra]
		public JObject DominantColor { get; set; }

		[JsonProperty ("releaseDateISO"), IsExtra]
		public string ReleaseDateIso { get; set; }

		[JsonProperty ("upcs"), IsExtra]
		public string [] Upcs { get; set; }

		[JsonProperty ("bigIcon"), IsExtra]
		public string BigIcon { get; set; }

		[JsonProperty ("iconKey"), IsExtra]
		public string IconKey { get; set; }

		public override void AcceptVisitor (IRdioObjectKeyVisitor visitor)
		{
			base.AcceptVisitor (visitor);

			if (TrackKeys != null) {
				foreach (var trackKey in TrackKeys)
					visitor.VisitObjectKey (trackKey);
			}
		}
	}
}