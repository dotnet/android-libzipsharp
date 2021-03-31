//
// Timeval.cs
//
// Authors:
//   Miguel de Icaza (miguel@novell.com)
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2003 Novell, Inc.
// (C) 2004-2006 Jonathan Pryor
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Mono.Unix.Native
{
	[Map ("struct timeval")]
	struct Timeval
				: IEquatable<Timeval>
	{
		[time_t]
		public long tv_sec;   // seconds
		[suseconds_t]
		public long tv_usec;  // microseconds

		public override int GetHashCode ()
		{
			return tv_sec.GetHashCode () ^ tv_usec.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || obj.GetType () != GetType ())
				return false;
			Timeval value = (Timeval)obj;
			return value.tv_sec == tv_sec && value.tv_usec == tv_usec;
		}

		public bool Equals (Timeval value)
		{
			return value.tv_sec == tv_sec && value.tv_usec == tv_usec;
		}

		public static bool operator == (Timeval lhs, Timeval rhs)
		{
			return lhs.Equals (rhs);
		}

		public static bool operator != (Timeval lhs, Timeval rhs)
		{
			return !lhs.Equals (rhs);
		}
	}
}
