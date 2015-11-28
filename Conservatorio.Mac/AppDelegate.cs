﻿//
// AppDelegate.cs
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

using AppKit;
using Foundation;

namespace Conservatorio.Mac
{
	public partial class AppDelegate : NSApplicationDelegate
	{
		readonly Sparkle.SUUpdater suupdater = new Sparkle.SUUpdater ();

		public override void DidFinishLaunching (NSNotification notification)
		{
			suupdater.AutomaticallyChecksForUpdates = true;
			suupdater.CheckForUpdatesInBackground ();

			NewHandler (this);
		}

		partial void CheckForUpdatesHandler (NSObject sender)
		{
			suupdater.CheckForUpdates (sender);
		}

		partial void NewHandler (NSObject sender)
		{
			new MainWindowController ().Window.MakeKeyAndOrderFront (this);
		}

		partial void ProjectPageHandler (NSObject sender)
		{
			NSWorkspace.SharedWorkspace.OpenUrl (new NSUrl ("http://conservator.io"));
		}
	}
}