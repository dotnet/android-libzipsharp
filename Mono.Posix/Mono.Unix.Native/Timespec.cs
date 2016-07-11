//
// Timespec.cs
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

namespace Mono.Unix.Native
{
	[Map ("struct timespec")]
	struct Timespec
				: IEquatable<Timespec>
	{
		[time_t]
		public long tv_sec;   // Seconds.
		public long tv_nsec;  // Nanoseconds.

		public override int GetHashCode ()
		{
			return tv_sec.GetHashCode () ^ tv_nsec.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || obj.GetType () != GetType ())
				return false;
			Timespec value = (Timespec)obj;
			return value.tv_sec == tv_sec && value.tv_nsec == tv_nsec;
		}

		public bool Equals (Timespec value)
		{
			return value.tv_sec == tv_sec && value.tv_nsec == tv_nsec;
		}

		public static bool operator == (Timespec lhs, Timespec rhs)
		{
			return lhs.Equals (rhs);
		}

		public static bool operator != (Timespec lhs, Timespec rhs)
		{
			return !lhs.Equals (rhs);
		}
	}

}
