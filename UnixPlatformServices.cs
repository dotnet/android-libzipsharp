//
// UnixPlatformServices.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mono.Unix;
using Mono.Unix.Native;

namespace Xamarin.ZipSharp
{
	class UnixPlatformServices : IPlatformServices
	{
		public bool IsDirectory (string path, out bool result)
		{
			return IsFileOfType (path, FilePermissions.S_IFDIR, out result);
		}

		public bool IsRegularFile (string path, out bool result)
		{
			return IsFileOfType (path, FilePermissions.S_IFREG, out result);
		}

		public bool GetFilesystemPermissions (string path, out EntryPermissions permissions)
		{
			permissions = EntryPermissions.Default;
			if (String.IsNullOrEmpty (path))
				return false;

			Stat sbuf;
			// Should we signal an error if stat fails? Is it important enough?
			if (Syscall.stat (path, out sbuf) == 0)
				permissions = (EntryPermissions)(Utilities.GetFilePermissions (sbuf) & UnixExternalPermissions.IMODE);

			return true;
		}

		public bool ReadAndProcessExtraFields (ZipEntry zipEntry)
		{
			var entry = zipEntry as UnixZipEntry;
			if (entry == null)
				throw new ArgumentException ("Invalid entry type, expected UnixZipEntry", nameof (zipEntry));

			// File attributes
			switch (entry.OperatingSystem) {
				case OperatingSystem.DOS:
				case OperatingSystem.AMIGA:
				case OperatingSystem.ACORN_RISC:
				case OperatingSystem.ALTERNATE_MVS:
				case OperatingSystem.CPM:
				case OperatingSystem.TANDEM:
				case OperatingSystem.OS_2:
				case OperatingSystem.WINDOWS_NTFS:
				case OperatingSystem.VFAT:
				case OperatingSystem.VM_CMS:
				case OperatingSystem.MVS:
					break;

				default:
					// Assuming a unix-like OS
					entry.FilePermissions = GetUnixPermissions (entry);
					break;
			}

			// UID and GID
			IList<ExtraField> fields = entry.GetExtraField (KnownExtraFields.InfoZipUnix3rdGeneration);
			SetIDSFromInfoZipUnix3rdGeneration (fields, entry);
		
			fields = entry.GetExtraField (KnownExtraFields.InfoZipUnixType2);
			SetIDSFromInfoZipUnixType2 (fields, entry);

			// Timestamps
			fields = entry.GetExtraField (KnownExtraFields.ExtendedTimestamp);
			if (SetTimestampsFromExtendedTimeStamp (fields, entry))
				return true;

			fields = entry.GetExtraField (KnownExtraFields.InfoZipUnixOriginal);
			if (SetTimestampsFromInfoZipUnixOriginal (fields, entry))
				return true;

			return false;
		}

		void SetIDSFromInfoZipUnixType2 (IList<ExtraField> fields, UnixZipEntry entry)
		{
			SetIDS (fields, entry, (ExtraField ef) => new ExtraField_InfoZipUnixType2 (ef));
		}

		void SetIDSFromInfoZipUnix3rdGeneration (IList<ExtraField> fields, UnixZipEntry entry)
		{
			SetIDS (fields, entry, (ExtraField ef) => new ExtraField_InfoZipUnix3rdGeneration (ef));
		}

		void SetIDS (IList<ExtraField> fields, UnixZipEntry entry, Func <ExtraField, ExtraField_UnixIDBase> fieldMaker)
		{
			if (fields == null || fields.Count == 0)
				return;
			Console.WriteLine ("Setting IDs");
			if (fieldMaker == null)
				throw new ArgumentNullException (nameof (fieldMaker));

			ForEachExtraField (fields, true, (ExtraField ef) => {
				var izef = fieldMaker (ef);
				if (!izef.DataValid)
					return;
				if (!entry.UID.HasValue && izef.UID.HasValue)
					entry.UID = izef.UID;
				if (!entry.GID.HasValue && izef.GID.HasValue)
					entry.GID = izef.GID;
			});
		}

