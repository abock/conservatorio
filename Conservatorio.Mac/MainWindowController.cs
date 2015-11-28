//
// MainWindowController.cs
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Foundation;
using AppKit;

using Conservatorio.Rdio;

namespace Conservatorio.Mac
{
	partial class MainWindowController : NSWindowController
	{
		UserSyncController syncController;
		NSUrl savedExportedDataUrl;

		public MainWindowController (IntPtr handle) : base (handle)
		{
		}

		[Export ("initWithCoder:")]
		public MainWindowController (NSCoder coder) : base (coder)
		{
		}

		public MainWindowController () : base ("MainWindow")
		{
		}

		public override void AwakeFromNib ()
		{
			userTextField.Activated += async (sender, e) => await StartSyncing ();

			actionButton.Activated += (sender, e) => {
				switch (tabView.IndexOf (tabView.Selected)) {
				case 1:
					syncController?.Cancel ();
					tabView.SelectAt (0);
					break;
				}
			};

			tabView.DidSelect += (sender, e) => {
				switch (tabView.IndexOf (e.Item)) {
				case 0:
					userTextField.BecomeFirstResponder ();
					break;
				case 1:
					actionButton.Title = "Cancel";
					break;
				}
			};

			saveButton.Activated += async (sender, e) => {
				if (savedExportedDataUrl != null)
					NSWorkspace.SharedWorkspace.ActivateFileViewer (new [] { savedExportedDataUrl });
				else
					await SaveExportedData ();
			};

			base.AwakeFromNib ();
		}

		async Task StartSyncing ()
		{
			if (String.IsNullOrEmpty (userTextField.StringValue))
				return;

			var startTime = DateTime.UtcNow;

			statusTextField.StringValue = String.Empty;
			detailedStatusTextField.StringValue = String.Empty;

			indeterminateProgressIndicator.StartAnimation (userTextField);
			indeterminateProgressIndicator.Hidden = false;
			progressIndicator.Hidden = true;
			progressIndicator.DoubleValue = 0;

			saveButton.Hidden = true;
			savedExportedDataUrl = null;
			saveButton.Title = "Save Exported Data…";

			tabView.SelectAt (1);

			syncController = new UserSyncController (userTextField.StringValue, new RdioObjectStore ());

			do {
				SyncState currentState;
				var previousState = syncController.SyncState;

				try {
					currentState = await syncController.SyncStepAsync (() => BeginInvokeOnMainThread (() => {
						progressIndicator.MinValue = 0;
						progressIndicator.MaxValue = syncController.TotalObjects;
						progressIndicator.DoubleValue = syncController.SyncedObjects;
						detailedStatusTextField.StringValue = String.Format ("{0} / {1}",
							syncController.SyncedObjects, syncController.TotalObjects);
					}));
				} catch (Exception e) {
					if (!(e is OperationCanceledException))
						Console.Error.WriteLine (e);
				
					progressIndicator.Hidden = true;
					indeterminateProgressIndicator.Hidden = true;
					detailedStatusTextField.Hidden = true;
					actionButton.Title = "Go Back";

					var webException = e as WebException;
					var httpException = e as HttpRequestException;
					if (webException != null || httpException != null)
						statusTextField.StringValue = "connection error. rdio has probably " +
							"gone offline forever. RIP in Peace \ud83d\ude22";
					else if (e is UserNotFoundException)
						statusTextField.StringValue = "could not find Rdio user "
							+ syncController.UserIdentifier;
					else if (e is OperationCanceledException)
						statusTextField.StringValue = "canceled";
					else
						statusTextField.StringValue = e.Message;

					syncController = null;
					return;
				}

				Console.WriteLine ("{0} -> {1}", previousState, currentState);

				switch (syncController.SyncState) {
				case SyncState.Start:
					break;
				case SyncState.FindingUser:
					statusTextField.StringValue = "searching for "
						+ syncController.UserIdentifier + "…";
					break;
				case SyncState.FoundUser:
					break;
				case SyncState.SyncingUserKeys:
					statusTextField.StringValue = "fetching user keys…";
					break;
				case SyncState.SyncedUserKeys:
					break;
				case SyncState.SyncingObjects:
					detailedStatusTextField.Hidden = false;
					indeterminateProgressIndicator.Hidden = true;
					progressIndicator.Hidden = false;
					statusTextField.StringValue = "fetching objects…";
					break;
				case SyncState.SyncedObjects:
					break;
				case SyncState.Finished:
					statusTextField.StringValue = "done in " + (DateTime.UtcNow - startTime).ToFriendlyString ();
					progressIndicator.Hidden = true;
					indeterminateProgressIndicator.Hidden = true;
					detailedStatusTextField.Hidden = true;
					saveButton.Hidden = false;
					saveButton.BecomeFirstResponder ();
					saveButton.Highlight (true);
					saveButton.KeyEquivalent = "\r";
					actionButton.Title = "Start Over";
					break;
				}
			} while (syncController.SyncState != SyncState.Finished);
		}

		async Task SaveExportedData ()
		{
			if (syncController?.SyncState != SyncState.Finished)
				return;

			var dialog = new NSSavePanel {
				Title = "Save Exported Rdio Data",
				NameFieldStringValue = syncController.FileName
			};

			var window = Window;
			if (window == null)
				await SaveExportedData (window, dialog.RunModal (), dialog.Url);
			else
				dialog.BeginSheet (window, async result => await SaveExportedData (window, result, dialog.Url));
		}

		async Task SaveExportedData (NSWindow window, nint panelResult, NSUrl url)
		{
			if (panelResult != 1 || syncController?.SyncState != SyncState.Finished)
				return;

			string error = null;

			try {
				if (url == null || url.FilePathUrl == null) {
					error = url?.ToString () ?? "Invalid file URL";
					return;
				}

				statusTextField.StringValue = "saving with sadness…";
				saveButton.Enabled = false;
				actionButton.Enabled = false;
				indeterminateProgressIndicator.Hidden = false;

				await syncController.CreateExporter ().ExportAsync (url.FilePathUrl.Path);

				statusTextField.StringValue = "saved your tedious music curation work!";
				actionButton.Enabled = true;
				indeterminateProgressIndicator.Hidden = true;

				savedExportedDataUrl = url;
				saveButton.Enabled = true;
				saveButton.Title = "Reveal in Finder…";
			} catch (Exception e) {
				Console.Error.WriteLine (e);
				error = e.Message;
			} finally {
				if (error != null)
					new NSAlert {
						AlertStyle = NSAlertStyle.Critical,
						MessageText = "Unable to save exported data to file",
						InformativeText = error
					}.RunSheetModal (window, NSApplication.SharedApplication);
			}
		}
	}
}