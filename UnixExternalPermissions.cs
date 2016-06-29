//
// UnixExternalPermissions.cs
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
	// The definitions are taken from InfoZip's unzip sources, from the zipinfo.c
	// file. unzip is distributed under the InfoZip license based on BSD
	// 
	// http://www.info-zip.org/pub/infozip/license.html
	//
	[Flags]
	enum UnixExternalPermissions : uint
	{
		/* Define OS-specific attributes for use on ALL platforms--the S_xxxx
		 * versions of these are defined differently (or not defined) by different
		 * compilers and operating systems. 
		*/
		IFMT   = 0xF000, /* 0170000     Unix file type mask */

		IFREG  = 0x8000, /* 0100000     Unix regular file */
		IFSOCK = 0xC000, /* 0140000     Unix socket (BSD, not SysV or Amiga) */
		IFLNK  = 0xA000, /* 0120000     Unix symbolic link (not SysV, Amiga) */
		IFBLK  = 0x6000, /* 0060000     Unix block special       (not Amiga) */
		IFDIR  = 0x4000, /* 0040000     Unix directory */
		IFCHR  = 0x2000, /* 0020000     Unix character special   (not Amiga) */
		IFIFO  = 0x1000, /* 0010000     Unix fifo    (BCC, not MSC or Amiga) */

		ISUID  = 0x0800, /* 04000       Unix set user id on execution */
		ISGID  = 0x0400, /* 02000       Unix set group id on execution */
		ISVTX  = 0x0200, /* 01000       Unix directory permissions control */
		IRWXU  = 0x01C0, /* 00700       Unix read, write, execute: owner */
		IRUSR  = 0x0100, /* 00400       Unix read permission: owner */
		IWUSR  = 0x0080, /* 00200       Unix write permission: owner */
		IXUSR  = 0x0040, /* 00100       Unix execute permission: owner */
		IRWXG  = 0x0038, /* 00070       Unix read, write, execute: group */
		IRGRP  = 0x0020, /* 00040       Unix read permission: group */
		IWGRP  = 0x0010, /* 00020       Unix write permission: group */
		IXGRP  = 0x0008, /* 00010       Unix execute permission: group */
		IRWXO  = 0x0007, /* 00007       Unix read, write, execute: other */
		IROTH  = 0x0004, /* 00004       Unix read permission: other */
		IWOTH  = 0x0002, /* 00002       Unix write permission: other */
		IXOTH  = 0x0001, /* 00001       Unix execute permission: other */

		IMODE  = ISUID | ISGID | ISVTX | IRWXU | IRUSR | IWUSR | IXUSR | IRWXG | IRGRP | IWGRP | IXGRP | IRWXO | IROTH | IWOTH | IXOTH,
	}
}

