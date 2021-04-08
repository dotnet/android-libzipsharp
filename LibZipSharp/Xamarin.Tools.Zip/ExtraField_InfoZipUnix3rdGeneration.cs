//
// ExtraField_InfoZipUnix3rdGeneration.cs
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
	class ExtraField_InfoZipUnix3rdGeneration : ExtraField_UnixIDBase
	{
		public ExtraField_InfoZipUnix3rdGeneration ()
		{ }

		public ExtraField_InfoZipUnix3rdGeneration (ExtraField ef) : base (ef)
		{ }

		//
		// Value         Size        Description
		// -----         ----        -----------
		// (UnixN)       0x7875      Short tag for this extra block type ("ux")
		// TSize         Short       total data size for this block
		// Version       1 byte      version of this extra field, currently 1
		// UIDSize       1 byte      Size of UID field
		// UID           Variable    UID for this entry
		// GIDSize       1 byte      Size of GID field
		// GID           Variable    GID for this entry
		//
		protected override void Parse ()
		{
			base.Parse ();

			DataValid = false;
			byte [] data = RawData;
			if (data?.Length < 5)
				return;

			if (data [0] > 1) // version
				return;

			ulong id;
			byte size = data [1];
			int index = 2;
			if (GetID (size, index, data, out id)) {
				UID = id;
			}
			index += size;

			size = data [index++];
			if (GetID (size, index, data, out id)) {
				GID = id;
			}

			DataValid = true;
		}

		bool GetID (byte size, int index, byte[] data, out ulong id)
		{
			switch (size) {
				case 1:
					id = data [2];
					return true;

				case 2:
					id = BytesToUnsignedShort (data, index);
					return true;

				case 4:
					id = BytesToUnsignedInt (data, index);
					return true;

				case 8:
					id = BytesToUnsignedLong (data, index);
					return true;
			}

			id = 0;
			return false;
		}
	}
}

