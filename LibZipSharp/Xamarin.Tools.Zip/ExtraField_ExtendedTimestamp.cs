//
// ExtraField_ExtendedTimestamp.cs
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

namespace Xamarin.Tools.Zip
{
	class ExtraField_ExtendedTimestamp : ExtraField
	{
		public DateTime ModificationTime { get; internal set; } = DateTime.MinValue;
		public DateTime AccessTime { get; internal set; } = DateTime.MinValue;
		public DateTime CreationTime { get; internal set; } = DateTime.MinValue;

		public ExtraField_ExtendedTimestamp ()
		{}

		public ExtraField_ExtendedTimestamp (ExtraField ef) : base (ef)
		{}

		public ExtraField_ExtendedTimestamp (ZipEntry entry, short fieldIndex, DateTime? createTime = null, DateTime? accessTime = null, DateTime? modificationTime = null, bool local = true)
		{
			ModificationTime = modificationTime ?? DateTime.MinValue;
			CreationTime = createTime ?? DateTime.MinValue;
			AccessTime = accessTime ?? DateTime.MinValue;
			Local = local;
			ID = KnownExtraFields.ExtendedTimestamp;
			EntryIndex = entry.Index;
			if (modificationTime.HasValue)
				entry.ModificationTime = modificationTime.Value;
		}

		// Local-header version:
		//
		//          Value         Size        Description
		//          -----         ----        -----------
		//  (time)  0x5455        Short       tag for this extra block type ("UT")
		//		    TSize         Short       total data size for this block
		//          Flags         Byte        info bits
		//          (ModTime)     Long        time of last modification (UTC/GMT)
		//          (AcTime)      Long        time of last access (UTC/GMT)
		//          (CrTime)      Long        time of original creation (UTC/GMT)
		//
		// Central-header version:
		//
		//          Value         Size        Description
		//          -----         ----        -----------
		//  (time)  0x5455        Short       tag for this extra block type ("UT")
		//          TSize         Short       total data size for this block
		//          Flags         Byte        info bits (refers to local header!)
		//          (ModTime)     Long        time of last modification (UTC/GMT)
		//
		// The lower three bits of Flags in both headers indicate which timetamps are 
		// present in the LOCAL extra field:
		//		bit 0           if set, modification time is present
		//		bit 1           if set, access time is present
		//		bit 2           if set, creation time is present
		//		bits 3-7        reserved for additional timestamps; not set
		//
		// Note that what we get here is JUST THE DATA - without the ID and TSize fields!
		protected override void Parse ()
		{
			base.Parse ();

			ModificationTime = DateTime.MinValue;
			AccessTime = DateTime.MinValue;
			CreationTime = DateTime.MinValue;
			DataValid = false;

			byte [] data = RawData;
			if (data?.Length < 5)
				return;
			byte flags = data [0];
			bool modTimePresent = (flags & 0x01) == 0x01;
			bool accTimePresent = (flags & 0x02) == 0x02;
			bool createTimePresent = (flags & 0x04) == 0x04;

			int expectedLength = 1; // Just the flags field - one byte
			if (Local) {
				if (modTimePresent)
					expectedLength += 4;
				if (accTimePresent)
					expectedLength += 4;
				if (createTimePresent)
					expectedLength += 4;
			} else {
				if (modTimePresent)
					expectedLength += 4;
			}

			if (data.Length != expectedLength) {
				DataValid = false;
				return;
			}

			if (expectedLength == 1)
				return;

			int index = 1;
			if (modTimePresent) {
				ModificationTime = Utilities.DateTimeFromUnixTime (BytesToUnsignedInt (data, index));
				index += 4;
			}

			if (accTimePresent) {
				AccessTime = Utilities.DateTimeFromUnixTime (BytesToUnsignedInt (data, index));
				index += 4;
			}

			if (createTimePresent)
				CreationTime = Utilities.DateTimeFromUnixTime (BytesToUnsignedInt (data, index));

			DataValid = true;
		}

		// Local-header version:
		//
		//          Value         Size        Description
		//          -----         ----        -----------
		//  (time)  0x5455        Short       tag for this extra block type ("UT")
		//		    TSize         Short       total data size for this block
		//          Flags         Byte        info bits
		//          (ModTime)     Long        time of last modification (UTC/GMT)
		//          (AcTime)      Long        time of last access (UTC/GMT)
		//          (CrTime)      Long        time of original creation (UTC/GMT)
		//
		// Central-header version:
		//
		//          Value         Size        Description
		//          -----         ----        -----------
		//  (time)  0x5455        Short       tag for this extra block type ("UT")
		//          TSize         Short       total data size for this block
		//          Flags         Byte        info bits (refers to local header!)
		//          (ModTime)     Long        time of last modification (UTC/GMT)
		//
		// The lower three bits of Flags in both headers indicate which timetamps are 
		// present in the LOCAL extra field:
		//		bit 0           if set, modification time is present
		//		bit 1           if set, access time is present
		//		bit 2           if set, creation time is present
		//		bits 3-7        reserved for additional timestamps; not set
		//
		// Note that what we get here is JUST THE DATA - without the ID and TSize fields!
		internal override void Encode()
		{
			base.Encode ();

			DataValid = false;

			MemoryStream data = new MemoryStream ();
			byte flags = 0;
			int expectedLength = 1; // Just the flags field - one byte
			if (ModificationTime != DateTime.MinValue) {
				flags |= 0x01;
				expectedLength += 4;
			}
			if (AccessTime != DateTime.MinValue && Local) {
				flags |= 0x02;
				expectedLength += 4;
			}
			if (CreationTime != DateTime.MinValue && Local) {
				flags |= 0x04;
				expectedLength += 4;
			}
			data.WriteByte (flags);
			if (ModificationTime != DateTime.MinValue) {
				byte[] modDate = UnsignedIntToBytes ((uint)Utilities.UnixTimeFromDateTime (ModificationTime));
				data.Write (modDate, 0, modDate.Length);
			}
			if (AccessTime != DateTime.MinValue && Local) {
				byte[] accDate = UnsignedIntToBytes ((uint)Utilities.UnixTimeFromDateTime (AccessTime));
				data.Write (accDate, 0, accDate.Length);
			}
			if (CreationTime != DateTime.MinValue && Local) {
				byte[] createDate = UnsignedIntToBytes ((uint)Utilities.UnixTimeFromDateTime (CreationTime));
				data.Write (createDate, 0, createDate.Length);
			}

			if (data.Length != expectedLength) {
				DataValid = false;
				return;
			}
			Length = (ushort)data.Length;
			RawData = data.ToArray ();
		}
	}
}
