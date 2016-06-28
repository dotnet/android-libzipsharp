//
// UnixZipArchive.cs
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
using System.IO;
using System.Text;

namespace Xamarin.ZipSharp
{
	public class UnixZipArchive : ZipArchive
	{
		public UnixPlatformOptions UnixOptions {
			get {
				var opts = Options as UnixPlatformOptions;
				if (opts == null)
					throw new InvalidOperationException ("Unexpected options type");
				return opts;
			}
		}

		internal UnixZipArchive (string defaultExtractionDir, UnixPlatformOptions options) : base (defaultExtractionDir, options)
		{
		}

		internal UnixZipArchive (Stream stream, UnixPlatformOptions options, OpenFlags flags) : base (stream, options, flags)
		{
		}

		public ZipEntry CreateSymbolicLink (string linkName, string linkDestination, EntryPermissions requestedPermissions= EntryPermissions.Default, Encoding encoding = null)
		{
			ZipEntry entry = AddEntry (linkName, linkDestination, encoding ?? Encoding.UTF8, CompressionMethod.Store);
			if (entry == null)
				return null;
			if (!SetEntryUnixPermissions (entry.Index, requestedPermissions == EntryPermissions.Default ? DefaultFilePermissions : requestedPermissions, UnixExternalPermissions.IFLNK))
				throw GetErrorException ();

			// We read it again to update permissions, flags, extra fields etc
			return ReadEntry (entry.Index);
		}

		internal bool SetEntryUnixPermissions (ulong index, EntryPermissions requestedPermissions, UnixExternalPermissions unixPermissions)
		{
			var permissions = (uint)requestedPermissions | (uint)unixPermissions;
			int ret = Native.zip_file_set_external_attributes (ArchivePointer, index, OperationFlags.None, (byte)OperatingSystem.UNIX, permissions << 16);
			return ret == 0;
		}
	}
}

