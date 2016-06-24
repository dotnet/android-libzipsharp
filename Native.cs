﻿//
// Native.cs
//
// Author:
//       Marek Habersack <grendel@twistedcode.net>
//       Dean Ellis <dellis1972@googlemail.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Runtime.InteropServices;

namespace Xamarin.ZipSharp
{
	internal class Native
	{
		[StructLayout (LayoutKind.Sequential)]
		public struct zip_error_t
		{
			public int zip_err;						/* libzip error code (ZIP_ER_*) */
			public int sys_err;						/* copy of errno (E*) or zlib error code */
			public IntPtr str;							/* string representation or NULL */
		};

		public struct zip_source_args_seek_t
		{
			public UInt64 offset;
			public int whence;
		};

		public struct zip_stat_t
		{
			public UInt64 valid;                 /* which fields have valid values */ 
			public IntPtr name;                  /* name of the file (char *) */ 
			public UInt64 index;                 /* index within archive */ 
			public UInt64 size;                  /* size of file (uncompressed) */ 
			public UInt64 comp_size;             /* size of file (compressed) */ 
			public IntPtr mtime;                 /* modification time (time_t) */ 
			public UInt32 crc;                   /* crc of file data */ 
			public Int16  comp_method;           /* compression method used */ 
			public UInt16 encryption_method;     /* encryption method used */ 
			public UInt32 flags;                 /* reserved for future use */ 
		};

		public const uint ZIP_STAT_NAME              = 0x0001u;
		public const uint ZIP_STAT_INDEX             = 0x0002u;
		public const uint ZIP_STAT_SIZE              = 0x0004u;
		public const uint ZIP_STAT_COMP_SIZE         = 0x0008u;
		public const uint ZIP_STAT_MTIME             = 0x0010u;
		public const uint ZIP_STAT_CRC               = 0x0020u;
		public const uint ZIP_STAT_COMP_METHOD       = 0x0040u;
		public const uint ZIP_STAT_ENCRYPTION_METHOD = 0x0080u;
		public const uint ZIP_STAT_FLAGS             = 0x0100u;

		[Flags]
		public enum zip_source_cmd
		{
			ZIP_SOURCE_OPEN,            /* prepare for reading */
			ZIP_SOURCE_READ,            /* read data */
			ZIP_SOURCE_CLOSE,           /* reading is done */
			ZIP_SOURCE_STAT,            /* get meta information */
			ZIP_SOURCE_ERROR,           /* get error information */
			ZIP_SOURCE_FREE,            /* cleanup and free resources */
			ZIP_SOURCE_SEEK,            /* set position for reading */
			ZIP_SOURCE_TELL,            /* get read position */
			ZIP_SOURCE_BEGIN_WRITE,     /* prepare for writing */
			ZIP_SOURCE_COMMIT_WRITE,    /* writing is done */
			ZIP_SOURCE_ROLLBACK_WRITE,  /* discard written changes */
			ZIP_SOURCE_WRITE,           /* write data */
			ZIP_SOURCE_SEEK_WRITE,      /* set position for writing */
			ZIP_SOURCE_TELL_WRITE,      /* get write position */
			ZIP_SOURCE_SUPPORTS,        /* check whether source supports command */
			ZIP_SOURCE_REMOVE,           /* remove file */
		};

		public delegate Int64 zip_source_callback (IntPtr state, IntPtr data, UInt64 len, zip_source_cmd cmd);

		public static int ZIP_SOURCE_MAKE_COMMAND_BITMASK (zip_source_cmd cmd)
		{
			return 1 << (int)cmd;
		}

		public static T ZIP_SOURCE_GET_ARGS<T> (IntPtr data, UInt64 len)
		{
			return Marshal.PtrToStructure<T> (data);
		}

		const string ZIP_LIBNAME = "libzip.dll";

		[DllImport (ZIP_LIBNAME, SetLastError = true)]
		public static extern IntPtr zip_open (string path, OpenFlags flags, out ErrorCode errorp);

		[DllImport (ZIP_LIBNAME, SetLastError = true)]
		public static extern IntPtr zip_open_from_source (IntPtr source, OpenFlags flags, out zip_error_t errorp);

		[DllImport (ZIP_LIBNAME, SetLastError = true)]
		public static extern void zip_stat_init ([In][Out] zip_stat_t st);

