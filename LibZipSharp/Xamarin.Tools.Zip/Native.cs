//
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

[assembly: DefaultDllImportSearchPathsAttribute(DllImportSearchPath.SafeDirectories | DllImportSearchPath.AssemblyDirectory)]

namespace Xamarin.Tools.Zip
{
	internal class Native
	{
		[StructLayout (LayoutKind.Sequential)]
		public struct LZSVersions
		{
			public string bzip2;
			public string libzip;
			public string zlib;
			public string zlibng;
			public string lzma;
			public string libzipsharp;
		};

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

		[StructLayout(LayoutKind.Explicit)]
		public struct zip_stat_t
		{
			[FieldOffset (0)]
			public UInt64 valid;                 /* which fields have valid values */
			[FieldOffset (8)]
			public IntPtr name;                  /* name of the file (char *) */
			[FieldOffset (16)]
			public UInt64 index;                 /* index within archive */
			[FieldOffset (24)]
			public UInt64 size;                  /* size of file (uncompressed) */
			[FieldOffset (32)]
			public UInt64 comp_size;             /* size of file (compressed) */
			[FieldOffset (40)]
			public IntPtr mtime;                 /* modification time (time_t) */
			[FieldOffset (48)]
			public UInt32 crc;                   /* crc of file data */
			[FieldOffset (52)]
			public Int16  comp_method;           /* compression method used */
			[FieldOffset (56)]
			public UInt16 encryption_method;     /* encryption method used */
			[FieldOffset (60)]
			public UInt32 flags;                 /* reserved for future use */
		};

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		public delegate Int64 zip_source_callback (IntPtr state, IntPtr data, UInt64 len, SourceCommand cmd);

		public static int ZipSourceMakeCommandBitmask (SourceCommand cmd)
		{
			return 1 << (int)cmd;
		}

		public static T ZipSourceGetArgs<T> (IntPtr data, UInt64 len)
		{
			return (T)Marshal.PtrToStructure (data, typeof (T));
		}

		const string ZIP_LIBNAME = "libZipSharpNative";

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		static extern void lzs_get_versions (out LZSVersions versions);

		public static Versions get_versions ()
		{
			lzs_get_versions (out LZSVersions ret);
			return new Versions {
				BZip2 = ret.bzip2 ?? String.Empty,
				LibZip = ret.libzip ?? String.Empty,
				Zlib = ret.zlib ?? String.Empty,
				ZlibNG = ret.zlibng ?? String.Empty,
				LZMA = ret.lzma ?? String.Empty,
				LibZipSharp = ret.libzipsharp ?? String.Empty
			};
		}

