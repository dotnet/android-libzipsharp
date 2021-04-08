//
// ErrorCode.cs
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
	/// <summary>
	/// Error codes. Enumeration members correspond to the <c>ZIP_ER_*</c> macros in the <c>zip.h</c>
	/// header file.
	/// </summary>
	public enum ErrorCode
	{
		/// <summary>
		/// Unknown error code
		/// </summary>
		Unknown        = -1,

		/// <summary>
		/// No error
		/// </summary>
		OK             = 0,

		/// <summary>
		/// Multi-disk zip archives not supported
		/// </summary>
		MultiDisk      = 1,

		/// <summary>
		/// Renaming temporary file failed
		/// </summary>
		Rename         = 2,

		/// <summary>
		/// Closing zip archive failed
		/// </summary>
		Close          = 3,

		/// <summary>
		/// Seek error
		/// </summary>
		Seek           = 4,

		/// <summary>
		/// Read error
		/// </summary>
		Read           = 5,

		/// <summary>
		/// Write error
		/// </summary>
		Write          = 6,

		/// <summary>
		/// CRC error
		/// </summary>
		CRC            = 7,

		/// <summary>
		/// Containing zip archive was closed
		/// </summary>
		ZipClosed      = 8,

		/// <summary>
		/// No such file
		/// </summary>
		NoEnt          = 9,

		/// <summary>
		/// File already exists
		/// </summary>
		Exists         = 10,

		/// <summary>
		/// Can't open file
		/// </summary>
		Open           = 11,

		/// <summary>
		/// Failure to create temporary file
		/// </summary>
		TmpOpen        = 12,

		/// <summary>
		/// Zlib error
		/// </summary>
		Zlib           = 13,

		/// <summary>
		/// Malloc failure
		/// </summary>
		Memory         = 14,

		/// <summary>
		/// Entry has been changed
		/// </summary>
		Changed        = 15,

		/// <summary>
		/// Compression method not supported
		/// </summary>
		CompNotSupp    = 16,

		/// <summary>
		/// Premature end of file
		/// </summary>
		EOF            = 17,

		/// <summary>
		/// Invalid argument
		/// </summary>
		Inval          = 18,

		/// <summary>
		/// Not a zip archive
		/// </summary>
		NoZip          = 19,

		/// <summary>
		/// Internal error
		/// </summary>
		Internal       = 20,

		/// <summary>
		/// Zip archive inconsistent
		/// </summary>
		Incons         = 21,

		/// <summary>
		/// Can't remove file
		/// </summary>
		Remove         = 22,

		/// <summary>
		/// Entry has been deleted
		/// </summary>
		Deleted        = 23,

		/// <summary>
		/// Encryption method not supported
		/// </summary>
		EncrNotSupp    = 24,

		/// <summary>
		/// Read-only archive
		/// </summary>
		RDOnly         = 25,

		/// <summary>
		/// No password provided
		/// </summary>
		NoPasswd       = 26,

		/// <summary>
		/// Wrong password provided
		/// </summary>
		WrongPasswd    = 27,

		/// <summary>
		/// Operation not supported
		/// </summary>
		OptNotSup      = 28,

		/// <summary>
		/// Resource still in use
		/// </summary>
		InUse          = 29,

		/// <summary>
		/// Tell error
		/// </summary>
		Tell           = 30,
	}
}
