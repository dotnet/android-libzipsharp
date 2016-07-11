//
// FilePermissions.cs
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
	// mode_t
	[Flags]
	[Map]
	[CLSCompliant (false)]
	enum FilePermissions : uint
	{
		S_ISUID = 0x0800, // Set user ID on execution
		S_ISGID = 0x0400, // Set group ID on execution
		S_ISVTX = 0x0200, // Save swapped text after use (sticky).
		S_IRUSR = 0x0100, // Read by owner
		S_IWUSR = 0x0080, // Write by owner
		S_IXUSR = 0x0040, // Execute by owner
		S_IRGRP = 0x0020, // Read by group
		S_IWGRP = 0x0010, // Write by group
		S_IXGRP = 0x0008, // Execute by group
		S_IROTH = 0x0004, // Read by other
		S_IWOTH = 0x0002, // Write by other
		S_IXOTH = 0x0001, // Execute by other

		S_IRWXG = (S_IRGRP | S_IWGRP | S_IXGRP),
		S_IRWXU = (S_IRUSR | S_IWUSR | S_IXUSR),
		S_IRWXO = (S_IROTH | S_IWOTH | S_IXOTH),
		ACCESSPERMS = (S_IRWXU | S_IRWXG | S_IRWXO), // 0777
		ALLPERMS = (S_ISUID | S_ISGID | S_ISVTX | S_IRWXU | S_IRWXG | S_IRWXO), // 07777
		DEFFILEMODE = (S_IRUSR | S_IWUSR | S_IRGRP | S_IWGRP | S_IROTH | S_IWOTH), // 0666

		// Device types
		// Why these are held in "mode_t" is beyond me...
		S_IFMT = 0xF000, // Bits which determine file type
		[Map (SuppressFlags = "S_IFMT")]
		S_IFDIR = 0x4000, // Directory
		[Map (SuppressFlags = "S_IFMT")]
		S_IFCHR = 0x2000, // Character device
		[Map (SuppressFlags = "S_IFMT")]
		S_IFBLK = 0x6000, // Block device
		[Map (SuppressFlags = "S_IFMT")]
		S_IFREG = 0x8000, // Regular file
		[Map (SuppressFlags = "S_IFMT")]
		S_IFIFO = 0x1000, // FIFO
		[Map (SuppressFlags = "S_IFMT")]
		S_IFLNK = 0xA000, // Symbolic link
		[Map (SuppressFlags = "S_IFMT")]
		S_IFSOCK = 0xC000, // Socket
	}

}
