//
// Platfom.cs
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
using System.IO;
using System.Collections.Generic;
using Xamarin.Tools.Zip.Properties;

namespace Xamarin.Tools.Zip
{
	public partial class PlatformServices
	{
		static readonly List<IPlatformServices> serviceRegistry = new List<IPlatformServices> ();

		public static PlatformServices Instance { get; } = new PlatformServices ();

		PlatformServices ()
		{
			RegisterUnixServices ();
			RegisterWindowsServices ();
		}

		partial void RegisterUnixServices ();
		partial void RegisterWindowsServices ();

		public static void RegisterServices (IPlatformServices services)
		{
			if (services == null)
				return;

			if (serviceRegistry.Contains (services))
				return;
			serviceRegistry.Add (services);
		}

		public static void UnregisterServices (IPlatformServices services)
		{
			if (services == null)
				return;

			if (serviceRegistry.Count == 0 || !serviceRegistry.Contains (services))
				return;
			serviceRegistry.Remove (services);
		}

		public bool IsDirectory (ZipArchive archive, string path)
		{
			if (String.IsNullOrEmpty (path))
				return false;

			bool isDir = false;
			if (CallServices ((IPlatformServices services) => services.IsDirectory (archive, path, out isDir)))
				return isDir;

			return Directory.Exists (path);
		}

		public bool IsRegularFile (ZipArchive archive, string path)
		{
			if (String.IsNullOrEmpty (path))
				return false;

			bool isFile = false;
			if (CallServices ((IPlatformServices services) => services.IsRegularFile (archive, path, out isFile)))
				return isFile;

			return File.Exists (path);
		}

		public EntryPermissions GetFilesystemPermissions (ZipArchive archive, string path)
		{
			if (String.IsNullOrEmpty (path))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (path)), nameof (path));

			EntryPermissions permissions = EntryPermissions.Default;
			if (!CallServices ((IPlatformServices services) => services.GetFilesystemPermissions (archive, path, out permissions)))
				return EntryPermissions.Default;

			return permissions;
		}

		public void SetEntryPermissions (ZipArchive archive, ulong index, EntryPermissions permissions, bool isDirectory)
		{
			CallServices ((IPlatformServices services) => services.SetEntryPermissions (archive, index, permissions, isDirectory));
		}

		public void SetEntryPermissions (ZipArchive archive, string sourcePath, ulong index, EntryPermissions permissions)
		{
			if (String.IsNullOrEmpty (sourcePath))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (sourcePath)), nameof (sourcePath));

			CallServices ((IPlatformServices services) => services.SetEntryPermissions (archive, sourcePath, index, permissions));
		}

		public long StoreSpecialFile (ZipArchive archive, string sourcePath, string archivePath, out CompressionMethod compressionMethod)
		{
			if (archive == null)
				throw new ArgumentNullException (nameof (archive));
			if (String.IsNullOrEmpty (sourcePath))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (sourcePath)), nameof (sourcePath));

			long index = -1;
			CompressionMethod cm = CompressionMethod.Default;
			CallServices ((IPlatformServices services) => services.StoreSpecialFile (archive, sourcePath, archivePath, out index, out cm));
			compressionMethod = cm;

			return index;
		}

		public void ReadAndProcessExtraFields (ZipEntry entry)
		{
			if (entry == null)
				throw new ArgumentNullException (nameof (entry));

			CallServices ((IPlatformServices services) => services.ReadAndProcessExtraFields (entry));
		}

		public bool WriteExtraFields (ZipArchive archive, ZipEntry entry, IList<ExtraField> extraFields = null)
		{
			if (entry == null)
				throw new ArgumentNullException (nameof (entry));

			return CallServices ((IPlatformServices services) => services.WriteExtraFields (archive, entry, extraFields));
		}

		public void SetFileProperties (ZipEntry entry, string extractedFilePath, bool throwOnNativeExceptions = true)
		{
			if (entry == null)
				throw new ArgumentNullException (nameof (entry));
			if (String.IsNullOrEmpty (extractedFilePath))
				throw new ArgumentException (string.Format (Resources.MustNotBeNullOrEmpty_string, nameof (extractedFilePath)), nameof (extractedFilePath));

			CallServices ((IPlatformServices services) => services.SetFileProperties (entry, extractedFilePath, throwOnNativeExceptions));
		}

		bool CallServices (Func<IPlatformServices, bool> code)
		{
			if (code == null)
				return false;

			foreach (IPlatformServices services in serviceRegistry) {
				if (services == null)
					continue;
				if (code (services))
					return true;
			}

			return false;
		}
	}
}

