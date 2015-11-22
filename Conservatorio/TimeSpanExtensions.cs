//
// TimeSpanExtensions.cs
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
using System.Text;

namespace Conservatorio
{
	public static class TimeSpanExtensions
	{
		public static string ToFriendlyString (this TimeSpan timeSpan)
		{
			var builder = new StringBuilder ();
			if (timeSpan.Days > 0)
				builder.Append (timeSpan.Days).Append (Plural (timeSpan.Days, " day", " days"));
			if (timeSpan.Hours > 0)
				builder.Append (' ').Append (timeSpan.Hours).Append (Plural (timeSpan.Hours, " hour", " hours"));
			if (timeSpan.Minutes > 0)
				builder.Append (' ').Append (timeSpan.Minutes).Append (Plural (timeSpan.Minutes, " minute", " minutes"));
			if (timeSpan.Seconds > 0)
				builder.Append (' ').Append (timeSpan.Seconds).Append (Plural (timeSpan.Seconds, " second", " seconds"));
			return builder.ToString ().TrimStart ();
		}
		
		static string Plural (int n, string singular, string plural)
		{
			return n == 1 ? singular : plural;
		}
	}
}