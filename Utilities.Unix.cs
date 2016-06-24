//
// Utilities.Unix.cs
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
using Mono.Unix.Native;

namespace Xamarin.ZipSharp
{
	partial class Utilities
	{
		public static Timeval TimevalFromDateTime (DateTime time)
		{
			ulong utime = Utilities.UnixTimeFromDateTime (time);
			long usec = 0;
			if (utime > 0) {
				TimeSpan udt = time - Utilities.UnixEpoch;
				usec = udt.Milliseconds * 1000;
			}
			return new Timeval {
				tv_sec = (long)(utime == 0 ? 0 : utime),
				tv_usec = usec
			};
		}

		public static UnixExternalPermissions GetFileType (Stat sbuf)
		{
			if (sbuf.st_mode.HasFlag (FilePermissions.S_IFBLK))
				return UnixExternalPermissions.IFBLK;

			if (sbuf.st_mode.HasFlag (FilePermissions.S_IFCHR))
				return UnixExternalPermissions.IFCHR;

			if (sbuf.st_mode.HasFlag (FilePermissions.S_IFDIR))
				return UnixExternalPermissions.IFDIR;

			if (sbuf.st_mode.HasFlag (FilePermissions.S_IFIFO))
				return UnixExternalPermissions.IFIFO;

			if (sbuf.st_mode.HasFlag (FilePermissions.S_IFLNK))
				return UnixExternalPermissions.IFLNK;

			if (sbuf.st_mode.HasFlag (FilePermissions.S_IFSOCK))
				return UnixExternalPermissions.IFSOCK;

			return UnixExternalPermissions.IFREG;
		}

		public static UnixExternalPermissions GetFilePermissions (Stat sbuf)
		{
			var ret = (UnixExternalPermissions)(0);

			return ret;
		}
	}
}

