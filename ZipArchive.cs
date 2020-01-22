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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace Xamarin.Tools.Zip
{
	/// <summary>
	/// Represents an open ZIP archive.
	/// </summary>
	public abstract partial class ZipArchive : IDisposable, IEnumerable <ZipEntry>
	{
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

		public IPlatformOptions Options { get; private set; }

		public event EventHandler<EntryExtractEventArgs> EntryExtract;

		internal ZipArchive (string defaultExtractionDir, IPlatformOptions options)
		{
			DefaultExtractionDir = defaultExtractionDir;
			if (options == null)
				throw new ArgumentNullException (nameof (options));
			Options = options;
		}

		internal ZipArchive (Stream stream, IPlatformOptions options, OpenFlags flags = OpenFlags.RDOnly)
		{
			if (options == null)
				throw new ArgumentNullException (nameof (options));
			Options = options;
			Native.zip_error_t errorp;
			var streamHandle = GCHandle.Alloc (stream, GCHandleType.Normal);
			IntPtr h = GCHandle.ToIntPtr (streamHandle);
			IntPtr source = Native.zip_source_function_create (callback, h, out errorp);
			archive = Native.zip_open_from_source (source, flags, out errorp);
			if (archive == IntPtr.Zero) {
				// error;
				string message = null;
				var error = (ErrorCode)errorp.zip_err;
				switch (error) {
					case ErrorCode.Exists:
						message = $"The file already exists";
						break;

					case ErrorCode.Incons:
						message = $"The stream failed consistency checks";
						break;

					case ErrorCode.Memory:
						message = "libzip returned out of memory error";
						break;

					case ErrorCode.NoEnt:
						message = $"Stream does not exist and file creation wasn't requested";
						break;

					case ErrorCode.NoZip:
						message = $"Stream is not a ZIP archive";
						break;

					case ErrorCode.Open:
						message = $"Stream could not be opened";
						break;

					case ErrorCode.Read:
						message = $"Error occured while reading the Stream";
						break;

					case ErrorCode.Seek:
						message = $"Stream does not support seeking";
						break;

					case ErrorCode.OK:
						break;

					default:
						message = $"Unexpected libzip error: {error}";
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
		public static ZipArchive Open (Stream stream, IPlatformOptions options = null)
		{
			return ZipArchive.CreateInstanceFromStream (stream, OpenFlags.None, options);
		}

		/// <summary>
		/// Create a new archive using the Stream provided. The steam should be an empty stream, any existing data will be overwritten.
		/// </summary>
		/// <param name="stream">The stream to create the arhive in</param>
		/// <param name="options">Platform-specific options</param>
		public static ZipArchive Create (Stream stream, IPlatformOptions options = null)
		{
			return ZipArchive.CreateInstanceFromStream (stream, OpenFlags.Create | OpenFlags.Truncate, options);
		}

		/// <summary>
		/// Open ZIP archive at <paramref name="path"/> using <see cref="FileMode"/> specified in the
		/// <paramref name="mode"/> parameter. If <paramref name="strictConsistencyChecks"/> is <c>true</c>
		/// some extra checks will be performed on the ZIP being opened. If <paramref name="defaultExtractionDir"/> is
		/// not <c>null</c> or empty it is used by default by all the entries as the destination directory. Otherwise the
		/// current directory is used as the destination. Output directory can be different for each entry, see <see cref="ZipEntry.Extract"/>
		/// </summary>
		/// <param name="path">Path to the ZIP archive.</param>
		/// <param name="mode">File open mode.</param>
		/// <param name="defaultExtractionDir">default target directory</param>
		/// <param name="strictConsistencyChecks">Perform strict consistency checks.</param>
		/// <returns>Opened ZIP archive</returns>
		public static ZipArchive Open (string path, FileMode mode, string defaultExtractionDir = null, bool strictConsistencyChecks = false, IPlatformOptions options = null)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException ("Must not be null or empty", nameof (path));
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
					message = $"The file {path} already exists";
					break;

				case ErrorCode.Incons:
					message = $"The file {path} failed consistency checks";
					break;

				case ErrorCode.Memory:
					message = "libzip returned out of memory error";
					break;

				case ErrorCode.NoEnt:
					message = $"File {path} does not exist and file creation wasn't requested";
					break;

				case ErrorCode.NoZip:
					message = $"File {path} is not a ZIP archive";
					break;

				case ErrorCode.Open:
					message = $"File {path} could not be opened";
					break;

				case ErrorCode.Read:
					message = $"Error occured while reading {path}";
					break;

				case ErrorCode.Seek:
					message = $"File {path} does not support seeking";
					break;

				case ErrorCode.OK:
					break;

				default:
					message = $"Unexpected libzip error: {error}";
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
		/// as passed to <see cref="ZipArchive.Open (string,FileMode,string,bool,IPlatformOptions)"/> is used.
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
		public ZipEntry AddStream (Stream stream, string archivePath, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod compressionMethod = CompressionMethod.Default, bool overwriteExisting = true, DateTime? modificationTime = null)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			sources.Add (stream);
			string destPath = EnsureArchivePath (archivePath);
			var handle = GCHandle.Alloc (stream, GCHandleType.Normal);
			IntPtr h = GCHandle.ToIntPtr (handle);
			IntPtr source = Native.zip_source_function (archive, callback, h);
			long index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.Overwrite : OperationFlags.None);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, compressionMethod, 0) < 0)
				throw GetErrorException ();
			if (permissions == EntryPermissions.Default)
				permissions = DefaultFilePermissions;
			PlatformServices.Instance.SetEntryPermissions (this, (ulong)index, permissions, false);
			ZipEntry entry = ReadEntry ((ulong)index);
			IList<ExtraField> fields = new List<ExtraField> ();
			ExtraField_ExtendedTimestamp timestamp = new ExtraField_ExtendedTimestamp (entry, 0, modificationTime: modificationTime ?? DateTime.UtcNow);
			fields.Add (timestamp);
			if (!PlatformServices.Instance.WriteExtraFields (this, entry, fields))
				throw GetErrorException ();
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
				throw new ArgumentException ("Must not be null or empty", nameof (sourcePath));
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
				throw new ArgumentException ("Must not be null or empty", nameof (sourcePath));

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
			if (Native.zip_set_file_compression (archive, (ulong)index, isDir ? CompressionMethod.Store : compressionMethod, 0) < 0)
				throw GetErrorException ();
			PlatformServices.Instance.SetEntryPermissions (this, sourcePath, (ulong)index, permissions);
			ZipEntry entry = ReadEntry ((ulong)index);
			IList<ExtraField> fields = new List<ExtraField> ();
			ExtraField_ExtendedTimestamp timestamp = new ExtraField_ExtendedTimestamp (entry, 0,
				createTime: File.GetCreationTimeUtc (sourcePath),
				accessTime: File.GetLastAccessTimeUtc (sourcePath),
				modificationTime: File.GetLastWriteTimeUtc (sourcePath));
			fields.Add (timestamp);
			if (!PlatformServices.Instance.WriteExtraFields (this, entry, fields))
				throw GetErrorException ();
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
				throw new ArgumentException ("must not be null or empty", nameof (text));

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
				throw new ArgumentException ("must not be null or empty", nameof (folder));

			if (string.IsNullOrEmpty (folderInArchive))
				throw new ArgumentException ("must not be null or empty", nameof (folderInArchive));

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
				throw new InvalidOperationException ("Archive destination path must not be empty");
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
			Native.zip_stat_t stat;
			int ret = Native.zip_stat_index (archive, index, OperationFlags.None, out stat);
			if (ret < 0)
				throw GetErrorException ();

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

			return new ZipException (Utilities.GetStringFromNativeAnsi (Native.zip_strerror (archive)) ?? "Unknown error", zip_error, system_error);
		}

		internal static unsafe Int64 stream_callback (IntPtr state, IntPtr data, UInt64 len, SourceCommand cmd)
		{
			byte [] buffer = null;
			var handle = GCHandle.FromIntPtr (state);
			if (!handle.IsAllocated)
				return -1;
			var stream = handle.Target as Stream;
			if (stream == null)
				return -1;
			switch (cmd) {
				case SourceCommand.Stat:
					if (len < (UInt64)sizeof (Native.zip_stat_t))
						return -1;
					var stat = Native.ZipSourceGetArgs<Native.zip_stat_t> (data, len);
					stat.size = (UInt64)stream.Length;
					stat.mtime = new IntPtr ((long)Utilities.UnixTimeFromDateTime (DateTime.UtcNow));
					stat.valid |= (ulong)(StatFlags.Size | StatFlags.MTime);
					Marshal.StructureToPtr (stat, data, false);
					return (Int64)sizeof (Native.zip_stat_t);

				case SourceCommand.Tell:
				case SourceCommand.TellWrite:
					return (Int64)stream.Position;

				case SourceCommand.Write:
					buffer = new byte [len];
					Marshal.Copy (data, buffer, 0, (int)len);
					stream.Write (buffer, 0, buffer.Length);
					return buffer.Length;

				case SourceCommand.SeekWrite:
				case SourceCommand.Seek:
					Native.zip_error_t error;
					UInt64 offset = Native.zip_source_seek_compute_offset ((UInt64)stream.Position, (UInt64)stream.Length, data, len, out error);
					stream.Seek ((long)offset, SeekOrigin.Begin);
					break;

				case SourceCommand.CommitWrite:
					stream.Flush ();
					break;

				case SourceCommand.Read:
					var length = (int)len;
					if (length > stream.Length - stream.Position) {
						length = (int)(stream.Length - stream.Position);
					}
					buffer = new byte [length];
					int bytesRead = stream.Read (buffer, 0, length);
					Marshal.Copy (buffer, 0, data, bytesRead);
					return bytesRead;

				case SourceCommand.BeginWrite:
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

			Native.zip_close (archive);
			foreach (var s in sources) {
				s.Dispose ();
			}
			sources.Clear ();
			archive = IntPtr.Zero;
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

