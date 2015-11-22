//
// Source.cs
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
	public abstract class Source : RdioObject
	{
		[JsonProperty ("name")]
		public string Name { get; set; }

		[JsonProperty ("icon")]
		public string Icon { get; set; }

		[JsonProperty ("baseIcon")]
		public string BaseIcon { get; set; }

		[JsonProperty ("dynamicIcon")]
		public string DynamicIcon { get; set; }

		[JsonProperty ("url")]
		public string Url { get; set; }

		[JsonProperty ("artist")]
		public string ArtistName { get; set; }

		[JsonProperty ("artistKey")]
		public string ArtistKey { get; set; }

		[JsonProperty ("artistUrl")]
		public string ArtistUrl { get; set; }

		[JsonProperty ("isExplicit")]
		public bool IsExplicit { get; set; }

		[JsonProperty ("isClean")]
		public bool IsClean { get; set; }

		[JsonProperty ("length")]
		public int TrackCount { get; set; }

		[JsonProperty ("duration")]
		public int Duration { get; set; }

		[JsonProperty ("canStream")]
		public bool CanStream { get; set; }

		[JsonProperty ("canSample")]
		public bool CanSample { get; set; }

		[JsonProperty ("canTether")]
		public bool CanTether { get; set; }

		[JsonProperty ("shortUrl")]
		public string ShortUrl { get; set; }

		[JsonProperty ("embedUrl")]
		public string EmbedUrl { get; set; }

		[JsonProperty ("price")]
		public string Price { get; set; }

		public override void AcceptVisitor (IRdioObjectKeyVisitor visitor)
		{
			if (ArtistKey != null)
				visitor.VisitObjectKey (ArtistKey);
		}
	}
}