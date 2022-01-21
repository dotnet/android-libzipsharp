using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xamarin.Tools.Zip;
using Unix = Mono.Unix;

namespace Tests {

	public class ZipTests {

		void AssertEntryIsValid (ZipEntry e, string expectedArchivePath,
				EntryPermissions permissions = EntryPermissions.Default, CompressionMethod compression = CompressionMethod.Store)
		{
			Assert.IsNotNull (e, $"ZipEntry for {expectedArchivePath} should not be null.");
			Assert.AreEqual (e.FullName, expectedArchivePath, $"Expected {expectedArchivePath} but got {e.FullName}");
			Assert.AreEqual (e.CompressionMethod, compression, $"Expected {compression} but was {e.CompressionMethod} for {e.FullName}");
			//Assert.AreEqual (e.CompressionMethod, permissions, $"Expected {permissions} but was {e.ExternalAttributes} for {e.FullName}");
		}

		const string TEXT =  "oijoihaofiehfeafewufn e;fau 9ubre9wurew9;ru9;0oewubewa9ru bawpeu;9fberbf oiewrf";

		[Test]
		public void NativeVersions ()
		{
			Versions versions = Info.GetVersions ();

			Assert.IsNotNull (versions, "GetVersions must not return null");
			Assert.IsNotEmpty (versions.BZip2, "BZip2 version must not be empty");
			Assert.IsNotEmpty (versions.LibZip, "libzip version must not be empty");
			Assert.IsNotEmpty (versions.Zlib, "zlib version must not be empty");
			Assert.IsNotEmpty (versions.ZlibNG, "zlib-ng version must not be empty");
#if HAVE_XZ
			Assert.IsNotEmpty (versions.LZMA, "LZMA version must not be empty");
#endif
			Assert.IsNotEmpty (versions.LibZipSharp, "LibZipSharp version must not be empty");

			Console.WriteLine ($"LibZipSharp version: {versions.LibZipSharp}");
			Console.WriteLine ($"BZip2 version: {versions.BZip2}");
			Console.WriteLine ($"libzip version: {versions.LibZip}");
			Console.WriteLine ($"zlib version: {versions.Zlib}");
			Console.WriteLine ($"zlib-ng version: {versions.ZlibNG}");
			Console.WriteLine ($"LZMA version: {versions.LZMA}");
		}

		[Test]
		public void CanCreateZipFile ()
		{
			var ms = new MemoryStream (Encoding.UTF8.GetBytes (TEXT));
			File.WriteAllText ("file1.txt", "1111");
			string filePath = Path.GetFullPath ("file1.txt");
			if (File.Exists ("test-archive-write.zip"))
				File.Delete ("test-archive-write.zip");
			using (var zip = ZipArchive.Open ("test-archive-write.zip", FileMode.CreateNew)) {
				ZipEntry e;
				e = zip.AddFile (filePath, Path.GetFileName (filePath));
				AssertEntryIsValid (e, Path.GetFileName (filePath));
				e = zip.AddFile (filePath, "/in/archive/path/ZipTestCopy.exe");
				AssertEntryIsValid (e, "in/archive/path/ZipTestCopy.exe");
				e = zip.AddFile (filePath, "/in/archive/path/ZipTestCopy2.exe", permissions: EntryPermissions.OwnerRead | EntryPermissions.OwnerWrite);
				AssertEntryIsValid (e, "in/archive/path/ZipTestCopy2.exe", permissions: EntryPermissions.OwnerRead | EntryPermissions.OwnerWrite);
				e = zip.AddFile (filePath, "/in/archive/path/ZipTestCopy3.exe", compressionMethod: CompressionMethod.Store);
				AssertEntryIsValid (e, "in/archive/path/ZipTestCopy3.exe", compression: CompressionMethod.Store);
				e = zip.AddFile (filePath, "\\invalid/archive\\path/ZipTestCopy4.exe");
				AssertEntryIsValid (e, "invalid/archive/path/ZipTestCopy4.exe");
				var text = "Hello World";
				e = zip.AddEntry ("/in/archive/data/foo.txt", text, Encoding.UTF8, CompressionMethod.Store);
				AssertEntryIsValid (e, "in/archive/data/foo.txt", compression: CompressionMethod.Store);
				e = zip.AddEntry ("/in/archive/foo/foo.exe", File.OpenRead (filePath), CompressionMethod.Store);
				AssertEntryIsValid (e, "in/archive/foo/foo.exe", compression: CompressionMethod.Store);
				e = zip.AddStream (ms, "/in/archive/foo/foo1.txt", compressionMethod: CompressionMethod.Store);
				AssertEntryIsValid (e, "in/archive/foo/foo1.txt", compression: CompressionMethod.Store);
			}
		}

