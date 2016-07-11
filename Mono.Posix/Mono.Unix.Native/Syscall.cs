//
// Syscall.cs
//
// Author:
//       Marek Habersack <grendel@twistedcode.net>
//
// Copyright (c) 2016 Marek Habersack
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
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Unix.Native
{
	[CLSCompliant (false)]
	sealed class Syscall : Stdlib
	{
		delegate long DoReadlinkFun (byte [] target);

		new internal const string LIBC = "libc";

		#region <sys/stat.h> Declarations
		//
		// <sys/stat.h>  -- COMPLETE
		//
		[DllImport (MPH, SetLastError = true,
		            EntryPoint = "Mono_Posix_Syscall_stat")]
		public static extern int stat (
			[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
			string file_name, out Stat buf);

		[DllImport (MPH, SetLastError = true,
		            EntryPoint = "Mono_Posix_Syscall_lstat")]
		public static extern int lstat (
			[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
			string file_name, out Stat buf);
		#endregion

		// Helper function for readlink(string, StringBuilder) and readlinkat (int, string, StringBuilder)
		static int ReadlinkIntoStringBuilder (DoReadlinkFun doReadlink, [Out] StringBuilder buf, ulong bufsiz)
		{
			// bufsiz > int.MaxValue can't work because StringBuilder can store only int.MaxValue chars
			int bufsizInt = checked((int)bufsiz);
			var target = new byte [bufsizInt];

			var r = doReadlink (target);
			if (r < 0)
				return checked((int)r);

			buf.Length = 0;
			var chars = UnixEncoding.Instance.GetChars (target, 0, checked((int)r));
			// Make sure that at more bufsiz chars are written
			buf.Append (chars, 0, System.Math.Min (bufsizInt, chars.Length));
			if (r == bufsizInt) {
				// may not have read full contents; fill 'buf' so that caller can properly check
				buf.Append (new string ('\x00', bufsizInt - buf.Length));
			}
			return buf.Length;
		}

		// readlink(2)
		//    ssize_t readlink(const char *path, char *buf, size_t bufsize);
		public static int readlink (string path, [Out] StringBuilder buf, ulong bufsiz)
		{
			return ReadlinkIntoStringBuilder (target => readlink (path, target), buf, bufsiz);
		}

		public static int readlink (string path, [Out] StringBuilder buf)
		{
			return readlink (path, buf, (ulong)buf.Capacity);
		}

		[DllImport (MPH, SetLastError = true,
						EntryPoint = "Mono_Posix_Syscall_readlink")]
		private static extern long readlink (
						[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
								string path, byte [] buf, ulong bufsiz);

		public static long readlink (string path, byte [] buf)
		{
			return readlink (path, buf, (ulong)buf.LongLength);
		}

		// chmod(2)
		//    int chmod(const char *path, mode_t mode);
		[DllImport (LIBC, SetLastError = true, EntryPoint = "chmod")]
		private static extern int sys_chmod (
						[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
								string path, uint mode);

		public static int chmod (string path, FilePermissions mode)
		{
			uint _mode = NativeConvert.FromFilePermissions (mode);
			return sys_chmod (path, _mode);
		}

		[DllImport (MPH, SetLastError = true,
								EntryPoint = "Mono_Posix_Syscall_utimes")]
		private static extern int sys_utimes (
								[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
								string filename, Timeval [] tvp);

		public static int utimes (string filename, Timeval [] tvp)
		{
			if (tvp != null && tvp.Length != 2) {
				SetLastError (Errno.EINVAL);
				return -1;
			}
			return sys_utimes (filename, tvp);
		}

		[DllImport (MPH, SetLastError = true,
						EntryPoint = "Mono_Posix_Syscall_lutimes")]
		private static extern int sys_lutimes (
						[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
								string filename, Timeval [] tvp);

		public static int lutimes (string filename, Timeval [] tvp)
		{
			if (tvp != null && tvp.Length != 2) {
				SetLastError (Errno.EINVAL);
				return -1;
			}
			return sys_lutimes (filename, tvp);
		}

		// chown(2)
		//    int chown(const char *path, uid_t owner, gid_t group);
		[DllImport (LIBC, SetLastError = true)]
		public static extern int chown (
			[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
			string path, uint owner, uint group);

		[DllImport (LIBC, SetLastError = true)]
		public static extern int symlink (
								[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
								string oldpath,
								[MarshalAs (UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(FileNameMarshaler))]
								string newpath);
	}
}
