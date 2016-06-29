//
// ExtraField_InfoZipUnixType2.cs
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
	class ExtraField_InfoZipUnixType2 : ExtraField_UnixIDBase
	{
		public ExtraField_InfoZipUnixType2 ()
		{ }

		public ExtraField_InfoZipUnixType2 (ExtraField ef) : base (ef)
		{ }

		//
		// Local-header version:
		//         Value         Size        Description
		//         -----         ----        -----------
		// (Unix2) 0x7855        Short       tag for this extra block type ("Ux")
		//         TSize         Short       total data size for this block (4)
		//         UID           Short       Unix user ID
		//         GID           Short       Unix group ID
		//
		// Central-header version:
		//
		//         Value         Size        Description
		//         -----         ----        -----------
		// (Unix2) 0x7855        Short       tag for this extra block type ("Ux")
		//         TSize         Short       total data size for this block (0)
		//
		// The data size of the central-header version is zero; it is used
		// solely as a flag that UID/GID info is present in the local-header
		// extra field.
		//
		protected override void Parse ()
		{
			base.Parse ();

			DataValid = false;
			if (!Local)
				return;
			
			byte [] data = RawData;
			if (data?.Length < 4)
				return;

			UID = BytesToUnsignedShort (data, 0);
			GID = BytesToUnsignedShort (data, 2);
			DataValid = true;
		}
	}
}

