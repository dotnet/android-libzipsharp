//
// ZipEntry.cs
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
using System.IO;
using System.Runtime.InteropServices;
      
namespace Xamarin.ZipSharp
{
	/// <summary>
	/// Represents a single entry in the ZIP archive.
	/// </summary>
	public abstract partial class ZipEntry
	{
		// Must not exceed Int32.MaxValue
		const int                     ReadBufSize = 16384;

		ZipArchive                    archive;
		Native.zip_stat_t             stat;
		StatFlags                     valid;
		short                         localExtraFieldsCount;
		short                         centralExtraFieldsCount;

		/// <summary>
		/// Archive to which this entry belongs
		/// </summary>
		/// <value>The archive.</value>
		public ZipArchive Archive {
			get { return archive; }
		}

		/// <summary>
		/// Gets index of the entry in the ZIP directory.
		/// </summary>
		/// <value>Entry index</value>
		public ulong Index { get; private set; }

		/// <summary>
		/// Gets the entry name.
		/// </summary>
		/// <value>The entry name.</value>
		public string Name { get; private set; }

		/// <summary>
		/// Gets the uncompressed entry size.
		/// </summary>
		/// <value>Uncompressed entry size.</value>
		public ulong Size { get; private set; }

		/// <summary>
		/// Gets the compressed size of the entry.
		/// </summary>
		/// <value>Compressed entry size.</value>
		public ulong CompressedSize { get; private set; }

		/// <summary>
		/// Gets the last modification time of the ZIP entry. The time might be in the UTC
		/// timezone if it was read from the Unix Extended Field. Check the <see cref="DateTime.Kind"/>
		/// property before setting the time stamp.
		/// </summary>
		/// <value>Last modification time or <see cref="DateTime.MinValue"/> if invalid/unset</value>
		public DateTime ModificationTime { get; internal set; } = DateTime.MinValue;

		/// <summary>
		/// Gets the entry CRC
		/// </summary>
		/// <value>Entry CRC</value>
		public uint CRC { get; private set; }

		/// <summary>
		/// Gets the compression method.
		/// </summary>
		/// <value>The compression method.</value>
		public CompressionMethod CompressionMethod { get; private set; }

		/// <summary>
		/// Gets the encryption method.
		/// </summary>
		/// <value>The encryption method.</value>
		public EncryptionMethod EncryptionMethod { get; private set; }

		/// <summary>
		/// Indicates whether this entry represents a directory
		/// </summary>
		/// <value><c>true</c> for directory entry</value>
		public bool IsDirectory { get; internal set; }

		public uint ExternalAttributes { get; private set; }

