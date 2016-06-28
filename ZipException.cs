//
// ZipException.cs
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
using System.Runtime.Serialization;

namespace Xamarin.ZipSharp
{
	/// <summary>
	/// A generic ZIP library exception
	/// </summary>
	public class ZipException : Exception
	{
		/// <summary>
		/// Native libzip error code, if any.
		/// </summary>
		/// <value>The zip error code.</value>
		public ErrorCode ZipErrorCode { get; private set; } = ErrorCode.Unknown;

		/// <summary>
		/// Gets the raw ZIP error code. Consult this value if <see cref="ZipErrorCode"/> is set
		/// to <see cref="ErrorCode.Unknown"/>
		/// </summary>
		/// <value>The raw zip error code.</value>
		public int RawZipErrorCode { get; private set; } = -1;

		/// <summary>
		/// Gets the system error, if any.
		/// </summary>
		/// <value>The system error.</value>
		public int SystemError { get; private set; } = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipIOException"/> class.
		/// </summary>
		public ZipException ()
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public ZipException (string message) : base (message)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="inner">Inner exception.</param>
		public ZipException (string message, Exception inner) : base (message, inner)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipException"/> class.
		/// </summary>
		/// <param name="info">Serialization info.</param>
		/// <param name="context">Streaming context.</param>
		public ZipException (SerializationInfo info, StreamingContext context) : base (info, context)
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="zip_error">Error code.</param>
		/// <param name="system_error">System error code.</param>
		public ZipException (string message, int zip_error, int system_error = -1) : this (message)
		{
			CommonInit (zip_error, system_error);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="inner">Inner exception.</param>
		/// <param name="zip_error">Error code.</param>
		/// <param name="system_error">System error code.</param>
		public ZipException (string message, Exception inner, int zip_error, int system_error = -1) : this (message, inner)
		{
			CommonInit (zip_error, system_error);
		}

		void CommonInit (int errorCode, int system_error)
		{
			ErrorCode ec = ErrorCode.Unknown;
			if (Enum.IsDefined (typeof (ErrorCode), errorCode))
				ec = (ErrorCode)errorCode;

			RawZipErrorCode = errorCode;
			ZipErrorCode = ec;
			SystemError = system_error;
		}
	}
}

