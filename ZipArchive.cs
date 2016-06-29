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
		HashSet<object> sources = new HashSet<object> ();

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
			var streamHandle = GCHandle.Alloc (stream, GCHandleType.Pinned);
			IntPtr h = GCHandle.ToIntPtr (streamHandle);
			IntPtr source = Native.zip_source_function_create (stream_callback, h, out errorp);
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
		/// as passed to <see cref="ZipArchive.Open (string,FileMode,string,bool)"/> is used.
		/// </summary>
		/// <returns>The all.</returns>
		/// <param name="destinationDirectory">Destination directory.</param>
		public void ExtractAll (string destinationDirectory = null)
		{
			foreach (ZipEntry ze in this) {
				if (ze == null)
					continue;
				ze.Extract (destinationDirectory);
			}
		}

		/// <summary>
		/// Adds the byte[] data to the archive.
		/// </summary>
		/// <returns>The new ZipEntry for the data</returns>
		/// <param name="data">A byte[] array containing the data to add</param>
		/// <param name="archivePath">the full path for the entry in the archive.</param>
		/// <param name="permissions">The permissions which the stream should have when extracted (Unix Only)</param>
		/// <param name="method">The compression method to use</param>
		/// <param name="overwriteExisting">If true an existing entry will be overwritten. If false and an existing entry exists and error will be raised</param>
		public ZipEntry AddEntry (byte[] data, string archivePath, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod method = CompressionMethod.Default, bool overwriteExisting = true)
		{
			sources.Add (data);
			string destPath = EnsureArchivePath (archivePath);
			IntPtr source = Native.zip_source_buffer (archive, data, 0);
			long index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.Overwrite : OperationFlags.None);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, method, 0) < 0)
				throw GetErrorException ();

			if (permissions == EntryPermissions.Default)
				permissions = DefaultFilePermissions;
			PlatformServices.Instance.SetEntryPermissions (this, (ulong)index, permissions, false);
			return ReadEntry ((ulong)index);
		}

		/// <summary>
		/// Adds the stream to the archive.
		/// </summary>
		/// <returns>A new ZipEntry for the stream</returns>
		/// <param name="stream">The stream to add to the archive.</param>
		/// <param name="archivePath">The fullpath for the entry in the archive</param>
		/// <param name="permissions">The permissions which the stream should have when extracted (Unix Only)</param>
		/// <param name="method">The compression method to use</param>
		/// <param name="overwriteExisting">If true an existing entry will be overwritten. If false and an existing entry exists and error will be raised</param>
		public ZipEntry AddStream (Stream stream, string archivePath, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod method = CompressionMethod.Default, bool overwriteExisting = true)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			sources.Add (stream);
			string destPath = EnsureArchivePath (archivePath);
			var handle = GCHandle.Alloc (stream, GCHandleType.Pinned);
			IntPtr h = GCHandle.ToIntPtr (handle);
			IntPtr source = Native.zip_source_function (archive, stream_callback, h);
			long index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.Overwrite : OperationFlags.None);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, method, 0) < 0)
				throw GetErrorException ();

			if (permissions == EntryPermissions.Default)
				permissions = DefaultFilePermissions;
			PlatformServices.Instance.SetEntryPermissions (this, (ulong)index, permissions, false);
			return ReadEntry ((ulong)index);
		}

		/// <summary>
		/// Adds the file.
		/// </summary>
		/// <returns>The file.</returns>
		/// <param name="sourcePath">Source path.</param>
		/// <param name="archivePath">Archive path.</param>
		/// <param name="permissions">Permissions.</param>
		/// <param name="method">Method.</param>
		/// <param name="overwriteExisting">Overwrite existing.</param>
		public ZipEntry AddFile (string sourcePath, string archivePath = null, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod method = CompressionMethod.Default, bool overwriteExisting = true)
		{
			if (String.IsNullOrEmpty (sourcePath))
				throw new ArgumentException ("Must not be null or empty", nameof (sourcePath));

			bool isDir = PlatformServices.Instance.IsDirectory (this, sourcePath);
			string destPath = EnsureArchivePath (archivePath ?? sourcePath, isDir);
			long index;
			IntPtr source;
			if (PlatformServices.Instance.IsRegularFile (this, sourcePath)) {
				source = Native.zip_source_file (archive, sourcePath, 0, -1);
				index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.Overwrite : OperationFlags.None);
			} else {
				index = PlatformServices.Instance.StoreSpecialFile (this, sourcePath, archivePath, out method);
			}
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, isDir ? CompressionMethod.Store : method, 0) < 0)
				throw GetErrorException ();

			if (permissions == EntryPermissions.Default) {
				permissions = PlatformServices.Instance.GetFilesystemPermissions (this, sourcePath);
				if (permissions == EntryPermissions.Default)
					permissions = isDir ? DefaultDirectoryPermissions : DefaultFilePermissions;
			}
			PlatformServices.Instance.SetEntryPermissions (this, sourcePath, (ulong)index, permissions);

			return ReadEntry ((ulong)index);
		}

		/// <summary>
		/// Adds the stream to the archive.
		/// </summary>
		/// <param name="entryName">The name of the entry with in the archive</param>
		/// <param name="data">A stream containing the data to add to the archive</param>
		/// <param name="method">The compression method to use</param>
		public ZipEntry AddEntry (string entryName, Stream data, CompressionMethod method = CompressionMethod.Default)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));
			return AddStream (data, entryName, method: method);
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
		/// <param name="method">The compression method to use</param>
		public ZipEntry AddEntry (string entryName, string text, Encoding encoding, CompressionMethod method = CompressionMethod.Default)
		{
			if (string.IsNullOrEmpty (text))
				throw new ArgumentException ("must not be null or empty", nameof (text));

			if (encoding == null)
				encoding = Encoding.Default;
			return AddEntry (encoding.GetBytes (text), entryName, method: method);
		}

		/// <summary>
		/// Add a list of files to the archive
		/// </summary>
		/// <param name="fileNames">An IEnumerable<stirng> of files to add</param>
		/// <param name="directoryPathInZip">The root directory path in archive.</param>
		public void AddFiles (IEnumerable<string> fileNames, string directoryPathInZip = null)
		{
			if (fileNames == null)
				throw new ArgumentNullException (nameof (fileNames));

			foreach (var file in fileNames) {
				AddFile (file, archivePath: directoryPathInZip);
			}
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
		/// <param name="method">The compresison method to use when adding files</param>
		public void AddDirectory (string folder, string folderInArchive, CompressionMethod method = CompressionMethod.Default)
		{
			if (string.IsNullOrEmpty (folder))
				throw new ArgumentException ("must not be null or empty", nameof (folder));

			if (string.IsNullOrEmpty (folderInArchive))
				throw new ArgumentException ("must not be null or empty", nameof (folderInArchive));

			string root = folderInArchive;
			foreach (string fileName in Directory.GetFiles (folder)) {
				AddFile (fileName, ArchiveNameForFile (fileName, root), method: method);
			}
			foreach (string dir in Directory.GetDirectories (folder)) {
				var internalDir = dir.Replace ("./", string.Empty).Replace (folder, string.Empty);
				string fullDirPath = folderInArchive + internalDir;
				CreateDirectory (fullDirPath, PlatformServices.Instance.GetFilesystemPermissions (this, dir));
				AddDirectory (dir, fullDirPath, method);
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

		string EnsureArchivePath (string archivePath, bool isDir = false)
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

			if (isDir) {
				if (archivePath [archivePath.Length - 1] != Path.DirectorySeparatorChar)
					archivePath = archivePath + Path.DirectorySeparatorChar;
			}
			else if (archivePath [archivePath.Length - 1] == Path.DirectorySeparatorChar)
					archivePath = archivePath.Substring (0, archivePath.Length - 1);

			if (Path.IsPathRooted (archivePath)) {
				archivePath = archivePath.Remove (0, Path.GetPathRoot (archivePath).Length);
			}

			return archivePath.Replace ("\\", "/");
		}

		internal ZipEntry ReadEntry (ulong index)
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
					stat.valid |= (ulong)StatFlags.Size;
					Marshal.StructureToPtr<Native.zip_stat_t> (stat, data, false);
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

