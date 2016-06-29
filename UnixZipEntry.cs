//
// UnixZipEntry.cs
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

namespace Xamarin.Tools.Zip
{
	public partial class UnixZipEntry : ZipEntry
	{
		/// <summary>
		/// Unix permissions for the entry
		/// </summary>
		/// <value>The permissions.</value>
		public uint Permissions {
			get { return GetPermissions (); }
			set { SetFilePermissions (value); }
		}

		/// <summary>
		/// Unix User ID for the entry, if any
		/// </summary>
		/// <value>Entry user ID.</value>
		public ulong? UID { get; set; }

		/// <summary>
		/// Unix Group ID for the entry, if any
		/// </summary>
		/// <value>Entry group ID</value>
		public ulong? GID { get; set; }

		/// <summary>
		/// Gets the last access time of the ZIP entry. The value is in the UTC timezone.
		/// </summary>
		/// <value>Last access time or <see cref="DateTime.MinValue"/> if invalid/unset</value>
		public DateTime AccessTime { get; internal set; } = DateTime.MinValue;

		/// <summary>
		/// Gets the creation time of the ZIP entry. The value is in the UTC timezone.
		/// </summary>
		/// <value>Creation time or <see cref="DateTime.MinValue"/> if invalid/unset</value>
		public DateTime CreationTime { get; internal set; } = DateTime.MinValue;

		/// <summary>
		/// Indicates whether this entry represents a symbolic link
		/// </summary>
		/// <value><c>true</c> for a symbolic link</value>
		public bool IsSymlink { get; internal set; }

		internal UnixZipEntry (ZipArchive archive, Native.zip_stat_t stat)
			: base (archive, stat)
		{}

		partial void SetFilePermissions (uint value);
	}
}

