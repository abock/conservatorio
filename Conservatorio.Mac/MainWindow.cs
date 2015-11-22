//
// MainWindow.cs
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

using Foundation;
using AppKit;

namespace Conservatorio.Mac
{
	partial class MainWindow : NSWindow
	{
		static readonly NSColor backgroundColor = NSColor.White;
		static readonly NSColor titleTextColor = NSColor.FromCalibratedRgba (0.2f, 0.2f, 0.2f, 1);
		static readonly NSColor textColor = NSColor.FromCalibratedRgba (0.4f, 0.4f, 0.4f, 1);

		public MainWindow (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MainWindow (NSCoder coder) : base (coder)
		{
		}

		public override void AwakeFromNib ()
		{
			base.AwakeFromNib ();

			StyleMask |= NSWindowStyle.FullSizeContentView;
			TitlebarAppearsTransparent = true;
			TitleVisibility = NSWindowTitleVisibility.Hidden;

			BackgroundColor = backgroundColor;
			MovableByWindowBackground = true;

			StyleViews (ContentView);

			titleTextField.TextColor = titleTextColor;
		}

		void StyleViews (NSView view)
		{
			var textField = view as NSTextField;
			if (textField != null)
				textField.TextColor = textColor;

			var subviews = view.Subviews;
			if (subviews != null) {
				foreach (var subview in subviews)
					StyleViews (subview);
			}
		}
	}
}