//
// KnownExtraFields.cs
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
	/// Extra fields as described in the ZIP source code and documentation. This list 
	/// is compiled from the proginfo/extrafld.txt found in the Unzip 6.0 distribution
	/// </summary>
	public static class KnownExtraFields
	{
		/// <summary>
		/// Zip64 extended information extra field
		/// </summary>
		public const ushort Zip64ExtendedInformation = 0x0001;

		/// <summary>
		/// AV Info
		/// </summary>
		public const ushort AVInfo = 0x0007;

		/// <summary>
		/// Reserved for extended language encoding data (PFS)
		/// </summary>
		public const ushort ReservedPFS = 0x0008;

		/// <summary>
		/// OS/2 extended attributes (also Info-ZIP)
		/// </summary>
		public const ushort OS2ExtendedAttributes = 0x0009;

		/// <summary>
		/// NTFS (Win9x/WinNT FileTimes)
		/// </summary>
		public const ushort NTFSFileTimes = 0x000a;

		/// <summary>
		/// OpenVMS (also Info-ZIP)
		/// </summary>
		public const ushort OpenVMS = 0x000c;

		/// <summary>
		/// UNIX
		/// </summary>
		public const ushort Unix = 0x000d;

		/// <summary>
		/// Reserved for file stream and fork descriptors
		/// </summary>
		public const ushort ReservedFileAndForDescriptors = 0x000e;

		/// <summary>
		/// Patch Descriptor
		/// </summary>
		public const ushort PatchDescriptor = 0x000f;

		/// <summary>
		/// PKCS#7 Store for X.509 Certificates
		/// </summary>
		public const ushort X509PKCS7CertificateStore = 0x0014;

		/// <summary>
		/// X.509 Certificate ID and Signature for individual file
		/// </summary>
		public const ushort X509FileCertificateIDAndSignature = 0x0015;

		/// <summary>
		/// X.509 Certificate ID for Central Directory
		/// </summary>
		public const ushort X509CentralDirectoryCertificateID = 0x0016;

		/// <summary>
		/// Strong Encryption Header
		/// </summary>
		public const ushort StrongEncryptionHeader = 0x0017;

		/// <summary>
		/// Record Management Controls
		/// </summary>
		public const ushort RecordManagementControls = 0x0018;

		/// <summary>
		/// PKCS#7 Encryption Recipient Certificate List
		/// </summary>
		public const ushort PKCS7RecipientCertificateList = 0x0019;

		/// <summary>
		/// IBM S/390 (Z390), AS/400 (I400) attributes - uncompressed
		/// </summary>
		public const ushort IBMS390AttributesUncompressed = 0x0065;

		/// <summary>
		/// Reserved for IBM S/390 (Z390), AS/400 (I400) attributes - compressed
		/// </summary>
		public const ushort IBMS390AttributesCompressed = 0x0066;

		/// <summary>
		/// POSZIP 4690 (reserved)
		/// </summary>
		public const ushort ReservedPOSZIP4690 = 0x4690;

		// The Header ID mappings defined by Info-ZIP and third parties are:

		/// <summary>
		/// Info-ZIP Macintosh (old, J.Lee)
		/// </summary>
		public const ushort InfoZipOldMacintosh = 0x07c8;

		/// <summary>
		/// ZipIt Macintosh (first version)
		/// </summary>
		public const ushort ZipItMacintoshFirstVersion = 0x2605;

		/// <summary>
		/// ZipIt Macintosh v 1.3.5 and newer (w/o full filename)
		/// </summary>
		public const ushort ZipItMacintoshNoFullName = 0x2705;

		/// <summary>
		/// ZipIt Macintosh 1.3.5+
		/// </summary>
		public const ushort ZipItMacintosh = 0x2805;

		/// <summary>
		/// Info-ZIP Macintosh (new, D.Haase's 'Mac3' field)
		/// </summary>
		public const ushort InfoZipMacintoshNew = 0x334d;

		/// <summary>
		/// Tandem NSK
		/// </summary>
		public const ushort TandemNSK = 0x4154;

		/// <summary>
		/// Acorn/SparkFS (David Pilling)
		/// </summary>
		public const ushort AcorSparkFS = 0x4341;

		/// <summary>
		/// Windows NT security descriptor (binary ACL)
		/// </summary>
		public const ushort WindowsNTBinaryACL = 0x4453;

		/// <summary>
		/// VM/CMS
		/// </summary>
		public const ushort VMCMS = 0x4704;

		/// <summary>
		/// MVS
		/// </summary>
		public const ushort MVS = 0x470f;

		/// <summary>
		/// Theos, old inofficial port
		/// </summary>
		public const ushort TheosOld = 0x4854;

		/// <summary>
		/// FWKCS MD5
		/// </summary>
		public const ushort FWKCSMD5 = 0x4b46;

		/// <summary>
		/// OS/2 access control list (text ACL)
		/// </summary>
		public const ushort OS2TextACL = 0x4c41;

		/// <summary>
		/// Info-ZIP OpenVMS (obsolete)
		/// </summary>
		public const ushort InfoZipOpenVMS = 0x4d49;

		/// <summary>
		/// Macintosh SmartZIP, by Macro Bambini
		/// </summary>
		public const ushort MacintoshSmartZIP = 0x4d63;

		/// <summary>
		/// Xceed original location extra field
		/// </summary>
		public const ushort XceedOriginalLocation = 0x4f4c;

		/// <summary>
		/// AOS/VS (binary ACL)
		/// </summary>
		public const ushort AOSVSBinaryACL = 0x5356;

		/// <summary>
		/// Extended timestamp
		/// </summary>
		public const ushort ExtendedTimestamp = 0x5455;

		/// <summary>
		/// Xceed unicode extra field
		/// </summary>
		public const ushort XceedUnicode = 0x554e;

		/// <summary>
		/// Info-ZIP UNIX (original; also OS/2, NT, etc.)
		/// </summary>
		public const ushort InfoZipUnixOriginal = 0x5855;

		/// <summary>
		/// Info-ZIP UTF-8 comment field
		/// </summary>
		public const ushort InfoZipUtf8Comment = 0x6375;

		/// <summary>
		/// BeOS (BeBox, PowerMac, etc.)
		/// </summary>
		public const ushort BeOS = 0x6542;

		/// <summary>
		/// Theos
		/// </summary>
		public const ushort Theos = 0x6854;

		/// <summary>
		/// Info-ZIP UTF-8 name field
		/// </summary>
		public const ushort InfoZipUtf8Name = 0x7075;

		/// <summary>
		/// AtheOS (AtheOS/Syllable attributes)
		/// </summary>
		public const ushort AtheOS = 0x7441;

		/// <summary>
		/// ASi UNIX
		/// </summary>
		public const ushort ASiUnix = 0x756e;

		/// <summary>
		/// Info-ZIP UNIX (16-bit UID/GID info)
		/// </summary>
		public const ushort InfoZipUnixType2 = 0x7855;

		/// <summary>
		/// Info-ZIP UNIX 3rd generation (generic UID/GID, ...)
		/// </summary>
		public const ushort InfoZipUnix3rdGeneration = 0x7875;

		/// <summary>
		/// Microsoft Open Packaging Growth Hint
		/// </summary>
		public const ushort MicrosoftOpenPackaging = 0xa220;

		/// <summary>
		/// SMS/QDOS
		/// </summary>
		public const ushort SMSQDOS = 0xfb4a;
	}
}

