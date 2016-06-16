//
// ExtraField.cs
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
	public class ExtraField
	{
		byte [] rawData;

		public ushort ID { get; set; }
		public ushort Length { get; set; }
		public bool Local { get; set; }
		public ushort FieldIndex { get; set; }
		public ulong EntryIndex { get; set; }
		public bool DataValid { get; protected set; } = true;

		public byte [] RawData { 
			get { return rawData; }
			set {
				rawData = value;
				Parse ();
			}
		}

		public ExtraField ()
		{ }

		public ExtraField (ExtraField ef)
		{
			if (ef == null)
				return;

			ID = ef.ID;
			Length = ef.Length;
			Local = ef.Local;
			FieldIndex = ef.FieldIndex;
			EntryIndex = ef.EntryIndex;
			DataValid = ef.DataValid;
			RawData = ef.RawData;
		}

		protected virtual void Parse ()
		{
			DataValid = true;
		}

		protected ulong BytesToUnsignedLong (byte [] data, int startIndex)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex > data.Length || startIndex + 7 >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));

			return (ulong)((data [startIndex + 7] << 56) |
			               (data [startIndex + 6] << 48) |
			               (data [startIndex + 5] << 40) |
			               (data [startIndex + 4] << 32) |
			               (data [startIndex + 3] << 24) |
			               (data [startIndex + 2] << 16) |
			               (data [startIndex + 1] << 8) |
			               data [startIndex]);
		}

		protected uint BytesToUnsignedInt (byte [] data, int startIndex)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex > data.Length || startIndex + 3 >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));
			
			return (uint)((data [startIndex + 3] << 24) |
						  (data [startIndex + 2] << 16) |
						  (data [startIndex + 1] << 8) |
						  data [startIndex]);
		}

		protected ushort BytesToUnsignedShort (byte [] data, int startIndex)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			if (startIndex < 0 || startIndex > data.Length || startIndex + 1 >= data.Length)
				throw new ArgumentOutOfRangeException (nameof (startIndex));
			
			return (ushort)((data [startIndex + 1] << 8) |
							data [startIndex]);
		}
	}
}

