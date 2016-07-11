//
// NativeConvert.generated.cs
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

namespace Mono.Unix.Native
{
	sealed partial class NativeConvert
	{
		const string LIB = "MonoPosixHelper";

		static void ThrowArgumentException (object value)
		{
			throw new ArgumentOutOfRangeException ("value", value, "Current platform doesn't support this value.");
		}

		[DllImport (LIB, EntryPoint = "Mono_Posix_FromErrno")]
		private static extern int FromErrno (Errno value, out Int32 rval);

		public static bool TryFromErrno (Errno value, out Int32 rval)
		{
			return FromErrno (value, out rval) == 0;
		}

		public static Int32 FromErrno (Errno value)
		{
			Int32 rval;
			if (FromErrno (value, out rval) == -1)
				ThrowArgumentException (value);
			return rval;
		}

		[DllImport (LIB, EntryPoint = "Mono_Posix_ToErrno")]
		private static extern int ToErrno (Int32 value, out Errno rval);

		public static bool TryToErrno (Int32 value, out Errno rval)
		{
			return ToErrno (value, out rval) == 0;
		}

		public static Errno ToErrno (Int32 value)
		{
			Errno rval;
			if (ToErrno (value, out rval) == -1)
				ThrowArgumentException (value);
			return rval;
		}

		[DllImport (LIB, EntryPoint = "Mono_Posix_FromFilePermissions")]
		private static extern int FromFilePermissions (FilePermissions value, out UInt32 rval);

		public static bool TryFromFilePermissions (FilePermissions value, out UInt32 rval)
		{
			return FromFilePermissions (value, out rval) == 0;
		}

		public static UInt32 FromFilePermissions (FilePermissions value)
		{
			UInt32 rval;
			if (FromFilePermissions (value, out rval) == -1)
				ThrowArgumentException (value);
			return rval;
		}

	}
}
