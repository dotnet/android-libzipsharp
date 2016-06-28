//
// WindowsPlatformServices.cs
//
// Author:
//       Dean Ellis <dellis1972@googlemail.com>
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
using System.IO;
using System.Linq;

namespace Xamarin.ZipSharp
{
	public class WindowsPlatformServices : IPlatformServices
	{
		public bool ExtractSpecialFile (ZipEntry entry, string destinationDir)
		{
			return true;
		}

		public bool IsDirectory (ZipArchive archive, string path, out bool result)
		{
			result = false;
			if (String.IsNullOrEmpty (path))
				return false;
			FileAttributes attr = File.GetAttributes (path);
			result = ((attr & FileAttributes.Directory) == FileAttributes.Directory);
			return true;
		}

		public bool IsRegularFile (ZipArchive archive, string path, out bool result)
		{
			result = false;
			if (String.IsNullOrEmpty (path))
				return false;
			FileAttributes attr = File.GetAttributes (path);
			result = ((attr & FileAttributes.Directory) != FileAttributes.Directory);
			return true;
		}

		public bool ReadAndProcessExtraFields (ZipEntry zipEntry)
		{
			var entry = zipEntry as WindowsZipEntry;
			if (entry == null)
				throw new ArgumentException ("Invalid entry type, expected WindowsZipEntry", nameof (zipEntry));

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
					break;
			}

			// Timestamps
			IList<ExtraField> fields = entry.GetExtraField (KnownExtraFields.ExtendedTimestamp);
			if (SetTimestampsFromExtendedTimeStamp (fields, entry))
				return true;

			fields = entry.GetExtraField (KnownExtraFields.InfoZipUnixOriginal);
			if (SetTimestampsFromInfoZipUnixOriginal (fields, entry))
				return true;

			return false;
		}

		bool SetTimestampsFromInfoZipUnixOriginal (IList<ExtraField> fields, WindowsZipEntry entry)
		{
			if (fields == null || fields.Count == 0)
				return false;

			DateTime modTime = DateTime.MinValue;
			DateTime accTime = DateTime.MinValue;

			ForEachExtraField (fields, true, (ExtraField ef) => {
				var izef = new ExtraField_InfoZipUnixOriginal (ef);
				SetOriginalUnixTimeStampTimes (izef, ref modTime, ref accTime);
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

		bool SetTimestampsFromExtendedTimeStamp (IList<ExtraField> fields, WindowsZipEntry entry)
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

		public bool SetEntryPermissions (ZipArchive archive, ulong index, EntryPermissions permissions, bool isDirectory)
		{
			return true;
		}

		public bool SetEntryPermissions (ZipArchive archive, string sourcePath, ulong index, EntryPermissions permissions)
		{
			return true;
		}

		public bool SetFileProperties (ZipEntry entry, string extractedFilePath, bool throwOnNativeErrors)
		{
			return true;
		}

		public bool StoreSpecialFile (ZipArchive archive, string sourcePath, string archivePath, out long index, out CompressionMethod compressionMethod)
		{
			index = -1;
			compressionMethod = CompressionMethod.Default;
			return true;
		}

		public bool WriteExtraFields (ZipEntry entry, IList<ExtraField> extraFields)
		{
			return true;
		}

		public bool GetFilesystemPermissions (ZipArchive archive, string path, out EntryPermissions permissions)
		{
			permissions = EntryPermissions.Default;

			if (String.IsNullOrEmpty (path))
				return false;

			FileSystemInfo fi;
			if (File.Exists (path))
				fi = new FileInfo (path);
			else if (Directory.Exists (path))
				fi = new DirectoryInfo (path);
			else
				return false;

			if (fi.Attributes == FileAttributes.Normal) {
				permissions = fi is FileInfo ? ZipArchive.DefaultFilePermissions : ZipArchive.DefaultDirectoryPermissions;
				return true;
			}

			permissions = EntryPermissions.OwnerRead | EntryPermissions.GroupRead | EntryPermissions.WorldRead;
			if (!fi.Attributes.HasFlag (FileAttributes.ReadOnly))
				permissions |= EntryPermissions.OwnerWrite | EntryPermissions.GroupWrite;
			if (fi is DirectoryInfo)
				permissions |= EntryPermissions.OwnerExecute | EntryPermissions.GroupExecute | EntryPermissions.WorldExecute;

			return true;
		}
	}
}