		FilePermissions GetUnixPermissions (UnixZipEntry entry)
		{
			uint xattr = (entry.ExternalAttributes >> 16) & 0xFFFF;
			FilePermissions mode = 0;

			var perms = (UnixExternalPermissions)(xattr & (uint)UnixExternalPermissions.IFMT);
			switch (perms) {
				case 0:
				case UnixExternalPermissions.IFREG:
					entry.IsDirectory = false;
					entry.IsSymlink = false;
					break;

				case UnixExternalPermissions.IFDIR:
					entry.IsDirectory = true;
					entry.IsSymlink = false;
					break;

				case UnixExternalPermissions.IFLNK:
					entry.IsSymlink = true;
					entry.IsDirectory = false;
					break;

				default:
					// TODO: implement support for the rest of file types
					// TODO: check what happens if zip with such files is unpacked on Windows
					throw new NotSupportedException ("Files other than regular ones, directories and symlinks aren't supported yet");
			}

			perms = (UnixExternalPermissions)(xattr & (uint)UnixExternalPermissions.IMODE);
			if (perms.HasFlag (UnixExternalPermissions.IRUSR))
				mode |= FilePermissions.S_IRUSR;
			if (perms.HasFlag (UnixExternalPermissions.IRGRP))
				mode |= FilePermissions.S_IRGRP;
			if (perms.HasFlag (UnixExternalPermissions.IROTH))
				mode |= FilePermissions.S_IROTH;

			if (perms.HasFlag (UnixExternalPermissions.IWUSR))
				mode |= FilePermissions.S_IWUSR;
			if (perms.HasFlag (UnixExternalPermissions.IWGRP))
				mode |= FilePermissions.S_IWGRP;
			if (perms.HasFlag (UnixExternalPermissions.IWOTH))
				mode |= FilePermissions.S_IWOTH;

			if (perms.HasFlag (UnixExternalPermissions.IXUSR))
				mode |= FilePermissions.S_IXUSR;
			if (perms.HasFlag (UnixExternalPermissions.IXGRP))
				mode |= FilePermissions.S_IXGRP;
			if (perms.HasFlag (UnixExternalPermissions.IXOTH))
				mode |= FilePermissions.S_IXOTH;

			if (perms.HasFlag (UnixExternalPermissions.ISUID))
				mode |= FilePermissions.S_ISUID;
			if (perms.HasFlag (UnixExternalPermissions.ISGID))
				mode |= FilePermissions.S_ISGID;
			if (perms.HasFlag (UnixExternalPermissions.ISVTX))
				mode |= FilePermissions.S_ISVTX;

			return mode;
		}

		bool SetTimestampsFromInfoZipUnixOriginal (IList<ExtraField> fields, UnixZipEntry entry)
		{
			if (fields == null || fields.Count == 0)
				return false;

			DateTime modTime = DateTime.MinValue;
			DateTime accTime = DateTime.MinValue;

			ForEachExtraField (fields, true, (ExtraField ef) => {
				var izef = new ExtraField_InfoZipUnixOriginal (ef);
				SetOriginalUnixTimeStampTimes (izef, ref modTime, ref accTime);
				if (izef.Length <= 4)
					return; // No UID/GID
				if (!entry.UID.HasValue && izef.UID.HasValue)
					entry.UID = izef.UID;
				if (!entry.GID.HasValue && izef.GID.HasValue)
					entry.GID = izef.GID;
			});
			if (modTime == DateTime.MinValue)
				ForEachExtraField (fields, false, (ExtraField ef) => SetOriginalUnixTimeStampTimes (new ExtraField_InfoZipUnixOriginal (ef), ref modTime, ref accTime));

			if (modTime != DateTime.MinValue)
				entry.ModificationTime = modTime;

			// We ignore ID/GID here because it's less important than timestamps which may be set from other
			// fields should this one lack any of them.
			return (modTime != DateTime.MinValue || accTime != DateTime.MinValue);
		}

		void SetOriginalUnixTimeStampTimes (ExtraField_InfoZipUnixOriginal tstamp, ref DateTime modTime, ref DateTime accTime)
		{
			if (!tstamp.DataValid)
				return;

			if (tstamp.ModificationTime != DateTime.MinValue)
				modTime = tstamp.ModificationTime;

			if (tstamp.AccessTime != DateTime.MinValue)
				accTime = tstamp.AccessTime;
		}

		bool SetTimestampsFromExtendedTimeStamp (IList<ExtraField> fields, UnixZipEntry entry)
		{
			if (fields == null || fields.Count == 0)
				return false;

			DateTime modTime = DateTime.MinValue;
			DateTime accTime = DateTime.MinValue;
			DateTime createTime = DateTime.MinValue;

			ForEachExtraField (fields, true, (ExtraField ef) => SetExtendedTimeStampTimes (new ExtraField_ExtendedTimestamp (ef), ref modTime, ref accTime, ref createTime));

			// Central directory field only has the modification time, if at all
			if (modTime == DateTime.MinValue)
				ForEachExtraField (fields, false, (ExtraField ef) => SetExtendedTimeStampTimes (new ExtraField_ExtendedTimestamp (ef), ref modTime, ref accTime, ref createTime));

			// We reset the entry modification time only if we got a valid value since
			// that time is also set from the entry header in ZipEntry constructor - no
			// need to invalidate that value if we don't have anything better.
			if (modTime != DateTime.MinValue)
				entry.ModificationTime = modTime;

			// We don't care as much about the other times
			entry.AccessTime = accTime;
			entry.CreationTime = createTime;

			return true;
		}

		void SetExtendedTimeStampTimes (ExtraField_ExtendedTimestamp tstamp, ref DateTime modTime, ref DateTime accTime, ref DateTime createTime)
		{
			if (!tstamp.DataValid)
				return;

			if (tstamp.ModificationTime != DateTime.MinValue)
				modTime = tstamp.ModificationTime;
			if (!tstamp.Local)
				return;

			if (tstamp.AccessTime != DateTime.MinValue)
				accTime = tstamp.AccessTime;
			if (tstamp.CreationTime != DateTime.MinValue)
				createTime = tstamp.CreationTime;
		}

