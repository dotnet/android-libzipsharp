//
// StatFlags.cs
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
	/// Flags to indicate which fields of the zip stat structure are valid. Enumeration members
	/// correspond to the <c>ZIP_STAT_*</c> macros in the <c>zip.h</c> header file.
	/// </summary>
	[Flags]
	public enum StatFlags : uint
	{
		/// <summary>
		/// The Name field is valid
		/// </summary>
		NAME                   = 0x0001u,

		/// <summary>
		/// The Index field is valid
		/// </summary>
		INDEX                  = 0x0002u,

		/// <summary>
		/// The Size field is valid
		/// </summary>
		SIZE                   = 0x0004u,

		/// <summary>
		/// The CompressedSize field is valid
		/// </summary>
		COMP_SIZE              = 0x0008u,

		/// <summary>
		/// The MTime field is valid
		/// </summary>
		MTIME                  = 0x0010u,

		/// <summary>
		/// The CRC field is valid
		/// </summary>
		CRC                    = 0x0020u,

		/// <summary>
		/// The CompressionMethod field is valid
		/// </summary>
		COMP_METHOD            = 0x0040u,

		/// <summary>
		/// The EncryptionMethod field is valid
		/// </summary>
		ENCRYPTION_METHOD      = 0x0080u,

		/// <summary>
		/// The Flags field is valid
		/// </summary>
		FLAGS                  = 0x0100u,
	}
}
