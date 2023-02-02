#include <zip.h>

#include "sizes.hh"

uint64_t lzs_get_size_zip_source_args_seek ()
{
	return static_cast<uint64_t>(sizeof (zip_source_args_seek_t));
}
