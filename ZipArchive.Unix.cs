using System;
using System.IO;

using Mono.Unix.Native;

namespace Xamarin.Tools.Zip
{
	public partial class ZipArchive
	{
		static ZipArchive CreateArchiveInstance (string defaultExtractionDir, IPlatformOptions options)
		{
			return new UnixZipArchive (defaultExtractionDir, EnsureOptions (options));
		}

		static ZipArchive CreateInstanceFromStream (Stream stream, OpenFlags flags = OpenFlags.RDOnly, IPlatformOptions options = null)
		{
			return new UnixZipArchive (stream, EnsureOptions (options), flags);
		}

		static UnixPlatformOptions EnsureOptions (IPlatformOptions options)
		{
			if (options == null)
				return new UnixPlatformOptions ();
			else {
				var opts = options as UnixPlatformOptions;
				if (opts == null)
					throw new ArgumentException ("must be an instance of UnixPlatformOptions", nameof (options));
				return opts;
			}
		}
	}
}

