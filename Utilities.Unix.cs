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

namespace Xamarin.Tools.Zip
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

		public static string GetLastErrorMessage ()
		{
			return Syscall.strerror (Syscall.GetLastError ());
		}

		static bool StatFile (string path, bool followSymlinks, out Stat sbuf)
		{
			int rv;
			if (followSymlinks)
				rv = Syscall.stat (path, out sbuf);
			else
				rv = Syscall.lstat (path, out sbuf);

			if (rv < 0) {
				// TODO: log properly. Maybe throw exception?
				Console.WriteLine ($"Warning: failed to stat file '{path}': {GetLastErrorMessage ()}");
				return false;
			}

			return true;
		}

		public static bool GetFilePermissions (string path, bool followSymlinks, out FilePermissions filePermissions)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException ("must not be null or empty", nameof (path));

			Stat sbuf;
			if (!StatFile (path, followSymlinks, out sbuf)) {
				filePermissions = FilePermissions.DEFFILEMODE;
				return false;
			}

			filePermissions = sbuf.st_mode & FilePermissions.ALLPERMS;
			return true;
		}

		public static bool GetFilePermissions (string path, bool followSymlinks, out UnixExternalPermissions filePermissions)
		{
			Stat sbuf;
			if (!StatFile (path, followSymlinks, out sbuf)) {
				filePermissions = (UnixExternalPermissions)FilePermissions.DEFFILEMODE;
				return false;
			}

			filePermissions = MapToUnixExternalPermissions (sbuf.st_mode & FilePermissions.ALLPERMS);
			return true;
		}

		static UnixExternalPermissions MapToUnixExternalPermissions (FilePermissions fp)
		{
			UnixExternalPermissions filePermissions = 0;
			if (fp.HasFlag (FilePermissions.S_ISUID))
				filePermissions |= UnixExternalPermissions.ISUID;

			if (fp.HasFlag (FilePermissions.S_ISGID))
				filePermissions |= UnixExternalPermissions.ISGID;

			if (fp.HasFlag (FilePermissions.S_ISVTX))
				filePermissions |= UnixExternalPermissions.ISVTX;

			if (fp.HasFlag (FilePermissions.S_IRUSR))
				filePermissions |= UnixExternalPermissions.IRUSR;

			if (fp.HasFlag (FilePermissions.S_IWUSR))
				filePermissions |= UnixExternalPermissions.IWUSR;

			if (fp.HasFlag (FilePermissions.S_IXUSR))
				filePermissions |= UnixExternalPermissions.IXUSR;

			if (fp.HasFlag (FilePermissions.S_IRGRP))
				filePermissions |= UnixExternalPermissions.IRGRP;

			if (fp.HasFlag (FilePermissions.S_IWGRP))
				filePermissions |= UnixExternalPermissions.IWGRP;

			if (fp.HasFlag (FilePermissions.S_IXGRP))
				filePermissions |= UnixExternalPermissions.IXGRP;

			if (fp.HasFlag (FilePermissions.S_IROTH))
				filePermissions |= UnixExternalPermissions.IROTH;

			if (fp.HasFlag (FilePermissions.S_IWOTH))
				filePermissions |= UnixExternalPermissions.IWOTH;

			if (fp.HasFlag (FilePermissions.S_IXOTH))
				filePermissions |= UnixExternalPermissions.IXOTH;

			if (filePermissions == 0)
				return (UnixExternalPermissions)FilePermissions.DEFFILEMODE;

			return filePermissions;
		}

		public static bool GetFileType (string path, bool followSymlinks, out FilePermissions fileType)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException ("must not be null or empty", nameof (path));

			Stat sbuf;
			if (!StatFile (path, followSymlinks, out sbuf)) {
				fileType = FilePermissions.S_IFREG;
				return false;
			}

			fileType = sbuf.st_mode & ~FilePermissions.ALLPERMS;
			return true;
		}

		public static bool GetFileType (string path, bool followSymlinks, out UnixExternalPermissions fileType)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException ("must not be null or empty", nameof (path));

			Stat sbuf;
			if (!StatFile (path, followSymlinks, out sbuf)) {
				fileType = UnixExternalPermissions.IFREG;
				return false;
			}

			fileType =  GetFileType (sbuf);
			return true;
		}

		public static UnixExternalPermissions GetFileType (Stat sbuf)
		{
			FilePermissions mode = sbuf.st_mode & ~FilePermissions.ALLPERMS;
			if (mode == FilePermissions.S_IFBLK)
				return UnixExternalPermissions.IFBLK;

			if (mode == FilePermissions.S_IFCHR)
				return UnixExternalPermissions.IFCHR;

			if (mode == FilePermissions.S_IFDIR)
				return UnixExternalPermissions.IFDIR;

			if (mode == FilePermissions.S_IFIFO)
				return UnixExternalPermissions.IFIFO;

			if (mode == FilePermissions.S_IFLNK)
				return UnixExternalPermissions.IFLNK;

			if (mode == FilePermissions.S_IFSOCK)
				return UnixExternalPermissions.IFSOCK;

			return UnixExternalPermissions.IFREG;
		}

		public static UnixExternalPermissions GetFilePermissions (Stat sbuf)
		{
			return MapToUnixExternalPermissions (sbuf.st_mode & FilePermissions.ALLPERMS);
		}
	}
}

