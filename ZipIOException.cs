//
// ZipIOException.cs
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
using System.IO;
using System.Runtime.Serialization;

namespace Xamarin.Tools.Zip
{
	/// <summary>
	/// Zip I/O exception.
	/// </summary>
	public class ZipIOException : IOException
	{
		/// <summary>
		/// Native libzip error code, if any.
		/// </summary>
		/// <value>The zip error code.</value>
		public ErrorCode ZipErrorCode { get; private set; } = ErrorCode.OK;

		/// <summary>
		/// Value of the native system <c>errno</c>, on Unix, 0 on Windows. Note that the value is only advisory,
		/// it might not have any meaning, depending on context.
		/// </summary>
		/// <value>errno variable value.</value>
		public int Errno { get; private set; } = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		public ZipIOException ()
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public ZipIOException (string message) : base (message)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="inner">Inner exception.</param>
		public ZipIOException (string message, Exception inner) : base (message, inner)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		/// <param name="info">Serialization info.</param>
		/// <param name="context">Streaming context.</param>
		public ZipIOException (SerializationInfo info, StreamingContext context) : base (info, context)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="errorCode">Error code.</param>
		/// <param name="errno">Errno value.</param>
		public ZipIOException (string message, ErrorCode errorCode, int errno = 0) : this (message)
		{
			CommonInit (errorCode, errno);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="inner">Inner exception.</param>
		/// <param name="errorCode">Error code.</param>
		/// <param name="errno">Errno value.</param>
		public ZipIOException (string message, Exception inner, ErrorCode errorCode, int errno = 0) : this (message, inner)
		{
			CommonInit (errorCode, errno);
		}

		void CommonInit (ErrorCode errorCode, int errno)
		{
			ZipErrorCode = errorCode;
			Errno = errno;
		}
	}
}

