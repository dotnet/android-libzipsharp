#include <cstring>
#include <bzlib.h>
#include <zlib.h>
#include <lzma.h>
#include <zipconf.h>

#include "version.hh"

constexpr char libzipsharp_version[] = LIBZIPSHARP_VERSION;
constexpr char libzip_version[] = LIBZIP_VERSION;
constexpr char libzlib_version[] = ZLIB_VERSION;
constexpr char libzlibng_version[] = ZLIBNG_VERSION;
constexpr char lzma_version[] = LZMA_VERSION_STRING;

void lzs_get_versions (LZSVersions *versions)
{
	if (versions == nullptr) {
		return;
	}

	versions->bzip2 = strdup (BZ2_bzlibVersion ());
	versions->libzip = strdup (libzip_version);
	versions->zlib = strdup (libzlib_version);
	versions->zlibng = strdup (libzlibng_version);
	versions->lzma = strdup (lzma_version);
	versions->libzipsharp = strdup (libzipsharp_version);
}
