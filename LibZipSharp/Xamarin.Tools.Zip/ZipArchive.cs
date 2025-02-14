//
// ZipArchive.cs
//
// Author:
//       Marek Habersack <grendel@twistedcode.net>
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
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using Xamarin.Tools.Zip.Properties;

namespace Xamarin.Tools.Zip
{
	/// <summary>
	/// Represents an open ZIP archive.
	/// </summary>
	public abstract partial class ZipArchive : IDisposable, IEnumerable <ZipEntry>
	{
		internal class CallbackContext {
			public Stream Source = null;
			public Stream Destination = null;
			public string DestinationFileName = null;
			public bool UseTempFile = true;
		}
		public const EntryPermissions DefaultFilePermissions = EntryPermissions.OwnerRead | EntryPermissions.OwnerWrite | EntryPermissions.GroupRead | EntryPermissions.WorldRead;
		public const EntryPermissions DefaultDirectoryPermissions = EntryPermissions.OwnerAll | EntryPermissions.GroupRead | EntryPermissions.GroupExecute |  EntryPermissions.WorldRead | EntryPermissions.WorldExecute;

		IntPtr          archive = IntPtr.Zero;
		bool            disposed;
		HashSet<IDisposable>    sources = new HashSet<IDisposable> ();
		static Native.zip_source_callback callback = new Native.zip_source_callback (stream_callback);

		internal IntPtr ArchivePointer {
			get { return archive; }
		}

		internal string DefaultExtractionDir { get; private set; }

		/// <summary>
		/// Gets the last error code, if any, that was set by the previously invoked method.
		/// </summary>
		/// <value>The last error code.</value>
		public ErrorCode LastErrorCode { get; private set; } = ErrorCode.OK;

		/// <summary>
		/// Gets the number of entries in the ZIP archive. It takes into account modifications to
		/// the archive since it was opened.
		/// </summary>
		/// <value>The number of entries. <c>-1</c> is returned if the archive isn't open.</value>
		public long EntryCount {
			get { return Native.zip_get_num_entries (archive, OperationFlags.None); }
		}

		/// <summary>
		/// Get or set the archive comment.  If <c>null</c> is passed when setting, comment is removed
		/// from the archive.
		/// </summary>
		public string Comment {
			get => Native.zip_get_archive_comment (archive);
			set {
				if (Native.zip_set_archive_comment (archive, value) != 0) {
					throw GetErrorException ();
				}
			}
		}

		/// <summary>
		/// Get options used when the archive was opened/created.
		/// </summary>
		public IPlatformOptions Options { get; private set; }

		/// <summary>
		/// Called before and after each entry is extracted.
		/// </summary>
		public event EventHandler<EntryExtractEventArgs> EntryExtract;

		internal ZipArchive (string defaultExtractionDir, IPlatformOptions options)
		{
			DefaultExtractionDir = defaultExtractionDir;
			if (options == null)
				throw new ArgumentNullException (nameof (options));
			Options = options;
		}

