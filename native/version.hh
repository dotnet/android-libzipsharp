#if !defined (__LZS_NATIVE_VERSION_HH)
#define __LZS_NATIVE_VERSION_HH

#include "helpers.hh"

struct LZSVersions
{
	const char *bzip2;
	const char *libzip;
	const char *zlib;
	const char *zlibng;
	const char *zstd;
	const char *lzma;
	const char *libzipsharp;
};

extern "C" {
	LZS_API void lzs_get_versions (LZSVersions *versions);
}
#endif // __LZS_NATIVE_VERSION_HH
