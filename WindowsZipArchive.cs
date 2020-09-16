//
// WindowsZipArchive.cs
//
// Author:
//       Dean Ellis <dellis1972@googlemail.com>
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
using Xamarin.Tools.Zip.Properties;

namespace Xamarin.Tools.Zip
{
	public class WindowsZipArchive : ZipArchive
	{
		internal WindowsPlatformOptions UnixOptions {
			get {
				var opts = Options as WindowsPlatformOptions;
				if (opts == null)
					throw new InvalidOperationException (string.Format (Resources.UnexpectedOptionsType_string_type_type, nameof (Options), typeof (WindowsPlatformOptions).Name, Options?.GetType ()?.Name ?? "Unknown"));
				return opts;
			}
		}

		internal WindowsZipArchive (string defaultExtractionDir, WindowsPlatformOptions options) : base (defaultExtractionDir, options)
		{
		}

		internal WindowsZipArchive (Stream stream, WindowsPlatformOptions options, OpenFlags flags) : base (stream, options, flags)
		{
		}

		internal bool SetEntryUnixPermissions (ulong index, EntryPermissions requestedPermissions, UnixExternalPermissions unixPermissions)
		{
			var permissions = (uint)requestedPermissions | (uint)unixPermissions;
			int ret = Native.zip_file_set_external_attributes (ArchivePointer, index, OperationFlags.None, (byte)OperatingSystem.UNIX, permissions << 16);
			return ret == 0;
		}
	}
}

