//
// Main.cs
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

using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using Foundation;
using AppKit;

namespace Conservatorio.Mac
{
	static class MainClass
	{
		[DllImport ("__Internal")]
		static extern string conservatorio_get_original_cwd ();

		static int Main (string[] args)
		{
			NSApplication.Init ();

			if (args.Length > 0) {
				// the XM launcher code for whatever reason changes into
				// the NSBundle.MainBundle.ResourcePath directory...
				// we install a launcher hook (xamarin_app_initialize)
				// to preserve the original CWD so we can change back to it
				Directory.SetCurrentDirectory (conservatorio_get_original_cwd ());

				// the init above will set this thread's sync context to
				// integrate with NSRunLoop, which we do not want
				SynchronizationContext.SetSynchronizationContext (null);
				return new ConsoleApp ().Main (args);
			}

			var sparkleFx = Path.Combine (NSBundle.MainBundle.BundlePath,
				"Contents", "Frameworks", "Sparkle.framework", "Sparkle");
			ObjCRuntime.Dlfcn.dlopen (sparkleFx, 0);

			NSApplication.Main (new string[0]);
			return 0;
		}
	}
}