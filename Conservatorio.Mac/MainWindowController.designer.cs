// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Conservatorio.Mac
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		AppKit.NSButton actionButton { get; set; }

		[Outlet]
		AppKit.NSTextField detailedStatusTextField { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator indeterminateProgressIndicator { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator progressIndicator { get; set; }

		[Outlet]
		AppKit.NSButton saveButton { get; set; }

		[Outlet]
		AppKit.NSTextField statusTextField { get; set; }

		[Outlet]
		AppKit.NSTabView tabView { get; set; }

		[Outlet]
		AppKit.NSTextField userTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (actionButton != null) {
				actionButton.Dispose ();
				actionButton = null;
			}

			if (detailedStatusTextField != null) {
				detailedStatusTextField.Dispose ();
				detailedStatusTextField = null;
			}

			if (indeterminateProgressIndicator != null) {
				indeterminateProgressIndicator.Dispose ();
				indeterminateProgressIndicator = null;
			}

			if (progressIndicator != null) {
				progressIndicator.Dispose ();
				progressIndicator = null;
			}

			if (statusTextField != null) {
				statusTextField.Dispose ();
				statusTextField = null;
			}

			if (tabView != null) {
				tabView.Dispose ();
				tabView = null;
			}

			if (userTextField != null) {
				userTextField.Dispose ();
				userTextField = null;
			}

			if (saveButton != null) {
				saveButton.Dispose ();
				saveButton = null;
			}
		}
	}
}
