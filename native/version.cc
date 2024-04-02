#include <cstring>
#include <bzlib.h>
#include <zlib.h>
#include <zstd.h>
#include <zipconf.h>

#include "version.hh"

constexpr char libzipsharp_version[] = LIBZIPSHARP_VERSION;
constexpr char libzip_version[] = LIBZIP_VERSION;
constexpr char libzlib_version[] = ZLIB_VERSION;
#if defined (ZLIBNG_VERSION)
constexpr char libzlibng_version[] = ZLIBNG_VERSION;
#else
constexpr char libzlibng_version[] = "not used";
#endif // ndef ZLIBNG_VERSION

void lzs_get_versions (LZSVersions *versions)
{
	if (versions == nullptr) {
		return;
	}

	versions->bzip2 = strdup (BZ2_bzlibVersion ());
	versions->libzip = strdup (libzip_version);
	versions->zlib = strdup (libzlib_version);
	versions->zlibng = strdup (libzlibng_version);
	versions->zstd = strdup (ZSTD_versionString ());
	versions->libzipsharp = strdup (libzipsharp_version);
}
