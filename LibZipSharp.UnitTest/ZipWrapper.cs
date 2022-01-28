using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Xamarin.Tools.Zip;
using Unix = Mono.Unix;

namespace Tests {
	public class ZipWrapper : IDisposable {
		ZipArchive archive;
		string filename;
		public ZipArchive Archive => archive;

		public ZipWrapper (string file, FileMode mode = FileMode.CreateNew) {
			filename = file;
			archive = ZipArchive.Open (filename, mode);
		}

		public void Flush () {
			if (archive != null) {
				archive.Close ();
				archive.Dispose ();
				archive = null;
			}
			archive = ZipArchive.Open (filename, FileMode.Open);
		}

		/// <summary>
		/// HACK: aapt2 is creating zip entries on Windows such as `assets\subfolder/asset2.txt`
		/// </summary>
		public void FixupWindowsPathSeparators (Action<string, string> onRename)
		{
			bool modified = false;
			foreach (var entry in archive) {
				if (entry.FullName.Contains ("\\")) {
					var name = entry.FullName.Replace ('\\', '/');
					onRename?.Invoke (entry.FullName, name);
					entry.Rename (name);
					modified = true;
				}
			}
			if (modified) {
				Flush ();
			}
		}

		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				if (archive != null) {
					archive.Close ();
					archive.Dispose ();
					archive = null;
				}
			}
		}
	}
}
