//
// Program.cs
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#if UNIX
using Mono.Unix.Native;
#endif
using Xamarin.Tools.Zip;

namespace ZipTest
{
	class MainClass
	{
		public static double CalculatePercent (double current, double max)
		{
			return Math.Round ((current * 100.0) / max, 2);
		}

		public static void Main (string [] args)
		{
			if (args?.Length <= 0) {
				Console.WriteLine ("Usage: ZipTest ZIP_ARCHIVE");
				return;
			}

			using (var zip = ZipArchive.Open (args [0], FileMode.Open, "unzipped")) {
				int cursorLeft = 0;
				Console.WriteLine ($"Number of entries: {zip.EntryCount}");
				zip.EntryExtract += (object sender, EntryExtractEventArgs e) => {
					ZipEntry ze = e.Entry;
					if (e.ProcessedSoFar == 0) {
						Console.Write ($"{(ze.IsDirectory ? "Directory" : "     File")}: {ze.FullName} {ze.Size} {ze.CompressedSize} {ze.CompressionMethod} {ze.EncryptionMethod} {ze.CRC:X} {ze.ModificationTime} {ze.ExternalAttributes:X}               ");
						cursorLeft = Console.CursorLeft;
					} else if (e.ProcessedSoFar < ze.Size) {
						Console.SetCursorPosition (cursorLeft, Console.CursorTop);
						Console.Write ($" {CalculatePercent (e.ProcessedSoFar, ze.Size)}%  ");
					} else
						Console.WriteLine ();
				};
				foreach (ZipEntry ze in zip) {
					ze.Extract ();
				}
			}


			if (File.Exists ("test-archive-write.zip"))
				File.Delete ("test-archive-write.zip");

			var t = "sdfjha;ouwrhewourfh;eajfbeouwfbdjabfkljdsbfakjsbfkjadsf";

			var ms = new MemoryStream (Encoding.UTF8.GetBytes (t));

			string asmPath = typeof (MainClass).GetType ().Assembly.Location;
			using (var zip = ZipArchive.Open ("test-archive-write.zip", FileMode.CreateNew)) {
				zip.AddFile (asmPath);
				zip.AddFile (asmPath, "/in/archive/path/ZipTestCopy.exe");
				zip.AddFile (asmPath, "/in/archive/path/ZipTestCopy2.exe", permissions: EntryPermissions.OwnerRead | EntryPermissions.OwnerWrite);
				zip.AddFile (asmPath, "/in/archive/path/ZipTestCopy3.exe", compressionMethod: CompressionMethod.Store);
				var text = "Hello World";
				zip.AddEntry ("/in/archive/data/foo.txt", text, Encoding.UTF8, CompressionMethod.Store);

				zip.AddEntry ("/in/archive/foo/foo.exe", File.OpenRead (asmPath), CompressionMethod.Store);
				zip.AddStream (ms, "/in/archive/foo/foo1.txt", compressionMethod: CompressionMethod.Store);
			}

			if (File.Exists ("test-archive-write.zip")) {
				using (var newzip = ZipArchive.Open ("test-archive-copy.zip", FileMode.Create)) {
					using (var zip = ZipArchive.Open (File.OpenRead ("test-archive-write.zip"))) {
						foreach (var e in zip) {
							Console.WriteLine ($" {e.FullName} {e.Size} {e.CompressedSize} {e.CompressionMethod}");
							ms = new MemoryStream ();
							e.Extract (ms);
							ms.Position = 0;
							newzip.AddStream (ms, e.FullName, compressionMethod: CompressionMethod.Store);
						}
					}
				}
			}

#if UNIX
			if (File.Exists ("test-archive-symlinks.zip"))
				File.Delete ("test-archive-symlinks.zip");
			using (var zip = ZipArchive.Open ("test-archive-symlinks.zip", FileMode.OpenOrCreate)) {
				var uzip = zip as UnixZipArchive;
				if (uzip != null) {
					uzip.CreateSymbolicLink ("broken-symlink-created.txt", "/tmp/path/link/destinatinon1.txt");
					Syscall.symlink ("/tmp/path/link/destination2.txt", "broken-symlink-from-fs.txt");

					// Use the "standard" zip here to test adding special files
					zip.AddFile ("broken-symlink-from-fs.txt");
				}
			}
#endif
		}
	}
}
