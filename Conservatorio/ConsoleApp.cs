//
// ConsoleApp.cs
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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Mono.Options;

using Conservatorio.Rdio;

namespace Conservatorio
{
	public class ConsoleApp
	{
		string outputPath;

		public int Main (IEnumerable<string> args)
		{
			var showHelp = false;
			outputPath = Environment.CurrentDirectory;

			var optionSet = new OptionSet {
				"Usage: conservatorio [OPTIONS]+ USER [USER...]",
				"",
				"Fetch users' favorites and synced Rdio track metadata as well as all " +
				"favorited, owned, collaborated, and subscriped playlists metadata, " +
				"backing up all raw JSON data as exported by Rdio.",
				"",
				"More information: http://conservator.io",
				"",
				"Options:",
				{ "o|output-dir=",
					"output all JSON data to files in {DIR} directory," +
					"defaulting to the current working directory",
					v => outputPath = v },
				{ "h|help", "show this help", v => showHelp = true }
			};

			var users = optionSet.Parse (args);

			if (showHelp || users.Count == 0) {
				optionSet.WriteOptionDescriptions (Console.Out);
				return 1;
			}

			foreach (var user in users)
				SyncUser (user).Wait ();

			return 0;
		}

		void DisplayStatusBar (int current, int total)
		{
			const int barWidth = 30;
			var progress = current / (double)total;
			int barProgress = (int)Math.Round (barWidth * progress);

			Console.Write ("\r    [");
			for (int i = 0; i < barWidth; i++) {
				if (i <= barProgress)
					Console.Write ('#');
				else
					Console.Write (' ');
			}
			Console.Write ("] ");
			Console.Write ("{0:0.0}% ({1} / {2}) ", progress * 100, current, total);
		}

		public async Task SyncUser (string userId)
		{
			var startTime = DateTime.UtcNow;
			var syncController = new UserSyncController (userId, new RdioObjectStore ());

			do {
				try {
					await syncController.SyncStepAsync (() => DisplayStatusBar (
						syncController.SyncedObjects, syncController.TotalObjects));
				} catch (Exception e) {
					var webException = e as WebException;
					if (webException != null)
						Console.WriteLine ("  ! Connection Error");
					else if (e is UserNotFoundException)
						Console.WriteLine ("  ! Could not find Rdio user {0}",
							syncController.UserIdentifier);
					else if (e is OperationCanceledException)
						Console.WriteLine ("  ! Canceled");
					else
						Console.WriteLine (e);

					syncController = null;
					return;
				}

				switch (syncController.SyncState) {
				case SyncState.Start:
					break;
				case SyncState.FindingUser:
					Console.WriteLine ("Starting work for '{0}'...",
						syncController.UserIdentifier);
					break;
				case SyncState.FoundUser:
					Console.WriteLine ("  * Resolved user {0} ({1})...",
						syncController.UserKeyStore.User.DisplayName,
						syncController.UserKeyStore.User.Key);
					break;
				case SyncState.SyncingUserKeys:
					Console.WriteLine ("  * Fetching keys...");
					break;
				case SyncState.SyncedUserKeys:
					Console.WriteLine ("    {0} toplevel keys of interest",
						syncController.UserKeyStore.TotalKeys);
					break;
				case SyncState.SyncingObjects:
					Console.WriteLine ("  * Fetching objects...");
					break;
				case SyncState.SyncedObjects:
					Console.WriteLine ();
					Console.WriteLine ("    {0} objects fetched",
						syncController.TotalObjects);
					break;
				case SyncState.Finished:
					Console.WriteLine ("  * Fetched in {0}", (DateTime.UtcNow - startTime).ToFriendlyString ());
					var path = Path.Combine (outputPath, syncController.FileName);
					Console.WriteLine ("  * Exporting to {0}...", path);
					syncController.Export (path);
					Console.WriteLine ("  * Done!");
					break;
				}
			} while (syncController.SyncState != SyncState.Finished);
		}
	}
}