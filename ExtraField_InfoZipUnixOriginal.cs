//
// ExtraField_InfoZipUnixOriginal.cs
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
	class ExtraField_InfoZipUnixOriginal : ExtraField
	{
		public DateTime AccessTime { get; internal set; } = DateTime.MinValue;
		public DateTime ModificationTime { get; internal set; } = DateTime.MinValue;
		public ushort? UID { get; internal set; }
		public ushort? GID { get; internal set; }

		public ExtraField_InfoZipUnixOriginal ()
		{ }

		public ExtraField_InfoZipUnixOriginal (ExtraField ef) : base (ef)
		{ }

		// Local-header version:
		//
		//         Value         Size        Description
		//         -----         ----        -----------
		// (Unix1) 0x5855        Short       tag for this extra block type ("UX")
		//         TSize         Short       total data size for this block
		//         AcTime        Long        time of last access (UTC/GMT)
		//         ModTime       Long        time of last modification (UTC/GMT)
		//         UID           Short       Unix user ID (optional)
		//         GID           Short       Unix group ID (optional)
		//
		// Central-header version:
		//
		//         Value         Size        Description
		//         -----         ----        -----------
		// (Unix1) 0x5855        Short       tag for this extra block type ("UX")
		//         TSize         Short       total data size for this block
		//         AcTime        Long        time of last access (GMT/UTC)
		//         ModTime       Long        time of last modification (GMT/UTC)
		//
		// Note that what we get here is JUST THE DATA - without the ID and TSize fields!
		protected override void Parse ()
		{
			base.Parse ();

			ModificationTime = DateTime.MinValue;
			AccessTime = DateTime.MinValue;
			DataValid = false;

			byte [] data = RawData;
			if (data?.Length < 8)
				return;

			int index = 0;
			ModificationTime = Utilities.DateTimeFromUnixTime (BytesToUnsignedInt (data, index));
			index += 4;

			AccessTime = Utilities.DateTimeFromUnixTime (BytesToUnsignedInt (data, index));
			index += 4;

			if (!Local || data.Length <= 8)
				return;

			UID = BytesToUnsignedShort (data, index);
			index += 2;
			GID = BytesToUnsignedShort (data, index);
		}
	}
}

