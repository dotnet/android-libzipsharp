#if !defined (__LZS_NATIVE_VERSION_HH)
#define __LZS_NATIVE_VERSION_HH

#if defined(_MSC_VER)
#define LZS_API __declspec(dllexport)
#else
#if defined (__clang__) || defined (__GNUC__)
#define LZS_API __attribute__ ((__visibility__ ("default")))
#else
#define LZS_API
#endif
#endif

struct LZSVersions
{
	const char *bzip2;
	const char *libzip;
	const char *zlib;
	const char *zlibng;
	const char *lzma;
	const char *libzipsharp;
};

extern "C" {
	LZS_API void lzs_get_versions (LZSVersions *versions);
}
#endif // __LZS_NATIVE_VERSION_HH