		public OperatingSystem OperatingSystem { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Xamarin.ZipSharp.ZipEntry"/> class.
		/// </summary>
		/// <param name="archive">ZIP archive instance.</param>
		/// <param name="stat">entry status information.</param>
		internal ZipEntry (ZipArchive archive, Native.zip_stat_t stat)
		{
			if (archive == null)
				throw new ArgumentNullException (nameof (archive));
			
			this.archive = archive;
			this.stat = stat;
		}

		internal void Init ()
		{
			valid = (StatFlags)stat.valid;
			
			// We mustn't free stat.name, it's handled by libzip and freeing it here would cause a crash.
			Name = GetStatField (StatFlags.NAME, () => Utilities.GetStringFromNativeAnsi (stat.name), String.Empty);
			Index = GetStatField (StatFlags.INDEX, () => stat.index);
			Size = GetStatField (StatFlags.SIZE, () => stat.size);
			CompressedSize = GetStatField (StatFlags.COMP_SIZE, () => stat.comp_size);
			
			// This value may be overriden on Unix systems if the extended fields with ID 0x000d, 0x5455 or 0x5855 are found for this entry
			ModificationTime = GetStatField (StatFlags.MTIME, () => Utilities.DateTimeFromUnixTime ((ulong)stat.mtime.ToInt64 ()), Utilities.UnixEpoch);
			CRC = GetStatField (StatFlags.CRC, () => stat.crc);
			CompressionMethod = GetStatField (StatFlags.COMP_METHOD, () => {
				if (Enum.IsDefined (typeof (CompressionMethod), stat.comp_method))
					return (CompressionMethod)stat.comp_method;
				return CompressionMethod.UNKNOWN;
			});
			EncryptionMethod = GetStatField (StatFlags.ENCRYPTION_METHOD, () => {
				if (Enum.IsDefined (typeof (EncryptionMethod), stat.encryption_method))
					return (EncryptionMethod)stat.encryption_method;
				return EncryptionMethod.UNKNOWN;
			});
			IsDirectory = Size == 0 && CompressedSize == 0 && Name.EndsWith ("/", StringComparison.Ordinal);
			
			byte opsys;
			uint xattr;
			if (Native.zip_file_get_external_attributes (archive.ArchivePointer, Index, OperationFlags.NONE, out opsys, out xattr) == 0) {
				ExternalAttributes = xattr;
				OperatingSystem = (OperatingSystem)opsys;
			} else {
				OperatingSystem = OperatingSystem.DOS;
				ExternalAttributes = 0;
			}
			
			localExtraFieldsCount = Native.zip_file_extra_fields_count (archive.ArchivePointer, Index, OperationFlags.LOCAL);
			centralExtraFieldsCount = Native.zip_file_extra_fields_count (archive.ArchivePointer, Index, OperationFlags.CENTRAL);
			
			PlatformServices.Instance.ReadAndProcessExtraFields (this);
		}

		/// <summary>
		/// Extract this entry in directory specified by <paramref name="destinationDir"/>, optionally changing the entry's name to the
		/// one given in <paramref name="destinationFileName"/>. The destination file is opened using mode specified in the
		/// <paramref name="outputFileMode"/> parameter.
		/// </summary>
		/// <param name="destinationDir">Destination dir.</param>
		/// <param name="destinationFileName">Destination file name.</param>
		/// <param name="outputFileMode">Output file mode.</param>
		public string Extract (string destinationDir = null, string destinationFileName = null, FileMode outputFileMode = FileMode.Create)
		{
			destinationDir = destinationDir?.Trim ();
			if (String.IsNullOrEmpty (destinationDir))
				destinationDir = String.IsNullOrEmpty (archive.DefaultExtractionDir) ? "." : archive.DefaultExtractionDir;
			destinationFileName = destinationFileName?.Trim ();
			string path = Path.Combine (destinationDir, String.IsNullOrEmpty (destinationFileName) ? Name : destinationFileName);
			string dir = Path.GetDirectoryName (path);
			Directory.CreateDirectory (dir);

			var args = new EntryExtractEventArgs {
				Entry = this,
				ProcessedSoFar = 0
			};

			OnExtract (args);
			if (!IsDirectory) {
				// TODO: handle non-regular files
				OperationFlags flags = OperationFlags.NONE;
				IntPtr file = IntPtr.Zero;
				try {
					file = Native.zip_fopen_index (archive.ArchivePointer, Index, flags);
					if (file == IntPtr.Zero)
						throw archive.GetErrorException ();
					DoExtract (file, path, outputFileMode, args);
					PlatformServices.Instance.SetFileProperties (this, path, true);
				} finally {
					if (file != IntPtr.Zero)
						Native.zip_fclose (file);
				}
			}
			OnExtract (args);

			return path;
		}

		bool TranslateLocation (ZipHeaderLocation location, out OperationFlags flags)
		{
			flags = OperationFlags.NONE;
			switch (location) {
				case ZipHeaderLocation.Both:
				case ZipHeaderLocation.Any:
					if (localExtraFieldsCount <= 0 && centralExtraFieldsCount <= 0)
						return false;
					// Don't set any flags here, native api errors out if it has both central and local flags set
					break;

				case ZipHeaderLocation.Central:
					if (centralExtraFieldsCount <= 0)
						return false;
					flags = OperationFlags.CENTRAL;
					break;

				case ZipHeaderLocation.Local:
					if (localExtraFieldsCount <= 0)
						return false;
					flags = OperationFlags.LOCAL;
					break;
			}

			return true;
		}

		ushort GetFieldCount (ushort fieldID, ZipHeaderLocation location, OperationFlags flags)
		{
			short count = Native.zip_file_extra_fields_count_by_id (archive.ArchivePointer, Index, fieldID, flags);
			if (count < 0)
				count = 0;
			return (ushort)count;
		}

		public bool ExtraFieldPresent (ushort fieldID, ZipHeaderLocation location = ZipHeaderLocation.Any)
		{
			OperationFlags flags = OperationFlags.NONE;

			if (!TranslateLocation (location, out flags))
				return false;

			if (flags != OperationFlags.CENTRAL && GetFieldCount (fieldID, location, OperationFlags.LOCAL) > 0)
				return true;
			return GetFieldCount (fieldID, location, OperationFlags.CENTRAL) > 0;
		}

		public IList <ExtraField> GetExtraField (ushort fieldID, ZipHeaderLocation location = ZipHeaderLocation.Any, bool required = false)
		{
			OperationFlags flags = OperationFlags.NONE;
			if (!TranslateLocation (location, out flags))
				return null;
			var ret = new List<ExtraField> ();
			if (flags == OperationFlags.NONE) {
				GatherExtraFields (GetFieldCount (fieldID, location, OperationFlags.LOCAL), fieldID, OperationFlags.LOCAL, ret, required);
				if (location == ZipHeaderLocation.Any && ret.Count > 0)
					return ret;
				GatherExtraFields (GetFieldCount (fieldID, location, OperationFlags.CENTRAL), fieldID, OperationFlags.CENTRAL, ret, required);
			} else
				GatherExtraFields (GetFieldCount (fieldID, location, flags), fieldID, flags, ret, required);

			return ret.Count > 0 ? ret : null;
		}

		void GatherExtraFields (ushort count, ushort fieldID, OperationFlags flags, List<ExtraField> fields, bool required)
		{
			if (count == 0)
				return;
			
			ushort fieldLength;
			IntPtr fieldData;

			for (ushort i = 0; i < count; i++) {
				fieldData = Native.zip_file_extra_field_get_by_id (archive.ArchivePointer, Index, fieldID, (ushort)i, out fieldLength, flags);
				if (fieldData == IntPtr.Zero) {
					if (!required)
						continue;
					throw archive.GetErrorException ();
				}

				byte [] data;
				if (fieldLength > 0) {
					data = new byte [fieldLength];
					Marshal.Copy (fieldData, data, 0, fieldLength);
				} else
					data = null;
				
				fields.Add (new ExtraField {
					RawData = data,
					EntryIndex = Index,
					FieldIndex = i,
					ID = fieldID,
					Local = true,
					Length = fieldLength
				});
			}
		}

		void DoExtract (IntPtr zipFile, string destinationPath, FileMode outputFileMode, EntryExtractEventArgs args)
		{
			using (FileStream fs = File.Open (destinationPath, outputFileMode)) {
				DoExtract (zipFile, fs, args);
			}
		}

		void DoExtract (IntPtr zipFile, Stream destinationStream, EntryExtractEventArgs args)
		{
			var buf = new byte [ReadBufSize];

			long nread;

			while ((nread = Native.zip_fread (zipFile, buf, (ulong)buf.Length)) > 0) {
				destinationStream.Write (buf, 0, (int)nread);
				args.ProcessedSoFar += (ulong)nread;
				OnExtract (args);
			}
		}

		void OnExtract (EntryExtractEventArgs args = null)
		{
			if (args == null) {
				args = new EntryExtractEventArgs {
					Entry = this,
					ProcessedSoFar = 100 // One-shot, assuming completion
				};
			}

			archive.OnEntryExtract (args);
		}

		T GetStatField <T> (StatFlags field, Func<T> value, T deflt = default (T))
		{
			if (valid.HasFlag (field))
				return value ();
			return deflt;
		}
	}
}