		[Test]
		public void CanExtractZipFile ()
		{
			var root = Path.Combine (Path.GetDirectoryName (typeof (ZipTests).Assembly.Location), "Exracted");
			if (Directory.Exists (root))
				Directory.Delete (root, recursive: true);
			File.WriteAllText ("foo.txt", "Hello World", encoding: Encoding.ASCII);
			File.WriteAllText ("foo1.txt", TEXT, encoding: Encoding.ASCII);
			using (var zip = ZipArchive.Open ("test-archive-write.zip", FileMode.Open)) {
				zip.ExtractAll (root);
				FileAssert.AreEqual (Path.Combine (root, "file1.txt"), "file1.txt", "file1.txt should have been 1111");
				FileAssert.AreEqual (Path.Combine (root, "in", "archive", "path", "ZipTestCopy.exe"), "file1.txt", "ZipTestCopy.exe should have been 1111");
				FileAssert.AreEqual (Path.Combine (root, "in", "archive", "path", "ZipTestCopy2.exe"), "file1.txt", "ZipTestCopy2.exe should have been 1111");
				FileAssert.AreEqual (Path.Combine (root, "in", "archive", "path", "ZipTestCopy3.exe"), "file1.txt", "ZipTestCopy3.exe should have been 1111");
				FileAssert.AreEqual (Path.Combine (root, "invalid", "archive", "path", "ZipTestCopy4.exe"), "file1.txt", "ZipTestCopy4.exe should have been 1111");
				FileAssert.AreEqual (Path.Combine (root, "in", "archive", "data", "foo.txt"), "foo.txt", "foo.txt should have been 'Hello World'");
				FileAssert.AreEqual (Path.Combine (root, "in", "archive", "foo", "foo.exe"), "file1.txt", "foo.exe should have been 1111");
				FileAssert.AreEqual (Path.Combine (root, "in", "archive", "foo", "foo1.txt"), "foo1.txt", $"foo1.txt should have been {TEXT}");
			}
		}

#if !WINDOWS
		[Test]
		public void CheckExtractedFilePermissions ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Unix)
				Assert.Ignore ("Skipping. Test invalid on windows.");
			string root = Path.Combine (Path.GetDirectoryName (typeof (ZipTests).Assembly.Location), "CheckExtractedFilePermissions");
			Directory.CreateDirectory (root);
			string extractedDir = Path.Combine (root, "Extracted");
			string file = Path.Combine (root, "script");
			string zip = Path.Combine (root, "foo.zip");
			if (File.Exists (zip))
				File.Delete (zip);
			if (File.Exists (file))
				File.Delete (file);
			File.WriteAllText (file, "#!/bin/bash" + Environment.NewLine + "echo foo");
			using (var archive = ZipArchive.Open (zip, FileMode.Create)) {
				ZipEntry entry = archive.AddFile (file, archivePath: "script", permissions: EntryPermissions.OwnerAll | EntryPermissions.GroupAll | EntryPermissions.WorldAll);
			}
			using (var archive = ZipArchive.Open (zip, FileMode.Open)) {
				archive.ExtractAll (extractedDir);
				string script = Path.Combine (extractedDir, "script");
				FileAssert.Exists (script);
				var ufi = new Unix.UnixFileInfo(script);
				Assert.IsTrue (ufi.FileAccessPermissions.HasFlag (Unix.FileAccessPermissions.UserExecute));
				Assert.IsTrue (ufi.FileAccessPermissions.HasFlag (Unix.FileAccessPermissions.GroupExecute));
				Assert.IsTrue (ufi.FileAccessPermissions.HasFlag (Unix.FileAccessPermissions.OtherExecute));
			}

		}
