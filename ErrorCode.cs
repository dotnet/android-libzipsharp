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

namespace Xamarin.ZipSharp
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
		UNKNOWN        = -1,

		/// <summary>
		/// No error
		/// </summary>
		OK             = 0,

		/// <summary>
		/// Multi-disk zip archives not supported
		/// </summary>
		MULTIDISK      = 1,

		/// <summary>
		/// Renaming temporary file failed
		/// </summary>
		RENAME         = 2,

		/// <summary>
		/// Closing zip archive failed
		/// </summary>
		CLOSE          = 3,

		/// <summary>
		/// Seek error
		/// </summary>
		SEEK           = 4,

		/// <summary>
		/// Read error
		/// </summary>
		READ           = 5,

		/// <summary>
		/// Write error
		/// </summary>
		WRITE          = 6,

		/// <summary>
		/// CRC error
		/// </summary>
		CRC            = 7,

		/// <summary>
		/// Containing zip archive was closed
		/// </summary>
		ZIPCLOSED      = 8,

		/// <summary>
		/// No such file
		/// </summary>
		NOENT          = 9,

		/// <summary>
		/// File already exists
		/// </summary>
		EXISTS         = 10,

		/// <summary>
		/// Can't open file
		/// </summary>
		OPEN           = 11,

		/// <summary>
		/// Failure to create temporary file
		/// </summary>
		TMPOPEN        = 12,

		/// <summary>
		/// Zlib error
		/// </summary>
		ZLIB           = 13,

		/// <summary>
		/// Malloc failure
		/// </summary>
		MEMORY         = 14,

		/// <summary>
		/// Entry has been changed
		/// </summary>
		CHANGED        = 15,

		/// <summary>
		/// Compression method not supported
		/// </summary>
		COMPNOTSUPP    = 16,

		/// <summary>
		/// Premature end of file
		/// </summary>
		EOF            = 17,

		/// <summary>
		/// Invalid argument
		/// </summary>
		INVAL          = 18,

		/// <summary>
		/// Not a zip archive
		/// </summary>
		NOZIP          = 19,

		/// <summary>
		/// Internal error
		/// </summary>
		INTERNAL       = 20,

		/// <summary>
		/// Zip archive inconsistent
		/// </summary>
		INCONS         = 21,

		/// <summary>
		/// Can't remove file
		/// </summary>
		REMOVE         = 22,

		/// <summary>
		/// Entry has been deleted
		/// </summary>
		DELETED        = 23,

		/// <summary>
		/// Encryption method not supported
		/// </summary>
		ENCRNOTSUPP    = 24,

		/// <summary>
		/// Read-only archive
		/// </summary>
		RDONLY         = 25,

		/// <summary>
		/// No password provided
		/// </summary>
		NOPASSWD       = 26,

		/// <summary>
		/// Wrong password provided
		/// </summary>
		WRONGPASSWD    = 27,

		/// <summary>
		/// Operation not supported
		/// </summary>
		OPNOTSUPP      = 28,

		/// <summary>
		/// Resource still in use
		/// </summary>
		INUSE          = 29,

		/// <summary>
		/// Tell error
		/// </summary>
		TELL           = 30,
	}
}
