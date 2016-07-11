
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
