//
// EncryptionMethod.cs
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
	/// Encryption method. Enumeration members correspond to the <c>ZIP_EM_*</c> macros
	/// in the <c>zip.h</c> header file.
	/// </summary>
	public enum EncryptionMethod : ushort
	{
		/// <summary>
		/// Not encrypted
		/// </summary>
		None            = 0,

		/// <summary>
		/// Traditional PKWARE encryption
		/// </summary>
		Trad_PKWare     = 1,

		/// <summary>
		/// Strong encryption: DES
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		DES             = 0x6601,

		/// <summary>
		/// Strong encryption: RC2, version &lt; 5.2
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		RC2_Old         = 0x6602,

		/// <summary>
		/// Strong encryption: 3DES (168-bit key)
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		Three_DES_168   = 0x6603,

		/// <summary>
		/// Strong encryption: 3DES (112-bit key)
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		Three_DES_112   = 0x6609,

		/// <summary>
		/// Strong encryption: PKZIP AES (128-bit key)
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		PKZIP_AES_128    = 0x660e,

		/// <summary>
		/// Deprecated alias for <see cref="AES_128"/>
		/// </summary>
		[Obsolete ("Use EncryptionMethod.PKZIP_AES_128")]
		AES_128          = PKZIP_AES_128,

		/// <summary>
		/// Strong encryption: PKZIP AES (192-bit key)
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		PKZIP_AES_192    = 0x660f,

		/// <summary>
		/// Deprecated alias for <see cref="AES_192"/>
		/// </summary>
		[Obsolete ("Use EncryptionMethod.PKZIP_AES_192")]
		AES_192          = PKZIP_AES_192,

		/// <summary>
		/// Strong encryption: AES (256-bit key)
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		PKZIP_AES_256    = 0x6610,

		/// <summary>
		/// Deprecated alias for <see cref="AES_256"/>
		/// </summary>
		[Obsolete ("Use EncryptionMethod.PKZIP_AES_256")]
		AES_256          = PKZIP_AES_256,

		/// <summary>
		/// Strong encryption: RC2, version >= 5.2
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		RC2             = 0x6702,

		/// <summary>
		/// Strong encryption: RC4
		/// <remarks>Not supported by the native libzip yet (as of v1.9.2)</remarks>
		/// </summary>
		RC4             = 0x6801,

		/// <summary>
		/// WinZIP AES encryption (128-bit key)
		/// </summary>
		WINZIP_AES_128  = 0x0101,

		/// <summary>
		/// WinZIP AES encryption (192-bit key)
		/// </summary>
		WINZIP_AES_192  = 0x0102,

		/// <summary>
		/// WinZIP AES encryption (256-bit key)
		/// </summary>
		WINZIP_AES_256  = 0x0103,

		/// <summary>
		/// Unknown algorithm
		/// </summary>
		Unknown         = 0xffff,
	}
}
