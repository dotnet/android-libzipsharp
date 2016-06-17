//
// ZipArchive.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.ZipSharp
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
		public long NumberOfEntries {
			get { return Native.zip_get_num_entries (archive, OperationFlags.NONE); }
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

			OpenFlags flags = OpenFlags.NONE;
			switch (mode) {
				case FileMode.Append:
				case FileMode.Open:
					break;
					
				case FileMode.Create:
					flags = OpenFlags.CREATE;
					break;
					
				case FileMode.CreateNew:
					flags = OpenFlags.CREATE | OpenFlags.EXCL;
					break;
					
				case FileMode.OpenOrCreate:
					flags = OpenFlags.CREATE;
					break;
					
				case FileMode.Truncate:
					flags = OpenFlags.TRUNCATE;
					break;
			}

			if (strictConsistencyChecks)
				flags |= OpenFlags.CHECKCONS;
			
			ErrorCode error = zip.Open (path, flags);
			string message = null;
			switch (error) {
				case ErrorCode.EXISTS:
					message = $"The file {path} already exists";
					break;

				case ErrorCode.INCONS:
					message = $"The file {path} failed consistency checks";
					break;

				case ErrorCode.MEMORY:
					message = "libzip returned out of memory error";
					break;

				case ErrorCode.NOENT:
					message = $"File {path} does not exist and file creation wasn't requested";
					break;

				case ErrorCode.NOZIP:
					message = $"File {path} is not a ZIP archive";
					break;

				case ErrorCode.OPEN:
					message = $"File {path} could not be opened";
					break;

				case ErrorCode.READ:
					message = $"Error occured while reading {path}";
					break;

				case ErrorCode.SEEK:
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

		public ZipEntry AddEntry (byte[] data, string archivePath, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod method = CompressionMethod.DEFAULT, bool overwriteExisting = true)
		{
			string destPath = NormalizeArchivePath (false, archivePath);
			if (String.IsNullOrEmpty (destPath))
				throw new InvalidOperationException ("Archive destination path must not be empty");

			IntPtr source = Native.zip_source_buffer (archive, data, 0);
			long index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.OVERWRITE : OperationFlags.NONE);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, method, 0) < 0)
				throw GetErrorException ();

			if (permissions == EntryPermissions.Default)
				permissions = DefaultFilePermissions;
			PlatformServices.Instance.SetEntryPermissions (this, (ulong)index, permissions, false);

			return ReadEntry ((ulong)index);
		}

		public ZipEntry AddFile (string sourcePath, string archivePath = null, EntryPermissions permissions = EntryPermissions.Default, CompressionMethod method = CompressionMethod.DEFAULT, bool overwriteExisting = true)
		{
			if (String.IsNullOrEmpty (sourcePath))
				throw new ArgumentException ("Must not be null or empty", nameof (sourcePath));

			bool isDir = PlatformServices.Instance.IsDirectory (sourcePath);
			string destPath = NormalizeArchivePath (isDir, archivePath ?? sourcePath);
			if (String.IsNullOrEmpty (destPath))
				throw new InvalidOperationException ("Archive destination path must not be empty");

			IntPtr source = Native.zip_source_file (archive, sourcePath, 0, -1);
			long index = Native.zip_file_add (archive, destPath, source, overwriteExisting ? OperationFlags.OVERWRITE : OperationFlags.NONE);
			if (index < 0)
				throw GetErrorException ();
			if (Native.zip_set_file_compression (archive, (ulong)index, method, 0) < 0)
				throw GetErrorException ();
			
			if (permissions == EntryPermissions.Default)
				permissions = isDir ? DefaultDirectoryPermissions : DefaultFilePermissions;
			PlatformServices.Instance.SetEntryPermissions (sourcePath, this, (ulong)index, permissions);

			return ReadEntry ((ulong)index);
		}

		string NormalizeArchivePath (bool isDir, string archivePath)
		{
			if (String.IsNullOrEmpty (archivePath))
				return archivePath;

			if (isDir) {
				if (archivePath [archivePath.Length - 1] != Path.DirectorySeparatorChar)
					return archivePath + Path.DirectorySeparatorChar;
				return archivePath;
			}

			if (archivePath [archivePath.Length - 1] == Path.DirectorySeparatorChar)
				return archivePath.Substring (0, archivePath.Length - 1);
			return archivePath;
		}

		internal ZipEntry ReadEntry (ulong index)
		{
			Native.zip_stat_t stat;
			int ret = Native.zip_stat_index (archive, index, OperationFlags.NONE, out stat);
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