#endif

		static DateTime WithoutMilliseconds (DateTime t) =>
			new DateTime (t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second, t.Kind);

		[Test]
		[Repeat (100)]
		[NonParallelizable]
		public void CheckDates ()
		{
			string root = Path.Combine (Path.GetDirectoryName (typeof (ZipTests).Assembly.Location));
			string file = Path.Combine (root, "foo.txt");
			string zip = Path.Combine (root, "foo.zip");
			if (File.Exists (zip))
				File.Delete (zip);
			if (File.Exists (file))
				File.Delete (file);
			File.WriteAllText (file, TEXT);
			DateTime lastWrite = WithoutMilliseconds (File.GetLastWriteTimeUtc (file));
			Thread.Sleep (TimeSpan.FromSeconds (2));
			using (var archive = ZipArchive.Open (zip, FileMode.Create)) {
				ZipEntry entry = archive.AddFile (file, archivePath: "foo.txt");
				Assert.AreEqual (lastWrite, WithoutMilliseconds (entry.ModificationTime), $"Check 1 {WithoutMilliseconds (entry.ModificationTime)} != {lastWrite}");
			}
			using (var archive = ZipArchive.Open (zip, FileMode.Open)) {
				ZipEntry entry = archive.ReadEntry ("foo.txt");
				Assert.IsTrue (entry.ExtraFieldPresent (KnownExtraFields.ExtendedTimestamp, ZipHeaderLocation.Local), "An Extra Field should have been added to the zip entry foo.txt");
				Assert.AreEqual (lastWrite, WithoutMilliseconds (entry.ModificationTime), $"Check 2 {WithoutMilliseconds (entry.ModificationTime)} != {lastWrite}");
			}
		}

		[Test]
		public void DateTimeConversion ()
		{
			DateTime now = DateTime.UtcNow;
			for (int m = 0; m < 59; m++)
			for (int s = 0; s < 59; s++)
			for (int ms = 0; ms < 999; ms++) {
				DateTime dt = new DateTime (now.Year, now.Month, now.Day, now.Hour, m, s, ms, DateTimeKind.Utc);
				var dateTimeOffset = new DateTimeOffset(dt);
				var unixTime =  (ulong)dateTimeOffset.ToUnixTimeSeconds();
				DateTime c = DateTimeOffset.FromUnixTimeSeconds ((long)unixTime).UtcDateTime;
				Assert.AreEqual (WithoutMilliseconds (dt), WithoutMilliseconds (c));

			}
		}

		[Test]
		public void AddStreamDateCheck ()
		{
			string root = Path.Combine (Path.GetDirectoryName (typeof (ZipTests).Assembly.Location), TestContext.CurrentContext.Test.Name);
			Directory.CreateDirectory (root);
			string zip = Path.Combine (root, "foo.zip");
			if (File.Exists (zip))
				File.Delete (zip);
			DateTime date = new DateTime (1992, 9, 1, 13,59, 34);
			using (var archive = ZipArchive.Open (zip, FileMode.Create)) {
				var ms = new MemoryStream (Encoding.ASCII.GetBytes (TEXT));
				ms.Position = 0;
				ZipEntry entry = archive.AddStream (ms, archivePath: "foomem.txt", modificationTime: date);
				Assert.AreEqual (date, WithoutMilliseconds (entry.ModificationTime), $"Check 1 {WithoutMilliseconds (entry.ModificationTime)} != {date}");
			}
			using (var archive = ZipArchive.Open (zip, FileMode.Open)) {
				ZipEntry entry = archive.ReadEntry ("foomem.txt");
				Assert.IsTrue (entry.ExtraFieldPresent (KnownExtraFields.ExtendedTimestamp, ZipHeaderLocation.Local), "An Extra Field should have been added to the zip entry foomem.txt");
				Assert.AreEqual (date, WithoutMilliseconds (entry.ModificationTime), $"Check 1 {WithoutMilliseconds (entry.ModificationTime)} != {date}");
			}
		}

		[Test]
		public void SmallTextFile ()
		{
			var zipStream = new MemoryStream ();
			var encoding = Encoding.UTF8;
			using (var zip = ZipArchive.Create (zipStream)) {
				zip.AddEntry ("foo", "bar", encoding);
			}
			using (var zip = ZipArchive.Open (zipStream)) {
				var entry = zip.ReadEntry ("foo");
				Assert.IsNotNull (entry, "Entry 'foo' should exist!");
				using (var stream = new MemoryStream ()) {
					entry.Extract (stream);
					stream.Position = 0;
					Assert.AreEqual ("bar", encoding.GetString (stream.ToArray ()));
				}
			}
		}

		[TestCase (CompressionMethod.Deflate)]
		[TestCase (CompressionMethod.Bzip2)]
		[TestCase (CompressionMethod.ZSTD)]
#if HAVE_XZ
		[TestCase (CompressionMethod.XZ)]
