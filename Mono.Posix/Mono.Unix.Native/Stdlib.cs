//
// Stdlib.cs
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
	class Stdlib
	{
		internal const string LIBC = "msvcrt";
		internal const string MPH = "MonoPosixHelper";

		//
		// <string.h>
		//

		private static object strerror_lock = new object ();

		[DllImport (LIBC, CallingConvention = CallingConvention.Cdecl,
						SetLastError = true, EntryPoint = "strerror")]
		private static extern IntPtr sys_strerror (int errnum);

		// strlen(3):
		//    size_t strlen(const char *s);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention = CallingConvention.Cdecl,
		            SetLastError = true, EntryPoint = "Mono_Posix_Stdlib_strlen")]
		public static extern ulong strlen (IntPtr s);

		[CLSCompliant (false)]
		public static string strerror (Errno errnum)
		{
			int e = NativeConvert.FromErrno (errnum);
			lock (strerror_lock) {
				IntPtr r = sys_strerror (e);
				return UnixMarshal.PtrToString (r);
			}
		}

		// strerror_r(3)
		//    int strerror_r(int errnum, char *buf, size_t n);
		[DllImport (MPH, SetLastError = true,
						EntryPoint = "Mono_Posix_Syscall_strerror_r")]
		private static extern int sys_strerror_r (int errnum,
						[Out] StringBuilder buf, ulong n);

		public static int strerror_r (Errno errnum, StringBuilder buf, ulong n)
		{
			int e = NativeConvert.FromErrno (errnum);
			return sys_strerror_r (e, buf, n);
		}

		public static int strerror_r (Errno errnum, StringBuilder buf)
		{
			return strerror_r (errnum, buf, (ulong)buf.Capacity);
		}

		[DllImport (LIBC, CallingConvention = CallingConvention.Cdecl)]
		public static extern void free (IntPtr ptr);

		// malloc(3):
		//    void *malloc(size_t size);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention = CallingConvention.Cdecl,
						SetLastError = true, EntryPoint = "Mono_Posix_Stdlib_malloc")]
		public static extern IntPtr malloc (ulong size);

		// realloc(3):
		//    void *realloc(void *ptr, size_t size);
		[CLSCompliant (false)]
		[DllImport (MPH, CallingConvention = CallingConvention.Cdecl,
						SetLastError = true, EntryPoint = "Mono_Posix_Stdlib_realloc")]
		public static extern IntPtr realloc (IntPtr ptr, ulong size);

		public static Errno GetLastError ()
		{
			int errno = Marshal.GetLastWin32Error ();
			return NativeConvert.ToErrno (errno);
		}

		[DllImport (MPH, CallingConvention = CallingConvention.Cdecl,
								EntryPoint = "Mono_Posix_Stdlib_SetLastError")]
		private static extern void SetLastError (int error);

		protected static void SetLastError (Errno error)
		{
			int _error = NativeConvert.FromErrno (error);
			SetLastError (_error);
		}

	}
}
