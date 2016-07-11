//
// Stat.cs
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
	// Use manually written To/From methods to handle fields st_atime_nsec etc.
	struct Stat
			: IEquatable<Stat>
	{
		[CLSCompliant (false)]
		[dev_t]
		public ulong st_dev;     // device
		[CLSCompliant (false)]
		[ino_t]
		public ulong st_ino;     // inode
		[CLSCompliant (false)]
		public FilePermissions st_mode;    // protection
		[NonSerialized]
#pragma warning disable 169
		private uint _padding_;  // padding for structure alignment
#pragma warning restore 169
		[CLSCompliant (false)]
		[nlink_t]
		public ulong st_nlink;   // number of hard links
		[CLSCompliant (false)]
		[uid_t]
		public uint st_uid;     // user ID of owner
		[CLSCompliant (false)]
		[gid_t]
		public uint st_gid;     // group ID of owner
		[CLSCompliant (false)]
		[dev_t]
		public ulong st_rdev;    // device type (if inode device)
		[off_t]
		public long st_size;    // total size, in bytes
		[blksize_t]
		public long st_blksize; // blocksize for filesystem I/O
		[blkcnt_t]
		public long st_blocks;  // number of blocks allocated
		[time_t]
		public long st_atime;   // time of last access
		[time_t]
		public long st_mtime;   // time of last modification
		[time_t]
		public long st_ctime;   // time of last status change
		public long st_atime_nsec; // Timespec.tv_nsec partner to st_atime
		public long st_mtime_nsec; // Timespec.tv_nsec partner to st_mtime
		public long st_ctime_nsec; // Timespec.tv_nsec partner to st_ctime

		public Timespec st_atim {
			get {
				return new Timespec { tv_sec = st_atime, tv_nsec = st_atime_nsec };
			}
			set {
				st_atime = value.tv_sec;
				st_atime_nsec = value.tv_nsec;
			}
		}

		public Timespec st_mtim {
			get {
				return new Timespec { tv_sec = st_mtime, tv_nsec = st_mtime_nsec };
			}
			set {
				st_mtime = value.tv_sec;
				st_mtime_nsec = value.tv_nsec;
			}
		}

		public Timespec st_ctim {
			get {
				return new Timespec { tv_sec = st_ctime, tv_nsec = st_ctime_nsec };
			}
			set {
				st_ctime = value.tv_sec;
				st_ctime_nsec = value.tv_nsec;
			}
		}

		public override int GetHashCode ()
		{
			return st_dev.GetHashCode () ^
					st_ino.GetHashCode () ^
					st_mode.GetHashCode () ^
					st_nlink.GetHashCode () ^
					st_uid.GetHashCode () ^
					st_gid.GetHashCode () ^
					st_rdev.GetHashCode () ^
					st_size.GetHashCode () ^
					st_blksize.GetHashCode () ^
					st_blocks.GetHashCode () ^
					st_atime.GetHashCode () ^
					st_mtime.GetHashCode () ^
					st_ctime.GetHashCode () ^
					st_atime_nsec.GetHashCode () ^
					st_mtime_nsec.GetHashCode () ^
					st_ctime_nsec.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || obj.GetType () != GetType ())
				return false;
			Stat value = (Stat)obj;
			return value.st_dev == st_dev &&
					value.st_ino == st_ino &&
						value.st_mode == st_mode &&
								value.st_nlink == st_nlink &&
								value.st_uid == st_uid &&
								value.st_gid == st_gid &&
								value.st_rdev == st_rdev &&
								value.st_size == st_size &&
								value.st_blksize == st_blksize &&
								value.st_blocks == st_blocks &&
								value.st_atime == st_atime &&
								value.st_mtime == st_mtime &&
								value.st_ctime == st_ctime &&
								value.st_atime_nsec == st_atime_nsec &&
								value.st_mtime_nsec == st_mtime_nsec &&
								value.st_ctime_nsec == st_ctime_nsec;
		}

		public bool Equals (Stat value)
		{
			return value.st_dev == st_dev &&
					value.st_ino == st_ino &&
					value.st_mode == st_mode &&
					value.st_nlink == st_nlink &&
					value.st_uid == st_uid &&
					value.st_gid == st_gid &&
					value.st_rdev == st_rdev &&
					value.st_size == st_size &&
					value.st_blksize == st_blksize &&
					value.st_blocks == st_blocks &&
					value.st_atime == st_atime &&
					value.st_mtime == st_mtime &&
					value.st_ctime == st_ctime &&
					value.st_atime_nsec == st_atime_nsec &&
					value.st_mtime_nsec == st_mtime_nsec &&
					value.st_ctime_nsec == st_ctime_nsec;
		}

		public static bool operator == (Stat lhs, Stat rhs)
		{
			return lhs.Equals (rhs);
		}

		public static bool operator != (Stat lhs, Stat rhs)
		{
			return !lhs.Equals (rhs);
		}
	}
}
