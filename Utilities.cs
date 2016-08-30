//
// Utilities.cs
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
using System.Runtime.InteropServices;

namespace Xamarin.Tools.Zip
{
	partial class Utilities
	{
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public static bool IsUnix { get; } = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;

		public static int Errno {
			get { return Marshal.GetLastWin32Error (); }
		}

		static Utilities ()
		{
		}

		public static string GetStringFromNativeAnsi (IntPtr data)
		{
			return Marshal.PtrToStringAnsi (data);
		}

		public static DateTime DateTimeFromUnixTime (ulong time)
		{
			try {
				return UnixEpoch.AddSeconds (time);
			} catch (ArgumentOutOfRangeException) {
				// Some ZIPs have timestamps larger than 9218762655527012 which
				// will cause the above code to throw. We'll return the epoch
				// in such instance
				return UnixEpoch;
			}
		}

		public static ulong UnixTimeFromDateTime (DateTime time)
		{
			if (time < UnixEpoch)
				return 0;

			return (ulong)((time - UnixEpoch).TotalSeconds);
		}
	}
}

