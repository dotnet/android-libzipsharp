//
// ZipEntryEnumerator.cs
//
// Author:
//       Marek Habersack <grendel@twistedcode.net>
//
// Copyright (c) 2016 Xamarin, Inc (http://xamarin.com)
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
using System.Collections;
using System.Collections.Generic;

namespace Xamarin.ZipSharp
{
	class ZipEntryEnumerator : IEnumerator <ZipEntry>
	{
		ZipArchive          archive;
		ZipEntry            current;
		ulong               index;
		bool                start;

		public ZipEntry Current { 
			get { return ReadEntry (index); }
		}
		
		object IEnumerator.Current {
			get { return Current; }
		}

		public ZipEntryEnumerator (ZipArchive archive)
		{
			if (archive == null)
				throw new ArgumentNullException (nameof (archive));
			this.archive = archive;
			index = 0;
			Reset ();
		}

		public bool MoveNext ()
		{
			if (!start)
				index++;
			else
				start = false;

			// Calling it each time because the archive can change in the meantime
			long nentries = archive.NumberOfEntries;
			if (nentries < 0 || index >= (ulong)nentries)
				return false;
			return true;
		}

		public void Reset ()
		{
			start = true;
			index = 0;
		}

		public void Dispose ()
		{
			archive = null;
			current = null;
		}

		ZipEntry ReadEntry (ulong index)
		{
			if (start)
				return null;

			if (current != null && current.Index == index)
				return current;
			
			current = archive.ReadEntry (index);
			return current;
		}
	}
}