		void ForEachExtraField (IList<ExtraField> fields, bool local, Action<ExtraField> code)
		{
			if (code == null)
				return;

			foreach (ExtraField ef in fields.Where ((ExtraField f) => f.Local == local)) {
				if (ef == null)
					continue;
				code (ef);
			}
		}

		public bool WriteExtraFields (ZipEntry entry, IList<ExtraField> extraFields)
		{
			if (entry == null)
				return false;


			throw new NotImplementedException ();
		}

		public bool SetEntryPermissions (ZipArchive archive, ulong index, EntryPermissions requestedPermissions, bool isDirectory)
		{
			return SetEntryPermissions (archive, index, requestedPermissions, isDirectory ? UnixExternalPermissions.IFDIR : UnixExternalPermissions.IFREG);
		}

		public bool SetEntryPermissions (string sourcePath, ZipArchive archive, ulong index, EntryPermissions requestedPermissions)
		{
			UnixExternalPermissions ftype = UnixExternalPermissions.IFREG;

			if (!String.IsNullOrEmpty (sourcePath)) {
				Stat sbuf;
				if (Syscall.stat (sourcePath, out sbuf) == 0)
					ftype = Utilities.GetFileType (sbuf);
			}

			return SetEntryPermissions (archive, index, requestedPermissions, ftype);
		}

		bool SetEntryPermissions (ZipArchive archive, ulong index, EntryPermissions requestedPermissions, UnixExternalPermissions unixPermissions)
		{
			var unixArchive = archive as UnixZipArchive;
			if (unixArchive == null)
				throw new InvalidOperationException ("Expected instance of UnixZipArchive");

			return unixArchive.SetEntryUnixPermissions (index, requestedPermissions, unixPermissions);
		}

		public bool StoreSpecialFile (ZipArchive zipArchive, string sourcePath, string archivePath, out long index, out CompressionMethod compressionMethod)
		{
			var archive = zipArchive as UnixZipArchive;
			if (archive == null)
				throw new ArgumentException ("must be an instance of UnixZipArchive", nameof (zipArchive));
			
			index = -1;
			compressionMethod = CompressionMethod.DEFAULT;

			throw new NotImplementedException ();
		}

		public bool SetFileProperties (ZipEntry zipEntry, string extractedFilePath, bool throwOnNativeExceptions = true)
		{
			Console.WriteLine ($"Setting file properties for: {extractedFilePath}");
			var entry = zipEntry as UnixZipEntry;
			if (entry == null)
				throw new ArgumentException ("Invalid entry type, expected UnixZipEntry", nameof (zipEntry));
			if (String.IsNullOrEmpty (extractedFilePath))
				throw new ArgumentException ("must not be null or empty", nameof (extractedFilePath));

			Console.WriteLine ($"  File permissions: {entry.FilePermissions}");
			int err = Syscall.chmod (extractedFilePath, entry.FilePermissions);
			Console.WriteLine ($"       err == {err}");
			if (throwOnNativeExceptions && err < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			Timeval modtime = Utilities.TimevalFromDateTime (entry.ModificationTime);
			Timeval acctime;
			if (entry.AccessTime != DateTime.MinValue)
				acctime = Utilities.TimevalFromDateTime (entry.AccessTime);
			else
				acctime = modtime;
			Console.WriteLine ($"  Modification time: {entry.ModificationTime.ToString ("R")}");
			Console.WriteLine ($"        Access time: {entry.AccessTime.ToString ("R")}");
			err = Syscall.utimes (extractedFilePath, new [] { acctime, modtime });
			if (throwOnNativeExceptions && err < 0)
				UnixMarshal.ThrowExceptionForLastError ();

			// Non-critical
			//
			// Both IDs in the entry are ulong values to be "forward compatible" (whatever that means)
			// since the ZIP field that stores them allows for arbitrary length of the ID (which today
			// would really mean just 64-bit values). The casts below are thus slightly unsafer, but I
			// don't really think it matters that much...
			//
			uint uid = entry.UID.HasValue ? (uint)entry.UID : unchecked((uint)-1);
			uint gid = entry.GID.HasValue ? (uint)entry.GID : unchecked((uint)-1);
			Console.WriteLine ($"  UID: {uid}");
			Console.WriteLine ($"  GID: {gid}");
			if (Syscall.chown (extractedFilePath, uid, gid) < 0) {
				// TODO: log it properly
				Console.WriteLine ($"Warning: failed to set owner of entry '{extractedFilePath}' ({Stdlib.GetLastError ()}): {Syscall.strerror (Syscall.GetLastError ())}");
			}
			return true;
		}

		bool IsFileOfType (string path, FilePermissions mode, out bool result)
		{
			result = false;
			if (String.IsNullOrEmpty (path))
				return false;

			Stat sbuf;
			if (Syscall.stat (path, out sbuf) != 0)
				return false;

			result = (sbuf.st_mode & mode) == mode;
			return true;
		}

		public bool ExtractSpecialFile (ZipEntry entry, string destinationDir)
		{
			throw new NotImplementedException ();
		}
	}
}
