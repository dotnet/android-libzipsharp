//
// OperationFlags.cs
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
	/// Operation flags, used by NameLocate, Fopen, Stat etc. Enumeration names correspond to the
	/// <c>ZIP_FL_*</c> constants in <c>zip.h</c>
	/// </summary>
	[Flags]
	public enum OperationFlags : uint
	{
		/// <summary>
		/// No flags are set/used
		/// </summary>
		None             = 0u,

		/// <summary>
		/// Ignore case on name lookup
		/// </summary>
		NoCase           = 1u,

		/// <summary>
		/// Ignore directory component
		/// </summary>
		NoDir            = 2u,

		/// <summary>
		/// Read compressed data
		/// </summary>
		Compressed       = 4u,

		/// <summary>
		/// Use original data, ignoring changes
		/// </summary>
		Unchanged        = 8u,

		/// <summary>
		/// Force recompression of data
		/// </summary>
		Recompress       = 16u,

		/// <summary>
		/// Read encrypted data (implies COMPRESSED)
		/// </summary>
		Encrypted        = 32u,

		/// <summary>
		/// Guess string encoding (is default)
		/// </summary>
		Enc_Guess        = 0u,

		/// <summary>
		/// Get unmodified string
		/// </summary>
		Enc_Raw          = 64u,

		/// <summary>
		/// Follow specification strictly
		/// </summary>
		Enc_Strict       = 128u,

		/// <summary>
		/// In local header
		/// </summary>
		Local            = 256u,

		/// <summary>
		/// In central directory
		/// </summary>
		Central          = 512u,

		/// <summary>
		/// String is UTF-8 encoded
		/// </summary>
		Enc_UTF_8        = 2048u,

		/// <summary>
		/// String is CP437 encoded
		/// </summary>
		Enc_CP437        = 4096u,

		/// <summary>
		/// Zip_file_add: if file with name exists, overwrite (replace) it
		/// </summary>
		Overwrite        = 8192u,
	}
}

