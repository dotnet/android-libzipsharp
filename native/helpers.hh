#if !defined (__LZS_HELPERS_HH)
#define __LZS_HELPERS_HH

#include <cstdlib>
#include <cstdint>

#if defined(_MSC_VER)
#define LZS_API __declspec(dllexport)
#else
#if defined (__clang__) || defined (__GNUC__)
#define LZS_API __attribute__ ((__visibility__ ("default")))
#else
#define LZS_API
#endif
#endif

#endif // __LZS_HELPERS_HH
