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
		Open,

		/// <summary>
		/// Read data into the buffer data of size len. Return the number of bytes placed into data on success.
		/// </summary>
		Read,

		/// <summary>
		/// Reading is done
		/// </summary>
		Close,

		/// <summary>
		/// Get meta information
		/// </summary>
		Stat,

		/// <summary>
		/// Get error information
		/// </summary>
		Error,

		/// <summary>
		/// Cleanup and free resources
		/// </summary>
		Free,

		/// <summary>
		/// Set position for reading
		/// </summary>
		Seek,

		/// <summary>
		/// Get read position
		/// </summary>
		Tell,

		/// <summary>
		/// Prepare for writing
		/// </summary>
		BeginWrite,

		/// <summary>
		/// Writing is done
		/// </summary>
		CommitWrite,

		/// <summary>
		/// Discard written changes
		/// </summary>
		RollbackWrite,

		/// <summary>
		/// Write data
		/// </summary>
		Write,

		/// <summary>
		/// Set position for writing
		/// </summary>
		SeekWrite,

		/// <summary>
		/// Get write position
		/// </summary>
		TellWrite,

		/// <summary>
		/// Check whether source supports command
		/// </summary>
		Supports,

		/// <summary>
		/// Remove the underlying file. This is called if a zip archive is empty when closed.
		/// </summary>
		Remove
	}
}
