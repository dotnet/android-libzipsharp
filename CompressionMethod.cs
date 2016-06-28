//
// CompressionMethod.cs
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
	/// Archive compression method. Enumeration members correspond to the <c>ZIP_CM_*</c> macros in
	/// the <c>zip.h</c> header file.
	/// </summary>
	public enum CompressionMethod : short
	{
		/// <summary>
		/// Unknown compression method (not present in the native libzip)
		/// </summary>
		Unknown        = -2,

		/// <summary>
		/// Better of deflate or store
		/// </summary>
		Default        = -1,

		/// <summary>
		/// Stored (uncompressed)
		/// </summary>
		Store          = 0,

		/// <summary>
		/// Shrunk
		/// </summary>
		Shrink         = 1,

		/// <summary>
		/// Reduced with factor 1
		/// </summary>
		Reduce_1       = 2,

		/// <summary>
		/// Reduced with factor 2
		/// </summary>
		Reduce_2       = 3,

		/// <summary>
		/// Reduced with factor 3
		/// </summary>
		Reduce_3       = 4,

		/// <summary>
		/// Reduced with factor 4
		/// </summary>
		Reduce_4       = 5,

		/// <summary>
		/// Imploded
		/// </summary>
		Implode        = 6,

		/* 7 - Reserved for Tokenizing compression algorithm */

		/// <summary>
		/// Deflated
		/// </summary>
		Deflate        = 8,

		/// <summary>
		/// Deflate (64-bit)
		/// </summary>
		Deflate64      = 9,

		/// <summary>
		/// PKWARE imploding
		/// </summary>
		PKWare_Implode = 10,

		/* 11 - Reserved by PKWARE */

		/// <summary>
		/// Compressed using BZIP2 algorithm
		/// </summary>
		Bzip2          = 12,

		/* 13 - Reserved by PKWARE */

		/// <summary>
		/// LZMA (EFS)
		/// </summary>
		LZMA           = 14,

		/* 15-17 - Reserved by PKWARE */

		/// <summary>
		/// Compressed using IBM TERSE (new)
		/// </summary>
		Terse          = 18,

		/// <summary>
		/// IBM LZ77 z Architecture (PFS)
		/// </summary>
		LZ77           = 19,

		/// <summary>
		/// WavPack compressed data
		/// </summary>
		WavPack        = 97,

		/// <summary>
		/// PPMd version I, Rev 1
		/// </summary>
		PPMD           = 98,
	}
}
