//
// OperatingSystem.cs
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

namespace Xamarin.ZipSharp
{
	/// <summary>
	/// Operating system on which the archive was created. Enu,meration members correspond to
	/// the <c>ZIP_OPSYS_*</c> macros in the <c>zip.h</c> header file.
	/// </summary>
	public enum OperatingSystem : uint
	{
		/// <summary>
		/// DOS
		/// </summary>
		DOS           = 0x00u,

		/// <summary>
		/// Amiga OS
		/// </summary>
		AMIGA         = 0x01u,

		/// <summary>
		/// Open VMS
		/// </summary>
		OPENVMS       = 0x02u,

		/// <summary>
		/// Generic UNIX
		/// </summary>
		UNIX          = 0x03u,

		/// <summary>
		/// VM CMS
		/// </summary>
		VM_CMS        = 0x04u,

		/// <summary>
		/// Atari ST
		/// </summary>
		ATARI_ST      = 0x05u,

		/// <summary>
		/// IBM OS/2
		/// </summary>
		OS_2          = 0x06u,

		/// <summary>
		/// Classic MacOS
		/// </summary>
		MACINTOSH     = 0x07u,

		/// <summary>
		/// IBM SystemZ
		/// </summary>
		Z_SYSTEM      = 0x08u,

		/// <summary>
		/// CP/M
		/// </summary>
		CPM           = 0x09u,

		/// <summary>
		/// Windows using the NTFS filesystem
		/// </summary>
		WINDOWS_NTFS  = 0x0au,

		/// <summary>
		/// MVS
		/// </summary>
		MVS           = 0x0bu,

		/// <summary>
		/// VSE
		/// </summary>
		VSE           = 0x0cu,

		/// <summary>
		/// Acorn RISC
		/// </summary>
		ACORN_RISC    = 0x0du,

		/// <summary>
		/// VFAT filesystem
		/// </summary>
		VFAT          = 0x0eu,

		/// <summary>
		/// Alternate MVS
		/// </summary>
		ALTERNATE_MVS = 0x0fu,

		/// <summary>
		/// BeOS
		/// </summary>
		BEOS          = 0x10u,

		/// <summary>
		/// Tandem
		/// </summary>
		TANDEM        = 0x11u,

		/// <summary>
		/// IBM OS/400
		/// </summary>
		OS_400        = 0x12u,

		/// <summary>
		/// Apple OS X
		/// </summary>
		OS_X          = 0x13u,
	}
}