		internal ZipArchive (Stream stream, IPlatformOptions options, OpenFlags flags = OpenFlags.RDOnly, bool useTempFile = true)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));
			Options = options;
			Native.zip_error_t errorp;
			CallbackContext context = new CallbackContext () {
				Source = stream,
				Destination = null,
				UseTempFile = useTempFile,
			};
			var contextHandle = GCHandle.Alloc (context, GCHandleType.Normal);
			IntPtr source = Native.zip_source_function_create (callback, GCHandle.ToIntPtr (contextHandle), out errorp);
			archive = Native.zip_open_from_source (source, flags, out errorp);
			if (archive == IntPtr.Zero) {
				// error;
				string message = null;
				var error = (ErrorCode)errorp.zip_err;
				switch (error) {
					case ErrorCode.Exists:
						message = Resources.FileAlreadyExists;
						break;

					case ErrorCode.Incons:
						message = Resources.StreamFailedConsistencyChecks;
						break;

					case ErrorCode.Memory:
						message = Resources.OutOfMemory;
						break;

					case ErrorCode.NoEnt:
						message = Resources.StreamDoesNotExist;
						break;

					case ErrorCode.NoZip:
						message = Resources.StreamIsNotAZip;
						break;

					case ErrorCode.Open:
						message = Resources.StreamCouldNotBeOpened;
						break;

					case ErrorCode.Read:
						message = Resources.ErrorReadingStream;
						break;

					case ErrorCode.Seek:
						message = Resources.StreamDoesNotSupportSeeking;
						break;

					case ErrorCode.OK:
						break;

					default:
						message = string.Format (Resources.UnexpectedLibZipError_error , error);
						break;
				}

				if (!String.IsNullOrEmpty (message))
					throw new ZipIOException (message, error, Utilities.Errno);

			}
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="T:Xamarin.ZipSharp.ZipArchive"/> is reclaimed by garbage collection.
		/// </summary>
		~ZipArchive()
		{
			Dispose (false);
		}

		ErrorCode Open (string path, OpenFlags flags)
		{
			LastErrorCode = ErrorCode.OK;

			ErrorCode error = ErrorCode.OK;
			archive = Native.zip_open (path, flags, out error);
			if (archive == IntPtr.Zero)
				LastErrorCode = error;

			return LastErrorCode;
		}

		/// <summary>
		/// Open an archive from the stream provided. This stream should contain an existing zip archive.
		/// </summary>
		/// <param name="stream">The stream to open</param>
		/// <param name="options">Platform-specific options</param>
		/// <param name="strictConsistencyChecks">Perform strict consistency checks.</param>
		/// <param name="useTempFile">Use a temporary file for the archive</param>
		public static ZipArchive Open (Stream stream, IPlatformOptions options = null, bool strictConsistencyChecks = false, bool useTempFile = true)
		{
			OpenFlags flags = OpenFlags.None;
			if (strictConsistencyChecks)
				flags |= OpenFlags.CheckCons;
			return ZipArchive.CreateInstanceFromStream (stream, flags, options, useTempFile);
		}

		/// <summary>
		/// Create a new archive using the Stream provided. The steam should be an empty stream, any existing data will be overwritten.
		/// </summary>
		/// <param name="stream">The stream to create the arhive in</param>
		/// <param name="options">Platform-specific options</param>
		/// <param name="strictConsistencyChecks">Perform strict consistency checks.</param>
		/// <param name="useTempFile">Use a temporary file for the archive</param>
		public static ZipArchive Create (Stream stream, IPlatformOptions options = null, bool strictConsistencyChecks = false, bool useTempFile = true)
		{
			OpenFlags flags = OpenFlags.Create | OpenFlags.Truncate;
			if (strictConsistencyChecks)
				flags |= OpenFlags.CheckCons;
			return ZipArchive.CreateInstanceFromStream (stream, flags, options, useTempFile);
		}

		/// <summary>
		/// Open ZIP archive at <paramref name="path"/> using <see cref="FileMode"/> specified in the
		/// <paramref name="mode"/> parameter. If <paramref name="strictConsistencyChecks"/> is <c>true</c>
		/// some extra checks will be performed on the ZIP being opened. If <paramref name="defaultExtractionDir"/> is
		/// not <c>null</c> or empty it is used by default by all the entries as the destination directory. Otherwise the
		/// current directory is used as the destination. Output directory can be different for each entry, see <see cref="ZipEntry.Extract(string, string, FileMode, bool, string)"/>
		/// </summary>
		/// <param name="path">Path to the ZIP archive.</param>
		/// <param name="mode">File open mode.</param>
		/// <param name="defaultExtractionDir">default target directory</param>
		/// <param name="strictConsistencyChecks">Perform strict consistency checks.</param>
		/// <param name="options">Platform-specific options, or <c>null</c> if none necessary (the default)</param>
		/// <param name="useTempFile">Use a temporary file for the archive</param>
		/// <returns>Opened ZIP archive</returns>
		public static ZipArchive Open (string path, FileMode mode, string defaultExtractionDir = null, bool strictConsistencyChecks = false, IPlatformOptions options = null, bool useTempFile = true)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (path)), nameof (path));
			var zip = CreateArchiveInstance (defaultExtractionDir, options);

			OpenFlags flags = OpenFlags.None;
			switch (mode) {
				case FileMode.Append:
				case FileMode.Open:
					break;

				case FileMode.Create:
					flags = OpenFlags.Create;
					break;

				case FileMode.CreateNew:
					flags = OpenFlags.Create | OpenFlags.Excl;
					break;

				case FileMode.OpenOrCreate:
					flags = OpenFlags.Create;
					break;

				case FileMode.Truncate:
					flags = OpenFlags.Truncate;
					break;
			}

			if (strictConsistencyChecks)
				flags |= OpenFlags.CheckCons;

			ErrorCode error = zip.Open (path, flags);
			string message = null;
			switch (error) {
				case ErrorCode.Exists:
					message = string.Format (Resources.FilePathAlreadyExists_file, path);
					break;

				case ErrorCode.Incons:
					message = string.Format (Resources.FileFailedConsistencyChecks_file, path);
					break;

				case ErrorCode.Memory:
					message = Resources.OutOfMemory;
					break;

				case ErrorCode.NoEnt:
					message = string.Format (Resources.FileDoesNotExist_file, path);
					break;

				case ErrorCode.NoZip:
					message = string.Format (Resources.FileIsNotAZip_file, path);
					break;

				case ErrorCode.Open:
					message = string.Format (Resources.FileCouldNotBeOpened_file, path);
					break;

				case ErrorCode.Read:
					message = string.Format (Resources.ErrorReadingFile_file, path);
					break;

				case ErrorCode.Seek:
					message = string.Format (Resources.FileDoesNotSupportSeeking_file, path);
					break;

				case ErrorCode.OK:
					break;

				default:
					message = string.Format (Resources.UnexpectedLibZipError_error, error);
					break;
			}

			if (!String.IsNullOrEmpty (message))
				throw new ZipIOException (message, error, Utilities.Errno);
			return zip;
		}

		/// <summary>
		/// Extracts all the entries from the archive and places them in the
		/// directory indicated by the <paramref name="destinationDirectory"/> parameter.
		/// If <paramref name="destinationDirectory"/> is <c>null</c> or empty, the default destination directory
		/// as passed to <see cref="ZipArchive.Open (string,FileMode,string,bool,IPlatformOptions, bool)"/> is used.
		/// </summary>
		/// <returns>The all.</returns>
		/// <param name="destinationDirectory">Destination directory.</param>
		/// <param name="password">Password of the ZipEntry</param>
		public void ExtractAll(string destinationDirectory = null, string password = null)
		{
			foreach (ZipEntry ze in this)
			{
				if (ze == null)
					continue;
				ze.Extract(destinationDirectory, password);
			}
		}

		/// <summary>
		/// Adds the byte[] data to the archive.
		/// </summary>
		/// <returns>The new ZipEntry for the data</returns>
		/// <param name="data">A byte[] array containing the data to add</param>
		/// <param name="archivePath">the full path for the entry in the archive.</param>
		/// <param name="permissions">The permissions which the stream should have when extracted (Unix Only)</param>
		/// <param name="compressionMethod">The compression method to use</param>
		/// <param name="overwriteExisting">If true an existing entry will be overwritten. If false and an existing entry exists and error will be raised</param>
		public ZipEntry AddEntry (byte[] data, string archivePath, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod compressionMethod = CompressionMethod.Default, bool overwriteExisting = true)
		{
			return AddStream (new MemoryStream (data), archivePath, permissions, compressionMethod, overwriteExisting);
		}

		/// <summary>
		/// Adds the stream to the archive.
		/// </summary>
		/// <returns>A new ZipEntry for the stream</returns>
		/// <param name="stream">The stream to add to the archive.</param>
		/// <param name="archivePath">The fullpath for the entry in the archive</param>
		/// <param name="permissions">The permissions which the stream should have when extracted (Unix Only)</param>
		/// <param name="compressionMethod">The compression method to use</param>
		/// <param name="overwriteExisting">If true an existing entry will be overwritten. If false and an existing entry exists and error will be raised</param>
		/// <param name="modificationTime">Set the entry's modification time to this value, if not <c>null</c>. Defaults to <c>null</c></param>
		public ZipEntry AddStream (Stream stream, string archivePath, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod compressionMethod = CompressionMethod.Default, bool overwriteExisting = true, DateTime? modificationTime = null)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			sources.Add (stream);
			string destPath = EnsureArchivePath (archivePath);
			var context = new CallbackContext () {
				Source = stream
			};
			var handle = GCHandle.Alloc (context, GCHandleType.Normal);
			IntPtr h = GCHandle.ToIntPtr (handle);
			IntPtr source = Native.zip_source_function (archive, callback, h);
			long index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.Overwrite : OperationFlags.None);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, compressionMethod, 9) < 0)
				throw GetErrorException ();
			if (permissions == EntryPermissions.Default)
				permissions = DefaultFilePermissions;
			PlatformServices.Instance.SetEntryPermissions (this, (ulong)index, permissions, false);
			ZipEntry entry = ReadEntry ((ulong)index);
			ExtraField_ExtendedTimestamp timestamp = new ExtraField_ExtendedTimestamp (entry, 0, modificationTime: modificationTime ?? DateTime.UtcNow);
			timestamp.Encode ();
			if (!PlatformServices.Instance.WriteExtraFields (this, entry, timestamp)) {
				throw GetErrorException ();
			}
			return entry;
		}


		/// <summary>
		/// Adds the file to archive directory. The file is added to either the root directory of
		/// the ZIP archive (if <paramref name="archiveDirectory"/> is <c>null</c> or empty) or to
		/// the directory named by <paramref name="archiveDirectory"/>. If <paramref name="useFileDirectory"/>
		/// is <c>true</c> the original file directory part is used to create the full path of the file
		/// in the archive. If <paramref name="useFileDirectory"/> is <c>false</c>, the original file directory
		/// part is ignored and the file is placed directly in <paramref name="archiveDirectory"/>. The original
		/// file name is always preserved, if you need to change it use <see cref="AddFile"/>.
		/// </summary>
		/// <returns>The file to add to an archive directory.</returns>
		/// <param name="sourcePath">Source file path.</param>
		/// <param name="archiveDirectory">Destination directory in the archive.</param>
		/// <param name="permissions">Entry permissions.</param>
		/// <param name="compressionMethod">Compression method.</param>
		/// <param name="overwriteExisting">If set to <c>true</c> overwrite existing entry in the archive.</param>
		/// <param name="useFileDirectory">If set to <c>true</c> use file directory part.</param>
		public ZipEntry AddFileToDirectory (string sourcePath, string archiveDirectory = null,
		                                    EntryPermissions permissions = EntryPermissions.Default,
		                                    CompressionMethod compressionMethod = CompressionMethod.Default,
		                                    bool overwriteExisting = true, bool useFileDirectory = true)
		{
			if (String.IsNullOrEmpty (sourcePath))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (sourcePath)), nameof (sourcePath));
			string destDir = NormalizeArchivePath (true, archiveDirectory);
			string destFile = useFileDirectory ? GetRootlessPath (sourcePath) : Path.GetFileName (sourcePath);
			return AddFile (sourcePath,
			                String.IsNullOrEmpty (destDir) ? null : destDir + "/" + destFile,
			                permissions, compressionMethod, overwriteExisting);
		}

		/// <summary>
		/// Adds the file to the archive. <paramref name="sourcePath"/> is either a relative or absolute
		/// path to the file to add to the archive. If <paramref name="sourcePath"/> is an absolute path,
		/// it will be converted to a relative one by removing the root of the path (<code>/</code> on Unix
		/// and <code>X://</code> on Windows) and stored using the resulting path in the archive. If, however,
		/// the <paramref name="archivePath"/> parameter is present it represents a full in-archive (that is -
		/// without the <code>/</code> or <code>X://</code> part) path of the file including the file name.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="sourcePath">Source path.</param>
		/// <param name="archivePath">Path in the archive, including file name.</param>
		/// <param name="permissions">Permissions.</param>
		/// <param name="compressionMethod">Compression method.</param>
		/// <param name="overwriteExisting">Overwrite existing entries in the archive.</param>
		public ZipEntry AddFile (string sourcePath, string archivePath = null,
		                         EntryPermissions permissions = EntryPermissions.Default,
		                         CompressionMethod compressionMethod = CompressionMethod.Default,
		                         bool overwriteExisting = true)
		{
			if (String.IsNullOrEmpty (sourcePath))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (sourcePath)), nameof (sourcePath));

			bool isDir = PlatformServices.Instance.IsDirectory (this, sourcePath);
			if (permissions == EntryPermissions.Default) {
				permissions = PlatformServices.Instance.GetFilesystemPermissions (this, sourcePath);
				if (permissions == EntryPermissions.Default)
					permissions = isDir ? DefaultDirectoryPermissions : DefaultFilePermissions;
			}

			if (PlatformServices.Instance.IsRegularFile (this, sourcePath))
				return AddStream (new FileStream (sourcePath, FileMode.Open, FileAccess.Read), archivePath ?? sourcePath, permissions, compressionMethod, overwriteExisting,
					modificationTime: File.GetLastWriteTimeUtc (sourcePath));

			string destPath = EnsureArchivePath (archivePath ?? sourcePath, isDir);
			long index = PlatformServices.Instance.StoreSpecialFile (this, sourcePath, archivePath, out compressionMethod);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, isDir ? CompressionMethod.Store : compressionMethod, 9) < 0)
				throw GetErrorException ();
			PlatformServices.Instance.SetEntryPermissions (this, sourcePath, (ulong)index, permissions);
			ZipEntry entry = ReadEntry ((ulong)index);
			ExtraField_ExtendedTimestamp timestamp = new ExtraField_ExtendedTimestamp (
				entry, 0,
				createTime: File.GetCreationTimeUtc (sourcePath),
				accessTime: File.GetLastAccessTimeUtc (sourcePath),
				modificationTime: File.GetLastWriteTimeUtc (sourcePath)
			);
			timestamp.Encode ();
			if (!PlatformServices.Instance.WriteExtraFields (this, entry, timestamp)) {
				throw GetErrorException ();
			}
			return entry;
		}

		/// <summary>
		/// Adds the stream to the archive.
		/// </summary>
		/// <param name="entryName">The name of the entry with in the archive</param>
		/// <param name="data">A stream containing the data to add to the archive</param>
		/// <param name="compressionMethod">The compression method to use</param>
		public ZipEntry AddEntry (string entryName, Stream data, CompressionMethod compressionMethod = CompressionMethod.Default)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			return AddStream (data, entryName, compressionMethod: compressionMethod);
		}

		/// <summary>
		/// Adds the text provided as a new entry within the archive. This is useful
		/// for when you want to just add plain text to the archive. For example a
		/// README/LICENCE file which needs to be pre-processed rather than just adding
		/// the raw file.
		/// </summary>
		/// <param name="entryName">The name of the entry with in the archive</param>
		/// <param name="text">The text to add to the entry</param>
		/// <param name="encoding">The Encoding to use for the data.</param>
		/// <param name="compressionMethod">The compression method to use</param>
		public ZipEntry AddEntry (string entryName, string text, Encoding encoding, CompressionMethod compressionMethod = CompressionMethod.Default)
		{
			if (string.IsNullOrEmpty (text))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (text)), nameof (text));

			if (encoding == null)
				encoding = Encoding.Default;
			return AddEntry (encoding.GetBytes (text), entryName, compressionMethod: compressionMethod);
		}

		/// <summary>
		/// Add a list of files to the archive. Each entry in <paramref name="fileNames"/> is passed
		/// to <see cref="AddFiles"/> and stored according to the rules described there.
		/// If <paramref name="directoryPathInZip"/> is non-null and not empty, it is treated as the
		/// either the directory in which to store all the files listed in <paramref name="fileNames"/>
		/// with their directory part stripped, if <paramref name="useFileDirectories"/> is <c>false</c> or
		/// with their directory part being used as a subdirectory of <paramref name="directoryPathInZip"/>
		/// </summary>
		///
		/// <remarks>
		/// Assuming <paramref name="directoryPathInZip"/> is <c>my/directory/</c>, and one of the files in
		/// the <paramref name="fileNames"/> parameter is <c>/path/to/my.file</c>, the following rules apply:
		///
		/// <list type="bullet">
		/// <item>
		/// <term>If <paramref name="useFileDirectories"/> == <c>true</c></term>
		/// <description>File is stored as <c>my/directory/path/to/my.file</c></description>
		/// </item>
		/// <item>
		/// <term>If <paramref name="useFileDirectories"/> == <c>false</c></term>
		/// <description>File is stored as <c>my/directory/my.file</c></description>
		/// </item>
		/// </list>
		/// </remarks>
		///
		/// <param name="fileNames">An IEnumerable&lt;string&gt; of files to add</param>
		/// <param name="directoryPathInZip">The root directory path in archive.</param>
		/// <param name="useFileDirectories">Whether directory part of files in <paramref name="fileNames"/> is used</param>
		public void AddFiles (IEnumerable<string> fileNames, string directoryPathInZip = null, bool useFileDirectories = true)
		{
			if (fileNames == null)
				throw new ArgumentNullException (nameof (fileNames));

			string archiveDir = NormalizeArchivePath (true, directoryPathInZip);
			bool gotArchiveDir = !String.IsNullOrEmpty (archiveDir);
			foreach (string file in fileNames) {
				if (String.IsNullOrEmpty (file))
					continue;

				if (!gotArchiveDir) {
					if (useFileDirectories)
						AddFile (file, GetRootlessPath (file));
					else
						AddFile (file, Path.GetFileName (file));
					continue;
				}

				string destFile;
				if (useFileDirectories)
					destFile = GetRootlessPath (file);
				else
					destFile = Path.GetFileName (file);

				AddFile (file, archivePath: Path.Combine (archiveDir, destFile));
			}
		}

		/// <summary>
		/// Deletes the specified entry. If <paramref name="entry"/> is null the request is silently ignored.
		/// An exception is thrown if the entry's index doesn't exist in the archive.
		/// </summary>
		/// <exception cref="ZipException">Thrown if the entry's index is invalid for this archive</exception>
		/// <param name="entry">Entry to delete.</param>
		public void DeleteEntry (ZipEntry entry)
		{
			if (entry == null)
				return;
			DeleteEntry (entry.Index);
		}

		/// <summary>
		/// Deletes the named entry. If <paramref name="entryName"/> is null/empty or when the named entry doesn't
		/// exist in the archive the request is silently ignored.
		/// An exception is thrown if the entry's index doesn't exist in the archive.
		/// </summary>
		/// <exception cref="ZipException">Thrown if the entry's index is invalid for this archive</exception>
		/// <param name="entryName">Entry name.</param>
		/// <param name="caseSensitive">If set to <c>true</c> name lookup is case sensitive.</param>
		public void DeleteEntry (string entryName, bool caseSensitive = false)
		{
			long index;
			if (!ContainsEntry (entryName, out index, caseSensitive))
				return;
			DeleteEntry ((ulong)index);
		}

		/// <summary>
		/// Deletes the specified entry. An exception is thrown if the entry's index doesn't exist in the archive.
		/// </summary>
		/// <exception cref="ZipException">Thrown if the entry's index is invalid for this archive</exception>
		/// <param name="entryIndex">Entry index.</param>
		public void DeleteEntry (ulong entryIndex)
		{
			if (Native.zip_delete (archive, entryIndex) < 0)
				throw GetErrorException ();
		}

		string GetRootlessPath (string path)
		{
			if (String.IsNullOrEmpty (path) || !Path.IsPathRooted (path))
				return path;

			return path.Remove (0, Path.GetPathRoot (path).Length);
		}

		long LookupEntry (string entryName, bool caseSensitive)
		{
			if (String.IsNullOrEmpty (entryName))
				return -1;

			return Native.zip_name_locate (archive, entryName, caseSensitive ? OperationFlags.None : OperationFlags.NoCase);
		}

		/// <summary>
		/// Checks to see if an entryName exists in the archive. The comparison is done case-insensitively by
		/// default.
		/// </summary>
		/// <returns>Returns true if the entry exists, false otherwise</returns>
		/// <param name="entryName">The name of the entry to check for</param>
		/// <param name="caseSensitive">Compare names case-insensitively if set to <c>true</c></param>
		public bool ContainsEntry (string entryName, bool caseSensitive = false)
		{
			return LookupEntry (entryName, caseSensitive) >= 0;
		}

		/// <summary>
		/// Checks to see if an entryName exists in the archive. The comparison is done case-insensitively by
		/// default. Additionally checks whether the entry represents a directory.
		/// </summary>
		/// <returns>Returns true if the entry exists, false otherwise</returns>
		/// <param name="entryName">The name of the entry to check for</param>
		/// <param name="index">Entry index in the archive or <c>-1</c> when not found</param>
		/// <param name="caseSensitive">Compare names case-insensitively if set to <c>true</c></param>
		public bool ContainsEntry (string entryName, out long index, bool caseSensitive = false)
		{
			index = LookupEntry (entryName, caseSensitive);
			return index >= 0;
		}

		/// <summary>
		/// Recursively Add an entire directory structure to the archive
		/// </summary>
		/// <param name="folder">The root of the directory to add</param>
		/// <param name="folderInArchive">The root name of the folder in the zip.</param>
		/// <param name="compressionMethod">The compresison method to use when adding files</param>
		public void AddDirectory (string folder, string folderInArchive, CompressionMethod compressionMethod = CompressionMethod.Default)
		{
			if (string.IsNullOrEmpty (folder))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (folder)), nameof (folder));

			if (string.IsNullOrEmpty (folderInArchive))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (folderInArchive)), nameof (folderInArchive));

			string root = folderInArchive;
			foreach (string fileName in Directory.GetFiles (folder)) {
				AddFile (fileName, ArchiveNameForFile (fileName, root), compressionMethod: compressionMethod);
			}
			foreach (string dir in Directory.GetDirectories (folder)) {
				var internalDir = dir.Replace ("./", string.Empty).Replace (folder, string.Empty);
				string fullDirPath = folderInArchive + internalDir;
				CreateDirectory (fullDirPath, PlatformServices.Instance.GetFilesystemPermissions (this, dir));
				AddDirectory (dir, fullDirPath, compressionMethod);
			}
		}

		public void CreateDirectory (string directoryName, EntryPermissions permissions = EntryPermissions.Default)
		{
			string dir = EnsureArchivePath (directoryName, true);
			long index = Native.zip_dir_add (archive, dir, OperationFlags.None);
			if (index < 0)
				throw GetErrorException ();

			if (permissions == EntryPermissions.Default)
				permissions = DefaultDirectoryPermissions;
			PlatformServices.Instance.SetEntryPermissions (this, (ulong)index, permissions, true);
		}

		internal string EnsureArchivePath (string archivePath, bool isDir = false)
		{
			string destPath = NormalizeArchivePath (isDir, archivePath);
			if (String.IsNullOrEmpty (destPath))
				throw new InvalidOperationException (Resources.DestinationMustNotBeEmpty);
			return destPath;
		}

		string ArchiveNameForFile (string filename, string directoryPathInZip)
		{
			if (string.IsNullOrEmpty (filename)) {
				throw new ArgumentNullException (nameof (filename));
			}
			string pathName;
			if (string.IsNullOrEmpty (directoryPathInZip)) {
				pathName = Path.GetFileName (filename);
			}
			else {
				pathName = Path.Combine (directoryPathInZip, Path.GetFileName (filename));
			}
			return pathName.Replace ("\\", "/");
		}

		string NormalizeArchivePath (bool isDir, string archivePath)
		{
			if (String.IsNullOrEmpty (archivePath))
				return archivePath;

			if (archivePath.IndexOf ('\\') >= 0)
				archivePath = archivePath.Replace ("\\", "/");

			if (isDir) {
				if (IsDirectorySeparator (archivePath [archivePath.Length - 1])) {
					archivePath = archivePath + Path.DirectorySeparatorChar;
				}
			} else if (IsDirectorySeparator (archivePath [archivePath.Length - 1]))
				archivePath = archivePath.Substring (0, archivePath.Length - 1);

			if (Path.IsPathRooted (archivePath)) {
				archivePath = archivePath.Remove (0, Path.GetPathRoot (archivePath).Length);
			}

			return archivePath;
		}

		bool IsDirectorySeparator (char ch)
		{
			// Paths passed to the various methods can include ZIP paths which use / as the
			// separator char, regardless of the operating system.
			return ch == '/' || ch == Path.DirectorySeparatorChar;
		}

		public ZipEntry ReadEntry (string entryName, bool caseSensitive = false)
		{
			return ReadEntry ((ulong)LookupEntry (entryName, caseSensitive));
		}

		public ZipEntry ReadEntry (ulong index)
		{
			return ReadEntry (index, throwIfDeleted: true);
		}

		/// <summary>
		/// Read a zip entry, given an index.
		///
		/// When throwIfDeleted is true, if the entry is deleted then an exception is thrown (the error will be
		/// ErrorCode.Deleted or ErrorCode.Inval, depending on whether the deleted entry previously existed in
		/// the zip or was newly added - that's just how libzip handles that). If throwIfDeleted is false then
		/// null is returned for deleted entries and an exception is just thrown for other errors.
		/// </summary>
		/// <param name="index">index to read</param>
		/// <param name="throwIfDeleted">whether to return null or throw an exception for deleted entries</param>
		/// <returns></returns>
		public ZipEntry ReadEntry (ulong index, bool throwIfDeleted)
		{
			Native.zip_stat_t stat;
			int ret = Native.zip_stat_index (archive, index, OperationFlags.None, out stat);
			if (ret < 0) {
				if (! throwIfDeleted) {
					IntPtr error = Native.zip_get_error (archive);

					if (error != IntPtr.Zero) {
						int zip_error = Native.zip_error_code_zip (error);
						// Deleted is returned when the deleted entry existed when the zip was opened
						// Inval is returned when the deleted entry was newly added to the zip, then deleted
						if (zip_error == (int) ErrorCode.Deleted || zip_error == (int)ErrorCode.Inval)
							return null;
					}
				}

				throw GetErrorException ();
			}

			var ze = ZipEntry.Create (this, stat);
			ze.Init ();

			return ze;
		}

		internal ZipException GetErrorException ()
		{
			IntPtr error = Native.zip_get_error (archive);
			int zip_error;
			int system_error;

			if (error != IntPtr.Zero) {
				zip_error = Native.zip_error_code_zip (error);
				system_error = Native.zip_error_code_system (error);
			} else {
				zip_error = -1;
				system_error = -1;
			}

			return new ZipException (Utilities.GetStringFromNativeAnsi (Native.zip_strerror (archive)) ?? Resources.UnknownError, zip_error, system_error);
		}

		internal static unsafe Int64 stream_callback (IntPtr state, IntPtr data, UInt64 len, SourceCommand cmd)
		{
#if !NET6_0_OR_GREATER
			byte [] buffer = null;
#endif
			Native.zip_error_t error;
			int length = (int)len;
			var handle = GCHandle.FromIntPtr (state);
			if (!handle.IsAllocated)
				return -1;
			var context = handle.Target as CallbackContext;
			if (context == null)
				return -1;
			var stream = context.Source;
			if (stream == null)
				return -1;
			var destination = context.Destination ?? context.Source;
			switch (cmd) {
				case SourceCommand.Stat:
					Native.zip_stat_t stat;
					if (!Native.ZipSourceGetArgs (data, len, out stat)) {
						return -1;
					}

					stat.size = (UInt64)stream.Length;
					stat.mtime = new IntPtr ((long)Utilities.UnixTimeFromDateTime (DateTime.UtcNow));
					stat.valid |= (ulong)(StatFlags.Size | StatFlags.MTime);
					Marshal.StructureToPtr (stat, data, false);
					return (Int64)sizeof (Native.zip_stat_t);

				case SourceCommand.Tell:
					return (Int64)stream.Position;
				case SourceCommand.TellWrite:
					return (Int64)destination.Position;

				case SourceCommand.Write:
#if NET6_0_OR_GREATER
					unsafe
					{
						byte* ptr = (byte*)data;
						destination.Write(new ReadOnlySpan<byte>(ptr, length));
					}
					return length;
#else
					buffer = ArrayPool<byte>.Shared.Rent (length);
					try {
						Marshal.Copy (data, buffer, 0, length);
						destination.Write (buffer, 0, length);
						return length;
					} finally {
						ArrayPool<byte>.Shared.Return (buffer);
					}
#endif
				case SourceCommand.SeekWrite:
					Native.zip_source_args_seek_t args;
					if (!Native.ZipSourceGetArgs (data, len, out args)) {
						return -1;
					}

					SeekOrigin seek = Native.ConvertWhence (args.whence);
					if (args.offset > Int64.MaxValue) {
						// Stream.Seek uses a signed 64-bit value for the offset, we need to split it up
						if (!Seek (destination, seek, Int64.MaxValue) || !Seek (destination, seek, (long)(args.offset - Int64.MaxValue))) {
							return -1;
						}
					} else {
						if (!Seek (destination, seek, (long)args.offset)) {
							return -1;
						}
					}
					
					break;

				case SourceCommand.Seek:
					long offset = Native.zip_source_seek_compute_offset ((UInt64)stream.Position, (UInt64)stream.Length, data, len, out error);
					if (offset < 0) {
						return offset;
					}
					if (!Seek (stream, SeekOrigin.Begin, offset)) {
						return -1;
					}
					break;

				case SourceCommand.CommitWrite:
					destination.Flush ();
					stream.Position = 0;
					destination.Position = 0;
					stream.SetLength (destination.Length);
					destination.CopyTo (stream);
					stream.Flush ();
					stream.Position = 0;
					destination.Dispose ();
					context.Destination = null;
					if (!string.IsNullOrEmpty (context.DestinationFileName)&& File.Exists (context.DestinationFileName)) {
						try {
							File.Delete (context.DestinationFileName);
						} catch (Exception) {
							// we are deleting a temp file. So we can ignore any error.
							// it will get cleaned up eventually.
							Console.WriteLine ($"warning: Could not delete {context.DestinationFileName}.");
						}
						context.DestinationFileName = null;
					}
					break;
				case SourceCommand.RollbackWrite:
					destination.Dispose ();
					context.Destination = null;
					break;

				case SourceCommand.Read:
					length = (int)Math.Min (stream.Length - stream.Position, length);
#if NET6_0_OR_GREATER
					unsafe
					{
						byte* ptr = (byte*)data;
						int bytesRead = stream.Read(new Span<byte>(ptr, length));
						return bytesRead;
					}
#else
					buffer = ArrayPool<byte>.Shared.Rent (length);
					try {
						int bytesRead = 0;
						int startIndex = 0;
						while (length > 0) {
							bytesRead = stream.Read (buffer, 0, length);
							Marshal.Copy (buffer, startIndex, data, bytesRead);
							startIndex += bytesRead;
							length -= bytesRead;
						}
						return bytesRead;
					} finally {
						ArrayPool<byte>.Shared.Return (buffer);
					}
#endif
				case SourceCommand.BeginWrite:
					try {
						if (context.UseTempFile) {
							string tempFile = Path.GetTempFileName ();
							context.Destination = File.Open (tempFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
							context.DestinationFileName = tempFile;
						} else {
							context.Destination = new MemoryStream ();
						}
					} catch (IOException) {
						// ok use a memory stream as a backup
						context.Destination = new MemoryStream ();
					}
					destination = context.Destination;
					destination.Position = 0;
					break;
				case SourceCommand.Open:
					stream.Position = 0;
					return 0;

				case SourceCommand.Close:
					stream.Flush ();
					break;

				case SourceCommand.Free:
					if (handle.IsAllocated)
						handle.Free ();
					break;

				case SourceCommand.Supports:
					var supports = (Int64)Native.zip_source_make_command_bitmap (
						SourceCommand.Open,
						SourceCommand.Read,
						SourceCommand.Close,
						SourceCommand.Stat,
						SourceCommand.Error,
						SourceCommand.Free
					);
					if (stream.CanSeek) {
						supports |= (Int64)Native.zip_source_make_command_bitmap (
							SourceCommand.Seek,
							SourceCommand.Tell,
							SourceCommand.Supports
						);
					}
					if (stream.CanWrite) {
						supports |= (Int64)Native.zip_source_make_command_bitmap (
							SourceCommand.BeginWrite,
							SourceCommand.CommitWrite,
							SourceCommand.RollbackWrite,
							SourceCommand.Write,
							SourceCommand.SeekWrite,
							SourceCommand.TellWrite,
							SourceCommand.Remove
						);
					}
					return supports;
				default:
					break;
			}

			return 0;

			bool Seek (Stream s, SeekOrigin origin, long offset)
			{
				return s.Seek (offset, origin) == offset;
			}
		}

		internal void OnEntryExtract (EntryExtractEventArgs args)
		{
			if (EntryExtract == null)
				return;
			EntryExtract (this, args);
		}

		public void Close ()
		{
			if (archive == IntPtr.Zero)
				return;

			try {
				if (Native.zip_close (archive) < 0)
					throw GetErrorException ();
			} finally {
				foreach (var s in sources) {
					s.Dispose ();
				}
				sources.Clear ();
				archive = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Closes the native ZIP archive handle and frees associated resources, if called from the finalizer.
		/// </summary>
		/// <param name="disposing"><c>false</c> if called from the finalizer, <c>true</c> otherwise.</param>
		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					// TODO: dispose managed state (managed objects).
				}

				Close ();
				disposed = true;
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="T:Xamarin.ZipSharp.ZipArchive"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="T:Xamarin.ZipSharp.ZipArchive"/>. The
		/// <see cref="Dispose()"/> method leaves the <see cref="T:Xamarin.ZipSharp.ZipArchive"/> in an unusable state. After
		/// calling <see cref="Dispose()"/>, you must release all references to the <see cref="T:Xamarin.ZipSharp.ZipArchive"/>
		/// so the garbage collector can reclaim the memory that the <see cref="T:Xamarin.ZipSharp.ZipArchive"/> was occupying.</remarks>
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Gets the enumerator which iterates over all the entries in the archive.
		/// </summary>
		/// <returns>The archive entry enumerator.</returns>
		public IEnumerator<ZipEntry> GetEnumerator ()
		{
			return new ZipEntryEnumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}
}