#endif
		public void UpdateEntryCompressionMethod (CompressionMethod method)
		{
			var zipStream = new MemoryStream ();
			var encoding = Encoding.UTF8;
			using (var zip = ZipArchive.Create (zipStream)) {
				zip.AddEntry ("foo", "bar", encoding, method);
			}
			using (var zip = ZipArchive.Open (zipStream)) {
				var entry = zip.ReadEntry ("foo");
				Assert.IsNotNull (entry, "Entry 'foo' should exist!");
				AssertEntryIsValid (entry, "foo", compression: method);
				using (var stream = new MemoryStream ()) {
					entry.Extract (stream);
					stream.Position = 0;
					Assert.AreEqual ("bar", encoding.GetString (stream.ToArray ()));
				}
				zip.AddEntry ("foo", "foo", encoding, CompressionMethod.Store);
				entry = zip.ReadEntry ("foo");
				AssertEntryIsValid (entry, "foo", compression: CompressionMethod.Store);
			}
		}

		[Test]
		public void CheckForUnknownCompressionMethods ()
		{
			string filePath = Path.GetFullPath ("packaged_resources");
			if (!File.Exists (filePath)) {
					filePath = Path.GetFullPath (Path.Combine ("LibZipSharp.UnitTest", "packaged_resources"));
			}
			using (var zip = ZipArchive.Open (filePath, FileMode.Open)) {
				foreach (var e in zip) {
					Console.WriteLine ($"{e.FullName} is {e.CompressionMethod}");
					Assert.AreNotEqual (CompressionMethod.Unknown, e.CompressionMethod, "Compression Method should not be Unknown.");
				}
			}
		}

		[Test]
		public void InMemoryZipFile ()
		{
			string filePath = Path.GetFullPath ("info.json");
			if (!File.Exists (filePath)) {
				filePath = Path.GetFullPath (Path.Combine ("LibZipSharp.UnitTest", "info.json"));
			}
			string fileRoot = Path.GetDirectoryName (filePath);
			using (var stream = new MemoryStream ()) {
				using (var zip = ZipArchive.Create (stream)) {
					zip.AddFile (filePath, "info.json");
					zip.AddFile (Path.Combine (fileRoot, "characters_players.json"), "characters_players.json");
					zip.AddFile (Path.Combine (fileRoot, "object_spawn.json"), "object_spawn.json");
				}

				stream.Position = 0;
				using (var zip = ZipArchive.Open (stream)) {
					Assert.AreEqual (3, zip.EntryCount);
					foreach (var e in zip) {
						Console.WriteLine (e.FullName);
					}
					zip.DeleteEntry ("info.json");
				}

				stream.Position = 0;
				using (var zip = ZipArchive.Open (stream)) {
					Assert.AreEqual (2, zip.EntryCount);
					zip.AddEntry ("info.json", File.ReadAllText (filePath), Encoding.UTF8, CompressionMethod.Deflate);
				}

				stream.Position = 0;
				using (var zip = ZipArchive.Open (stream)) {
					Assert.AreEqual (3, zip.EntryCount);
					Assert.IsTrue (zip.ContainsEntry ("info.json"));
					var entry1 = zip.ReadEntry ("info.json");
					using (var s = new MemoryStream ()) {
						entry1.Extract (s);
						s.Position = 0;
						using (var sr = new StreamReader (s)) {
							var t = sr.ReadToEnd ();
							Assert.AreEqual (File.ReadAllText (filePath), t);
						}

					}
				}
			}
		}

		[TestCase (false)]
		[TestCase (true)]
		public void EnumerateSkipDeletedEntries (bool deleteFromExistingFile)
		{
			var ms = new MemoryStream (Encoding.UTF8.GetBytes(TEXT));
			File.WriteAllText ("file1.txt", "1111");
			string filePath = Path.GetFullPath ("file1.txt");
			if (File.Exists ("test-archive-write.zip"))
				File.Delete ("test-archive-write.zip");

			ZipArchive zip = null;
			try {
				zip = ZipArchive.Open ("test-archive-write.zip", FileMode.CreateNew);

				ZipEntry e;
				e = zip.AddFile (filePath, "/path/ZipTestCopy1.exe");
				e = zip.AddFile (filePath, "/path/ZipTestCopy2.exe");
				var text = "Hello World";
				e = zip.AddEntry ("/data/foo1.txt", text, Encoding.UTF8, CompressionMethod.Store);
				e = zip.AddEntry ("/data/foo2.txt", File.OpenRead(filePath), CompressionMethod.Store);

				if (deleteFromExistingFile) {
					zip.Close ();
					zip = ZipArchive.Open ("test-archive-write.zip", FileMode.Open);
				}

				ValidateEnumeratedEntries (zip, "path/ZipTestCopy1.exe", "path/ZipTestCopy2.exe", "data/foo1.txt", "data/foo2.txt");

				// Delete first
				zip.DeleteEntry ("path/ZipTestCopy1.exe");
				ValidateEnumeratedEntries (zip, "path/ZipTestCopy2.exe", "data/foo1.txt", "data/foo2.txt");

				// Delete last
				zip.DeleteEntry ("data/foo2.txt");
				ValidateEnumeratedEntries (zip, "path/ZipTestCopy2.exe", "data/foo1.txt");

				// Delete middle
				zip.DeleteEntry ("path/ZipTestCopy2.exe");
				ValidateEnumeratedEntries (zip, "data/foo1.txt");

				// Delete all
				zip.DeleteEntry ("data/foo1.txt");
				ValidateEnumeratedEntries (zip);
			}
			finally {
				zip?.Dispose ();
			}
		}

		void ValidateEnumeratedEntries (ZipArchive zip, params string[] expectedEntries)
		{
			var actualEntries = new List<string>();
			foreach (var entry in zip) {
				actualEntries.Add (entry.FullName);
			}

			Assert.AreEqual (expectedEntries, actualEntries.ToArray ());
		}
	}
}
