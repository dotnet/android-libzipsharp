#include <cstdio>

#include "values.hh"

uint32_t lzs_convert_whence_value (int32_t whence)
{
	switch (whence) {
		case SEEK_SET:
			return LZS_SEEK_SET;

		case SEEK_CUR:
			return LZS_SEEK_CUR;

		case SEEK_END:
			return LZS_SEEK_END;

		default:
			return LZS_SEEK_INVALID;
	}
}
