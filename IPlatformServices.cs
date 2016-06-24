//
// IPlatformServices.cs
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

namespace Xamarin.ZipSharp
{
	/// <summary>
	/// Platform-specific services to manage aspects of the ZIP archive specific to a givem platform/operating system
	/// </summary>
	public interface IPlatformServices
	{
		/// <summary>
		/// Checks whether the filesystem location identified by <paramref name="path"/> is a regular
		/// file. Irregular files include device nodes, sockets, character or block devices on Unix systems etc.
		/// </summary>
		/// <returns><c>true</c> if <paramref name="path"/> points to regular file</returns>
		/// <param name="path">Path to the filesystem location</param>
		bool IsRegularFile (string path, out bool result);
		bool IsDirectory (string path, out bool result);
		bool StoreSpecialFile (ZipArchive archive, string sourcePath, string archivePath, out long index, out CompressionMethod compressionMethod);
		bool SetEntryPermissions (string sourcePath, ZipArchive archive, ulong index, EntryPermissions permissions);
		bool SetEntryPermissions (ZipArchive archive, ulong index, EntryPermissions permissions, bool isDirectory);
		bool ReadAndProcessExtraFields (ZipEntry entry);
		bool WriteExtraFields (ZipEntry entry, IList<ExtraField> extraFields);
		bool SetFileProperties (ZipEntry entry, string extractedFilePath, bool throwOnNativeErrors);
		bool ExtractSpecialFile (ZipEntry entry, string destinationDir);
	}
}
