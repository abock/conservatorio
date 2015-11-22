//
// TokenEndpointResponse.cs
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
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace Conservatorio.Rdio
{
	[JsonObject]
	public sealed class TokenEndpointResponse
	{
		[JsonProperty ("error")]
		public string ErrorCode { get; set; }

		[JsonProperty ("error_description")]
		public string ErrorDescription { get; set; }

		[JsonProperty ("token_type")]
		public string TokenType { get; set; }

		[JsonProperty ("access_token")]
		public string AccessToken { get; set; }

		[JsonProperty ("refresh_token")]
		public string RefreshToken { get; set; }

		[JsonProperty ("expires_in")]
		public long ExpiresIn { get; set; }

		[JsonProperty ("scope")]
		public string Scope { get; set; }

		public AuthenticationHeaderValue ToHttpAuthenticationHeader ()
		{
			if (String.IsNullOrEmpty (AccessToken))
				throw new InvalidOperationException ("AccessToken is not valid");

			if (!String.Equals (TokenType, "bearer", StringComparison.OrdinalIgnoreCase))
				throw new NotImplementedException ($"unsupported TokenType: {TokenType}");

			return new AuthenticationHeaderValue ("Bearer",  AccessToken);
		}
	}
}