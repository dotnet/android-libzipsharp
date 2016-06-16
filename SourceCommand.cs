//
// SourceCommand.cs
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
	/// Commands sent to user-defined ZIP source callback function/method. Enumeration
	/// members correspond to the members of the <c>zip_source_cmd</c> enum defined in
	/// the <c>zip.h</c> header file (the <c>ZIP_SOURCE_*</c> symbols).
	/// </summary>
	public enum SourceCommand
	{
		/// <summary>
		/// Prepare for reading.
		/// </summary>
		ZIP_SOURCE_OPEN,

		/// <summary>
		/// Read data into the buffer data of size len. Return the number of bytes placed into data on success.
		/// </summary>
		ZIP_SOURCE_READ,            /*  */

		/// <summary>
		/// Reading is done
		/// </summary>
		ZIP_SOURCE_CLOSE,           /*  */

		/// <summary>
		/// Get meta information
		/// </summary>
		ZIP_SOURCE_STAT,            /*  */

		/// <summary>
		/// Get error information
		/// </summary>
		ZIP_SOURCE_ERROR,           /*  */

		/// <summary>
		/// Cleanup and free resources
		/// </summary>
		ZIP_SOURCE_FREE,            /*  */

		/// <summary>
		/// Set position for reading
		/// </summary>
		ZIP_SOURCE_SEEK,            /*  */

		/// <summary>
		/// Get read position
		/// </summary>
		ZIP_SOURCE_TELL,            /*  */

		/// <summary>
		/// Prepare for writing
		/// </summary>
		ZIP_SOURCE_BEGIN_WRITE,     /*  */

		/// <summary>
		/// Writing is done
		/// </summary>
		ZIP_SOURCE_COMMIT_WRITE,    /*  */

		/// <summary>
		/// Discard written changes
		/// </summary>
		ZIP_SOURCE_ROLLBACK_WRITE,  /*  */

		/// <summary>
		/// Write data
		/// </summary>
		ZIP_SOURCE_WRITE,           /*  */

		/// <summary>
		/// Set position for writing
		/// </summary>
		ZIP_SOURCE_SEEK_WRITE,      /*  */

		/// <summary>
		/// Get write position
		/// </summary>
		ZIP_SOURCE_TELL_WRITE,      /*  */

		/// <summary>
		/// Check whether source supports command
		/// </summary>
		ZIP_SOURCE_SUPPORTS,        /*  */

		/// <summary>
		/// Remove the underlying file. This is called if a zip archive is empty when closed.
		/// </summary>
		ZIP_SOURCE_REMOVE
	}
}