		[DllImport (ZIP_LIBNAME)]
		public static extern Int64 zip_name_locate (IntPtr archive, string fname, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_fopen (IntPtr archive, string fname, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_fopen_index (IntPtr archive, UInt64 index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_fopen_encrypted (IntPtr archive, string fname, OperatingSystem flags, string password);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_fopen_index_encrypted (IntPtr archive, UInt64 index, OperationFlags flags, string password);

		[DllImport (ZIP_LIBNAME)]
		public static extern Int64 zip_fread (IntPtr file, byte[] buf, UInt64 nbytes);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_fclose (IntPtr file);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_close (IntPtr archive);

		[DllImport (ZIP_LIBNAME)]
		public static extern void zip_discard (IntPtr archive);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_stat (IntPtr archive, string fname, OperationFlags flags, out zip_stat_t sb);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_stat_index (IntPtr archive, UInt64 index, OperationFlags flags, out zip_stat_t sb);

		[DllImport (ZIP_LIBNAME)]
		public static extern string zip_file_get_comment (IntPtr archive, UInt64 index, out UInt32 lenp, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern string zip_get_archive_comment (IntPtr archive, out int lenp, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_get_archive_flag (IntPtr archive, ArchiveGlobalFlags flag, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern string zip_get_name (IntPtr archive, UInt64 index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern Int64 zip_get_num_entries (IntPtr archive, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_set_default_password (IntPtr archive, string password);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_rename (IntPtr archive, UInt64 index, string name);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_delete (IntPtr archive, UInt64 index);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_unchange (IntPtr archive, UInt64 index);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_unchange_all (IntPtr archive);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_unchange_archive (IntPtr archive);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_file_extra_field_delete (IntPtr archive, UInt64 index, UInt16 extra_field_index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_file_extra_field_delete_by_id (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_file_extra_field_get (IntPtr archive, UInt64 index, UInt16 extra_field_index, out UInt16 idp, out UInt16 lenp, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_file_extra_field_get_by_id (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, out UInt16 lenp, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_file_extra_field_set (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, byte[] extra_field_data, UInt16 len, OperationFlags flags);

		public static int zip_file_extra_field_set (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, byte[] extra_field_data, OperationFlags flags)
		{
			return zip_file_extra_field_set (archive, index, extra_field_id, extra_field_index, extra_field_data, (UInt16)(extra_field_data == null ? 0 : extra_field_data.Length), flags);
		}

		[DllImport (ZIP_LIBNAME)]
		public static extern Int16 zip_file_extra_fields_count (IntPtr archive, UInt64 index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern Int16 zip_file_extra_fields_count_by_id (IntPtr archive, UInt64 index, UInt16 extra_field_id, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern string zip_file_strerror (IntPtr file);

		[DllImport (ZIP_LIBNAME, CharSet = CharSet.Ansi)]
		public static extern IntPtr zip_strerror (IntPtr archive);

		[DllImport (ZIP_LIBNAME)]
		public static extern void zip_error_init (IntPtr error);

		[DllImport (ZIP_LIBNAME)]
		public static extern void zip_error_init_with_code (IntPtr error, int ze);

		[DllImport (ZIP_LIBNAME)]
		public static extern void zip_error_fini (IntPtr ze);

		[DllImport (ZIP_LIBNAME)]
		public static extern string zip_error_strerror (IntPtr ze);

		[DllImport (ZIP_LIBNAME)]
		public static extern void zip_source_free (IntPtr source);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_source_zip (IntPtr archive, IntPtr srcarchive, UInt64 srcidx, OpenFlags flags, UInt64 start, UInt64 len);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_source_file (IntPtr archive, string fname, UInt64 start, Int64 len);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_source_file_create (string fname, UInt64 start, UInt64 len, out IntPtr error);

		[DllImport (ZIP_LIBNAME, SetLastError = true)]
		public static extern IntPtr zip_source_function (IntPtr source,
			[MarshalAs (UnmanagedType.FunctionPtr)]zip_source_callback callback, IntPtr user_data);

		[DllImport (ZIP_LIBNAME, SetLastError = true)]
		public static extern IntPtr zip_source_function_create (
			[MarshalAs (UnmanagedType.FunctionPtr)]zip_source_callback callback, IntPtr user_data, out zip_error_t errorp);

		[DllImport (ZIP_LIBNAME, SetLastError = true)]
		public static extern UInt64 zip_source_seek_compute_offset (UInt64 offset, UInt64 length, IntPtr data, UInt64 data_length, out zip_error_t error);

		[DllImport (ZIP_LIBNAME)]
		public static extern Int64 zip_dir_add (IntPtr archive, string name, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern Int64 zip_file_add (IntPtr archive, string name, IntPtr source, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_file_replace (IntPtr archive, UInt64 index, IntPtr source, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_set_file_comment (IntPtr archive, UInt64 index, string comment, UInt16 len, OperationFlags flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_set_file_compression (IntPtr archive, UInt64 index, CompressionMethod comp, UInt32 comp_flags);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_source_buffer (IntPtr archive, byte[] data, UInt64 len, int freep);

		public static unsafe IntPtr zip_source_buffer (IntPtr archive, byte [] data, int freep)
		{
			return zip_source_buffer (archive, data, (UInt64)(data == null ? 0 : data.Length), freep);
		}

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_source_buffer_create (byte[] data, UInt64 len, int freep, IntPtr error);

		public static IntPtr zip_source_buffer_create (byte [] data, int freep, IntPtr error)
		{
			return zip_source_buffer_create (data, (UInt64)(data == null ? 0 : data.Length), freep, error);
		}

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_file_get_external_attributes (IntPtr archive, UInt64 index, OperationFlags flags, out byte opsys, out UInt32 attributes);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_file_set_external_attributes (IntPtr archive, UInt64 index, OperationFlags flags, byte opsys, UInt32 attributes);

		[DllImport (ZIP_LIBNAME)]
		public static extern IntPtr zip_get_error (IntPtr archive);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_error_code_zip (IntPtr error);

		[DllImport (ZIP_LIBNAME)]
		public static extern int zip_error_code_system (IntPtr error);

		public static long zip_source_make_command_bitmap (params zip_source_cmd[] cmd)
		{
			if (cmd == null)
				throw new ArgumentNullException (nameof(cmd));
			int bitmap = 0;
			for (int i = 0; i < cmd.Length; i++) {
				bitmap |= ZIP_SOURCE_MAKE_COMMAND_BITMASK (cmd [i]);
			}
			return bitmap;
		}
	}
}
