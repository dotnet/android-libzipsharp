using System;
using System.IO;

namespace Xamarin.Tools.Zip
{
	public partial class ZipArchive
	{
		static ZipArchive CreateArchiveInstance (string defaultExtractionDir, IPlatformOptions options)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				return new UnixZipArchive (defaultExtractionDir, EnsureOptions (options) as UnixPlatformOptions);
			}
			else {
				return new WindowsZipArchive (defaultExtractionDir, EnsureOptions (options) as WindowsPlatformOptions);
			}
		}

		static ZipArchive CreateInstanceFromStream (Stream stream, OpenFlags flags = OpenFlags.RDOnly, IPlatformOptions options = null)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				return new UnixZipArchive (stream, EnsureOptions (options) as UnixPlatformOptions, flags);
			}
			else {
				return new WindowsZipArchive (stream, EnsureOptions (options) as WindowsPlatformOptions, flags);
			}
		}

		static IPlatformOptions EnsureOptions (IPlatformOptions options)
		{
			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				if (options == null)
					return new UnixPlatformOptions ();
				else {
					var opts = options as UnixPlatformOptions;
					if (opts == null)
						throw new ArgumentException ("must be an instance of UnixPlatformOptions", nameof (options));
					return opts;
				}
			}
			else {
				if (options == null)
					return new WindowsPlatformOptions ();
				else {
					var opts = options as WindowsPlatformOptions;
					if (opts == null)
						throw new ArgumentException ("must be an instance of WindowsPlatformOptions", nameof (options));
					return opts;
				}
			}

		}
	}
}