		[DllImport (ZIP_LIBNAME, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_open (IntPtr path, OpenFlags flags, out ErrorCode errorp);

		public static IntPtr zip_open (string path, OpenFlags flags, out ErrorCode errorp)
		{
			IntPtr utfPath = Utilities.StringToUtf8StringPtr (path);
			try {
				return zip_open (utfPath, flags, out errorp);
			} finally {
				Utilities.FreeUtf8StringPtr (utfPath);
			}
		}

		[DllImport (ZIP_LIBNAME, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_open_from_source (IntPtr source, OpenFlags flags, out zip_error_t errorp);

		[DllImport (ZIP_LIBNAME, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void zip_stat_init ([In][Out] zip_stat_t st);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 zip_name_locate (IntPtr archive, IntPtr fname, OperationFlags flags);

		public static Int64 zip_name_locate (IntPtr archive, string fname, OperationFlags flags)
		{
			var utfFname = Utilities.StringToUtf8StringPtr (fname);
			try {
				return zip_name_locate (archive, utfFname, flags);
			} finally {
				Utilities.FreeUtf8StringPtr (utfFname);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_fopen (IntPtr archive, IntPtr fname, OperationFlags flags);

		public static IntPtr zip_fopen (IntPtr archive, string fname, OperationFlags flags)
		{
			var utfFname = Utilities.StringToUtf8StringPtr (fname);
			try {
				return zip_fopen (archive, utfFname, flags);
			} finally {
				Utilities.FreeUtf8StringPtr (utfFname);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_fopen_index (IntPtr archive, UInt64 index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_fopen_encrypted (IntPtr archive, IntPtr fname, OperatingSystem flags, string password);

		public static IntPtr zip_fopen_encrypted (IntPtr archive, string fname, OperatingSystem flags, string password)
		{
			var utfFname = Utilities.StringToUtf8StringPtr (fname);
			try {
				return zip_fopen_encrypted (archive, utfFname, flags, password);
			} finally {
				Utilities.FreeUtf8StringPtr (utfFname);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_fopen_index_encrypted (IntPtr archive, UInt64 index, OperationFlags flags, string password);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 zip_fread (IntPtr file, byte[] buf, UInt64 nbytes);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_fclose (IntPtr file);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_close (IntPtr archive);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void zip_discard (IntPtr archive);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_stat (IntPtr archive, IntPtr fname, OperationFlags flags, out zip_stat_t sb);

		public static int zip_stat (IntPtr archive, string fname, OperationFlags flags, out zip_stat_t sb)
		{
			var utfFname = Utilities.StringToUtf8StringPtr (fname);
			try {
				return zip_stat (archive, utfFname, flags, out sb);
			} finally {
				Utilities.FreeUtf8StringPtr (utfFname);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_stat_index (IntPtr archive, UInt64 index, OperationFlags flags, out zip_stat_t sb);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint="zip_file_get_comment")]
		public static extern IntPtr zip_file_get_comment_ptr (IntPtr archive, UInt64 index, out UInt32 lenp, OperationFlags flags);

		public static string zip_file_get_comment (IntPtr archive, UInt64 index, out UInt32 lenp, OperationFlags flags)
		{
			return Utilities.Utf8StringPtrToString (zip_file_get_comment_ptr (archive, index, out lenp, flags));
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint="zip_get_archive_comment")]
		public static extern IntPtr zip_get_archive_comment_ptr (IntPtr archive, out int lenp, OperationFlags flags);

		public static string zip_get_archive_comment (IntPtr archive, out int lenp, OperationFlags flags)
		{
			return Utilities.Utf8StringPtrToString (zip_get_archive_comment_ptr (archive, out lenp, flags));
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_get_archive_flag (IntPtr archive, ArchiveGlobalFlags flag, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint="zip_get_name")]
		public static extern IntPtr zip_get_name_ptr (IntPtr archive, UInt64 index, OperationFlags flags);

		public static string zip_get_name (IntPtr archive, UInt64 index, OperationFlags flags)
		{
			return Utilities.Utf8StringPtrToString (zip_get_name_ptr (archive, index, flags));
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 zip_get_num_entries (IntPtr archive, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_set_default_password (IntPtr archive, string password);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_rename (IntPtr archive, UInt64 index, IntPtr name);

		public static int zip_rename (IntPtr archive, UInt64 index, string name)
		{
			var utfName = Utilities.StringToUtf8StringPtr (name);
			try {
				return zip_rename (archive, index, utfName);
			} finally {
				Utilities.FreeUtf8StringPtr (utfName);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_delete (IntPtr archive, UInt64 index);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_unchange (IntPtr archive, UInt64 index);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_unchange_all (IntPtr archive);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_unchange_archive (IntPtr archive);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_file_extra_field_delete (IntPtr archive, UInt64 index, UInt16 extra_field_index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_file_extra_field_delete_by_id (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_file_extra_field_get (IntPtr archive, UInt64 index, UInt16 extra_field_index, out UInt16 idp, out UInt16 lenp, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_file_extra_field_get_by_id (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, out UInt16 lenp, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		static extern int zip_file_extra_field_set (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, byte[] extra_field_data, UInt16 len, OperationFlags flags);

		public static int zip_file_extra_field_set (IntPtr archive, UInt64 index, UInt16 extra_field_id, UInt16 extra_field_index, byte[] extra_field_data, OperationFlags flags)
		{
			return zip_file_extra_field_set (archive, index, extra_field_id, extra_field_index, extra_field_data, (UInt16)(extra_field_data.Length), flags);
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int16 zip_file_extra_fields_count (IntPtr archive, UInt64 index, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int16 zip_file_extra_fields_count_by_id (IntPtr archive, UInt64 index, UInt16 extra_field_id, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl, EntryPoint="zip_file_strerror")]
		public static extern IntPtr zip_file_strerror_ptr (IntPtr file);

		public static string zip_file_strerror (IntPtr file)
		{
			return Utilities.Utf8StringPtrToString (zip_file_strerror_ptr (file));
		}

		[DllImport (ZIP_LIBNAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_strerror (IntPtr archive);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void zip_error_init (IntPtr error);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void zip_error_init_with_code (IntPtr error, int ze);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void zip_error_fini (IntPtr ze);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_error_strerror (IntPtr ze);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void zip_source_free (IntPtr source);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_zip (IntPtr archive, IntPtr srcarchive, UInt64 srcidx, OpenFlags flags, UInt64 start, UInt64 len);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_file (IntPtr archive, IntPtr fname, UInt64 start, Int64 len);

		public static IntPtr zip_source_file (IntPtr archive, string fname, UInt64 start, Int64 len)
		{
			IntPtr utfFname = Utilities.StringToUtf8StringPtr (fname);
			try {
				return Native.zip_source_file (archive, utfFname, start, len);
			} finally {
				Utilities.FreeUtf8StringPtr (utfFname);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_file_create (IntPtr fname, UInt64 start, UInt64 len, out IntPtr error);

		public static IntPtr zip_source_file_create (string fname, UInt64 start, UInt64 len, out IntPtr error)
		{
			IntPtr utfFname = Utilities.StringToUtf8StringPtr (fname);
			try {
				return zip_source_file_create (utfFname, start, len, out error);
			} finally {
				Utilities.FreeUtf8StringPtr (utfFname);
			}
		}

		[DllImport (ZIP_LIBNAME, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_function (IntPtr source,
			[MarshalAs (UnmanagedType.FunctionPtr)]zip_source_callback callback, IntPtr user_data);

		[DllImport (ZIP_LIBNAME, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_function_create (
			[MarshalAs (UnmanagedType.FunctionPtr)]zip_source_callback callback, IntPtr user_data, out zip_error_t errorp);

		[DllImport (ZIP_LIBNAME, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern UInt64 zip_source_seek_compute_offset (UInt64 offset, UInt64 length, IntPtr data, UInt64 data_length, out zip_error_t error);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 zip_dir_add (IntPtr archive, IntPtr name, OperationFlags flags);

		public static Int64 zip_dir_add (IntPtr archive, string name, OperationFlags flags)
		{
			IntPtr utfName = Utilities.StringToUtf8StringPtr (name);
			try {
				return zip_dir_add (archive, utfName, flags);
			} finally {
				Utilities.FreeUtf8StringPtr (utfName);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern Int64 zip_file_add (IntPtr archive, IntPtr name, IntPtr source, OperationFlags flags);

		public static Int64 zip_file_add (IntPtr archive, string name, IntPtr source, OperationFlags flags)
		{
			IntPtr utfName = Utilities.StringToUtf8StringPtr (name);
			try {
				return zip_file_add (archive, utfName, source, flags);
			} finally {
				Utilities.FreeUtf8StringPtr (utfName);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_file_replace (IntPtr archive, UInt64 index, IntPtr source, OperationFlags flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_set_file_comment (IntPtr archive, UInt64 index, IntPtr comment, UInt16 len, OperationFlags flags);

		public static int zip_set_file_comment (IntPtr archive, UInt64 index, string comment, UInt16 len, OperationFlags flags)
		{
			IntPtr utfComment = Utilities.StringToUtf8StringPtr (comment);
			try {
				return zip_set_file_comment (archive, index, utfComment, len, flags);
			} finally {
				Utilities.FreeUtf8StringPtr (utfComment);
			}
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_set_file_compression (IntPtr archive, UInt64 index, CompressionMethod comp, UInt32 comp_flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_file_set_mtime(IntPtr archive, UInt64 index, ulong mtime, UInt32 flags);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_buffer (IntPtr archive, byte[] data, UInt64 len, int freep);

		public static unsafe IntPtr zip_source_buffer (IntPtr archive, byte [] data, int freep)
		{
			return zip_source_buffer (archive, data, (UInt64)(data == null ? 0 : data.Length), freep);
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_source_buffer_create (byte[] data, UInt64 len, int freep, IntPtr error);

		public static IntPtr zip_source_buffer_create (byte [] data, int freep, IntPtr error)
		{
			return zip_source_buffer_create (data, (UInt64)(data == null ? 0 : data.Length), freep, error);
		}

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_file_get_external_attributes (IntPtr archive, UInt64 index, OperationFlags flags, out byte opsys, out UInt32 attributes);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_file_set_external_attributes (IntPtr archive, UInt64 index, OperationFlags flags, byte opsys, UInt32 attributes);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zip_get_error (IntPtr archive);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_error_code_zip (IntPtr error);

		[DllImport (ZIP_LIBNAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int zip_error_code_system (IntPtr error);

		public static long zip_source_make_command_bitmap (params SourceCommand[] cmd)
		{
			if (cmd == null)
				throw new ArgumentNullException (nameof(cmd));
			int bitmap = 0;
			for (int i = 0; i < cmd.Length; i++) {
				bitmap |= ZipSourceMakeCommandBitmask (cmd [i]);
			}
			return bitmap;
		}

		[DllImport ("kernel32.dll", SetLastError = true)]
		private static extern bool SetDllDirectory (string lpPathName);

		static Native ()
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				string executingDirectory = System.IO.Path.GetDirectoryName (typeof(Native).Assembly.Location);
				SetDllDirectory (Environment.Is64BitProcess ? System.IO.Path.Combine (executingDirectory, "lib64") : executingDirectory);
			}
		}
	}
}
