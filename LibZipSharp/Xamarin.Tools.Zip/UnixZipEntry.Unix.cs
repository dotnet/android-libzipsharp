//
// UnixZipEntry.Unix.cs
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

using Mono.Unix.Native;

namespace Xamarin.Tools.Zip
{
	partial class UnixZipEntry
	{
		internal const FilePermissions DefaultDirMode = FilePermissions.S_IRUSR | FilePermissions.S_IWUSR | FilePermissions.S_IXUSR |
														 FilePermissions.S_IRGRP | FilePermissions.S_IXGRP |
														 FilePermissions.S_IROTH | FilePermissions.S_IXOTH;
		internal const FilePermissions DefaultFileMode = FilePermissions.S_IRUSR | FilePermissions.S_IWUSR | FilePermissions.S_IRGRP | FilePermissions.S_IROTH;

		FilePermissions? permissions;

		partial void SetFilePermissions (uint value)
		{
			if (!Enum.IsDefined (typeof (FilePermissions), value))
				throw new ArgumentOutOfRangeException (nameof (value), $"value {value} does not map exactly to FilePermissions");

			FilePermissions = (FilePermissions)value;
		}

		internal FilePermissions FilePermissions {
			get {
				if (permissions.HasValue)
					return permissions.Value;

				return IsDirectory ? DefaultDirMode : DefaultFileMode;
			}

			set { permissions = value; }
		}

		uint GetPermissions ()
		{
			return (uint)FilePermissions;
		}
	}
}

