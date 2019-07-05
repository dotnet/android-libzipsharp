using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using Xamarin.Tools.Zip;

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
	}
}