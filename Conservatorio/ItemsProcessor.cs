//
// QueueProcessor.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Conservatorio
{
	public class ItemsProcessor<T>
	{	
		readonly ReaderWriterLockSlim mutex = new ReaderWriterLockSlim (LockRecursionPolicy.NoRecursion);
		readonly HashSet<T> items;
		readonly Func<IReadOnlyList<T>, Task> itemsProcessor;
		readonly Action<int> chunkProcessed;
		readonly int workerCount;
		readonly int workChunkSize;

		bool isProcessing;
		int totalEnqueued;
		int totalProcessed;

		public bool IsProcessing {
			get { return isProcessing; }
		}

		public int TotalEnqueued {
			get { return totalEnqueued; }
		}

		public int TotalProcessed {
			get { return totalProcessed; }
		}

		public ItemsProcessor (IEnumerable<T> items, Func<IReadOnlyList<T>, Task> itemsProcessor,
			Action<int> chunkProcessed = null,
			int? workerCount = null,
			int workChunkSize = 100)
		{
			if (items == null)
				throw new ArgumentNullException (nameof (items));

			if (itemsProcessor == null)
				throw new ArgumentNullException (nameof (itemsProcessor));

			this.items = new HashSet<T> (items);
			this.itemsProcessor = itemsProcessor;
			this.chunkProcessed = chunkProcessed;

			if (workerCount == null || !workerCount.HasValue)
				this.workerCount = Environment.ProcessorCount;
			else if (workerCount.Value < 1)
				this.workerCount = 1;
			else
				this.workerCount = workerCount.Value;

			this.workChunkSize = workChunkSize;

			totalEnqueued = this.items.Count;
		}

		public void Add (T item)
		{
			try {
				mutex.EnterWriteLock ();
				if (items.Add (item))
					totalEnqueued++;
			} finally {
				mutex.ExitWriteLock ();
			}
		}

		public void Add (IEnumerable<T> items)
		{
			try {
				mutex.EnterWriteLock ();
				foreach (var item in items) {
					if (this.items.Add (item))
						totalEnqueued++;
				}
			} finally {
				mutex.ExitWriteLock ();
			}
		}

		public async Task ProcessAsync (CancellationToken ct = default (CancellationToken))
		{
			isProcessing = true;
			var tasks = new List<Task> (workerCount);
			for (int i = 0; i < workerCount; i++)
				tasks.Add (Task.Run (() => CoreProcessAsync (ct), ct));
			await Task.WhenAll (tasks);
			isProcessing = false;
		}

		async Task CoreProcessAsync (CancellationToken ct)
		{
			while (items.Count > 0) {
				ct.ThrowIfCancellationRequested ();

				var takenItems = new List<T> (workChunkSize);

				try {
					mutex.EnterWriteLock ();

					foreach (var item in items) {
						takenItems.Add (item);
						if (takenItems.Count >= workChunkSize)
							break;
					}

					foreach (var item in takenItems)
						items.Remove (item);
				} finally {
					mutex.ExitWriteLock ();
				}

				await itemsProcessor (takenItems);

				Interlocked.Add (ref totalProcessed, takenItems.Count);

				chunkProcessed?.Invoke (takenItems.Count);
			}
		}
	}
}