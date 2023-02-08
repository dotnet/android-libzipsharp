#if !defined (__LZS_VALUES_HH)
#define __LZS_VALUES_HH

#include "helpers.hh"

// Values here must be identical to those in LibZipSharp/Xamarin.Tools.Zip/Native.cs
constexpr uint32_t LZS_SEEK_SET     = 0;
constexpr uint32_t LZS_SEEK_CUR     = 1;
constexpr uint32_t LZS_SEEK_END     = 2;
constexpr uint32_t LZS_SEEK_INVALID = 0xDEADBEEF;

extern "C" {
	LZS_API uint32_t lzs_convert_whence_value (int32_t whence);
}
#endif // __LZS_VALUES_HH
