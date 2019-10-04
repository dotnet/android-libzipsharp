using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xamarin.Tools.Zip;
using Xamarin.Tools.Zip.Ex;

namespace Tests {

	public class ZipArchiveExTest {

		Random rnd = new Random (234187614);

		void CreateBaseFilesDirectory ()
		{
			if (Directory.Exists ("Resources"))
				Directory.Delete ("Resources", recursive: true);
			Directory.CreateDirectory ("Resources");
			Directory.CreateDirectory (Path.Combine ("Resources", "subfolder"));
			
			for (int i = 0; i < 500; i++) {
				int fileSize = rnd.Next (short.MaxValue*4);
				byte [] buffer = new byte [fileSize];
				rnd.NextBytes (buffer);
				File.WriteAllBytes (Path.Combine ("Resources", $"File{i}.txt"), buffer);
			}
			for (int i = 0; i < 500; i++) {
				int fileSize = rnd.Next (short.MaxValue);
				byte [] buffer = new byte [fileSize];
				rnd.NextBytes (buffer);
				File.WriteAllBytes (Path.Combine ("Resources", "subfolder", $"FileSub{i}.txt"), buffer);
			}
		}

		void CreateNewFilesDirectory ()
		{
			if (Directory.Exists ("NewFiles"))
				Directory.Delete ("NewFiles", recursive: true);
			Directory.CreateDirectory ("NewFiles");
			Directory.CreateDirectory (Path.Combine ("NewFiles", "NewFiles1"));
			for (int i = 0; i < 500; i++) {
				int fileSize = rnd.Next (short.MaxValue * 4);
				byte [] buffer = new byte [fileSize];
				rnd.NextBytes (buffer);
				File.WriteAllBytes (Path.Combine ("NewFiles", $"File{i}.txt"), buffer);
			}
			for (int i = 0; i < 500; i++) {
				int fileSize = rnd.Next (short.MaxValue);
				byte [] buffer = new byte [fileSize];
				rnd.NextBytes (buffer);
				File.WriteAllBytes (Path.Combine ("NewFiles", "NewFiles1", $"FileSub{i}.txt"), buffer);
			}
		}

		CompressionMethod GetCompressionMethod (string fileName)
		{
			if (fileName.Contains ("5"))
				return CompressionMethod.Store;
			return CompressionMethod.Deflate;
		}

		[Test]
		public void CreateaValidZipArchive ()
		{
			string package_base = "package_base.zip";
			string temp = "package_base_new.zip";
			if (File.Exists (package_base))
				File.Delete (package_base);
			if (File.Exists (temp))
				File.Delete (temp);

			CreateBaseFilesDirectory ();
			using (var file = File.OpenWrite (package_base)) {
				using (var zip = ZipArchive.Create (file)) {
					zip.AddDirectory ("Resources", "res");
				}
			}
			string extract_base = Path.GetFullPath ("extract_base");
			if (Directory.Exists (extract_base))
				Directory.Delete (extract_base, recursive: true);
			Directory.CreateDirectory (extract_base);
			using (var archive = ZipArchive.Open (package_base, FileMode.Open, strictConsistencyChecks: true)) {
				archive.ExtractAll (extract_base);
				foreach (var entry in archive) {
					Assert.IsNotNull (entry, "Entry should not be null");
					string destinationFile = Path.Combine (extract_base, entry.FullName);
					if (entry.IsDirectory) {
						DirectoryAssert.Exists (destinationFile, $"Directory {entry.FullName} should have been created.");
					} else {
						FileAssert.Exists (destinationFile, $"File {entry.FullName} should have been extracted.");
						FileInfo info = new FileInfo (destinationFile);
						Assert.AreEqual (info.Length, entry.Size, $"file size for {entry.FullName} ({entry.Size}) is not match {destinationFile} ({info.Length})");
					}
				}
			}
			File.Copy (package_base, temp, overwrite: true);
			CreateNewFilesDirectory ();
			using (var notice = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("NOTICE.txt")) {
				using (var zip = new ZipArchiveEx (temp, FileMode.Open)) {
					zip.FixupWindowsPathSeparators ((a, b) => TestContext.WriteLine ($"Fixing up malformed entry `{a}` -> `{b}`"));
					zip.Archive.AddEntry ("NOTICE", notice);

					int count = 0;
					string root = Path.GetFullPath ("NewFiles");
					foreach (var file in Directory.GetFiles ("NewFiles", "*.txt")) {
						zip.Archive.AddEntry (file.Replace (root, string.Empty), File.OpenRead (file), GetCompressionMethod (file));
						if (count == ZipArchiveEx.ZipFlushLimit) {
							zip.Flush ();
							count = 0;
						}
					}
				}
			}
			string extract_new = Path.GetFullPath ("extract_new");
			if (Directory.Exists (extract_new))
				Directory.Delete (extract_new, recursive: true);
			Directory.CreateDirectory (extract_new);
			using (var archive = ZipArchive.Open (temp, FileMode.Open, strictConsistencyChecks: true)) {
				archive.ExtractAll (extract_new);
				foreach (var entry in archive) {
					Assert.IsNotNull (entry, "Entry should not be null");
					string destinationFile = Path.Combine (extract_new, entry.FullName);
					if (entry.IsDirectory) {
						DirectoryAssert.Exists (destinationFile, $"Directory {entry.FullName} should have been created.");
					} else {
						FileAssert.Exists (destinationFile, $"File {entry.FullName} should have been extracted.");
						FileInfo info = new FileInfo (destinationFile);
						Assert.AreEqual (info.Length, entry.Size, $"file size for {entry.FullName} ({entry.Size}) is not match {destinationFile} ({info.Length})");
						CompressionMethod expected = GetCompressionMethod (entry.FullName);
						if (entry.FullName.StartsWith ("res", StringComparison.OrdinalIgnoreCase)) {
							expected = entry.CompressionMethod;
						}
						Assert.AreEqual (expected, entry.CompressionMethod, $"{entry.FullName} was compressed with {entry.CompressionMethod} but should have been compressed as {expected}.");
					}
				}
			}
			if (Directory.Exists ("Resources"))
				Directory.Delete ("Resources", recursive: true);
			if (Directory.Exists ("NewFiles"))
				Directory.Delete ("NewFiles", recursive: true);
			if (Directory.Exists (extract_base))
				Directory.Delete (extract_base, recursive: true);
			if (Directory.Exists (extract_new))
				Directory.Delete (extract_new, recursive: true);
			if (File.Exists (package_base))
				File.Delete (package_base);
			if (File.Exists (temp))
				File.Delete (temp);
		}
	}
}
