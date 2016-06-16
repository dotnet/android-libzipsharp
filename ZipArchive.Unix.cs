using System;
using System.IO;

using Mono.Unix.Native;

namespace Xamarin.ZipSharp
{
	public partial class ZipArchive
	{
		static ZipArchive CreateArchiveInstance (string defaultExtractionDir, IPlatformOptions options)
		{
			UnixPlatformOptions opts;
			if (options == null)
				opts = new UnixPlatformOptions ();
			else {
				opts = options as UnixPlatformOptions;
				if (opts == null)
					throw new ArgumentException ("must be an instance of UnixPlatformOptions", nameof (options));
			}

			return new UnixZipArchive (defaultExtractionDir, opts);
		}
	}
}

