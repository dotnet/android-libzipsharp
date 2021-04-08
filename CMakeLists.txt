cmake_minimum_required(VERSION 3.18)

#
# TODO:
#
#  * Windows: use vcpkg to build bzip2 and lzma (vcpkg can be used as a cmake subproject)
#  * Include portions of Mono.Posix native code (if necessary and when the new Mono.Posix is ready)
#  * Add support Android builds
#  * Add support for iOS/tvOS/macCatalyst builds
#  * Decide what to do with crypto support
#

option(BUILD_DEPENDENCIES "Build only libzip dependencies" OFF)
option(BUILD_LIBZIP "Build libzip and libZipSharp" OFF)

set(CMAKE_POSITION_INDEPENDENT_CODE True CACHE BOOL "Always build position independent code" FORCE)

#
# Options for subprojects
#

#
# libzip, zlib-ng, xz
#
set(BUILD_SHARED_LIBS OFF CACHE BOOL "Build only as a static library" FORCE)

#
# libzip
#
set(CMAKE_POLICY_DEFAULT_CMP0074 "NEW" CACHE STRING "" FORCE)

#
# We need the static libzip library but we don't want to hide the symbols, so this policy must be set in
# a way that makes cmake ignore the value of C_VISIBILITY_PRESET which libzip's CMakeLists.txt sets to "hidden"
# when building the static library.
#
set(CMAKE_POLICY_DEFAULT_CMP0063 "OLD" CACHE STRING "" FORCE)

set(BUILD_TOOLS OFF CACHE BOOL "Do not build libzip tools" FORCE)
set(BUILD_REGRESS OFF CACHE BOOL "Do not build libzip regression tests" FORCE)
set(BUILD_EXAMPLES OFF CACHE BOOL "Do not build libzip examples" FORCE)
set(BUILD_DOC OFF CACHE BOOL "Do not build libzip documentation" FORCE)
set(ENABLE_GNUTLS OFF CACHE BOOL "Do not use gnutls for libzip" FORCE)
set(ENABLE_OPENSSL OFF CACHE BOOL "Do not use OpenSSL for libzip" FORCE)
set(ENABLE_MBEDTLS OFF CACHE BOOL "Do not use mbedtls for libzip" FORCE)
set(ENABLE_WINDOWS_CRYPTO OFF CACHE BOOL "Do not use Windows Crypto" FORCE)

#
# zlib-ng
#
set(ZLIB_COMPAT ON CACHE BOOL "Build zlib-ng for compatibility with zlib" FORCE)
set(ZLIB_ENABLE_TESTS OFF CACHE BOOL "Do not build zlib-ng tests" FORCE)

#
# Read product version
#
file(STRINGS "LibZipSharp.props" LZS_PRODUCT_VERSION_XML REGEX "^[ \t]*<_LibZipSharpNugetVersion>(.*)</_LibZipSharpNugetVersion>")
string(REGEX REPLACE "^[ \t]*<_LibZipSharpNugetVersion>(.*)</_LibZipSharpNugetVersion>" "\\1" LZS_VERSION "${LZS_PRODUCT_VERSION_XML}")

if(WIN32)
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded" CACHE STRING "Statically link MSVC runtime library")
  set(CMAKE_TOOLCHAIN_FILE ${CMAKE_SOURCE_DIR}/external/vcpkg/scripts/buildsystems/vcpkg.cmake CACHE STRING "Vcpkg toolchain file")
endif()

project(
  ZipSharpNative
  VERSION ${LZS_VERSION}
  DESCRIPTION "LibZipSharp, a managed wrapper around the libzip library"
  HOMEPAGE_URL "https://github.com/xamarin/LibZipSharp"
  LANGUAGES CXX C
  )

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

include(CheckCXXCompilerFlag)
include(CheckCCompilerFlag)
include(CheckLinkerFlag)

if((NOT BUILD_DEPENDENCIES) AND (NOT BUILD_LIBZIP))
  message(FATAL_ERROR "Either -DBUILD_DEPENDENCIES=ON or -DBUILD_LIBZIP=ON must be used when invoking cmake for this script")
endif()

if(BUILD_DEPENDENCIES AND BUILD_LIBZIP)
  message(FATAL_ERROR "Only one of the -DBUILD_DEPENDENCIES=ON or -DBUILD_LIBZIP=ON arguments may be present when invoking cmake for this script")
endif()

if(CMAKE_BUILD_TYPE IN_LIST DEBUG_CONFIGURATIONS)
  set(IS_DEBUG True)
else()
  set(IS_DEBUG False)
endif()

set(IS_LINUX False)
set(IS_BSD False)
set(IS_MACOS False)
set(IS_CATALYST False)
set(IS_IOS False)
set(IS_TVOS False)
set(IS_ANDROID False)

if(NOT CMAKE_CROSSCOMPILING)
  if(CMAKE_HOST_SYSTEM_NAME STREQUAL Linux)
    set(IS_LINUX True)
  endif()

  if(APPLE)
    set(IS_MACOS True)
  endif()
else()
  if(APPLE)
    if(CMAKE_SYSTEM_NAME STREQUAL iOS)
      set(IS_IOS True)
    elseif(CMAKE_SYSTEM_NAME STREQUAL tvOS)
      set(IS_TVOS True)
    elseif(CMAKE_SYSTEM_NAME STREQUAL watchOS)
      message(FATAL_ERROR "watchOS not supported")
    elseif(CMAKE_SYSTEM_NAME STREQUAL Darwin)
      set(IS_CATALYST True)
    else()
      message(FATAL_ERROR "Unknown system when cross-compiling: ${CMAKE_SYSTEM_NAME}")
    endif()
  endif()

  if(ANDROID)
    set(IS_ANDROID True)
  endif()
endif()

if(APPLE AND BUILD_DEPENDENCIES)
  # Builds with more than one architecture will fail
  if(CMAKE_OSX_ARCHITECTURES)
    list(LENGTH CMAKE_OSX_ARCHITECTURES OSX_ARCH_LEN)
    if(OSX_ARCH_LEN GREATER 1)
      message(FATAL_ERROR "Only one architecture may be specified in CMAKE_OSX_ARCHITECTURES for dependencies build")
    endif()
  endif()
endif()

macro(_compiler_has_flag _lang _flag)
  string(REGEX REPLACE "-|,|=" "_" _flag_name ${_flag})
  string(TOUPPER "${_lang}" _lang_upper)

  cmake_language(CALL check_${_lang}_compiler_flag "${_flag}" HAS_${_flag_name}_${_lang_upper})
  if(HAS_${_flag_name}_${_lang_upper})
    set(COMPILER_FLAG_FOUND True)
  else()
    set(COMPILER_FLAG_FOUND False)
  endif()
endmacro()

macro(cxx_compiler_has_flag _flag)
  _compiler_has_flag(cxx ${_flag})
endmacro()

macro(c_compiler_has_flag _flag)
  _compiler_has_flag(c ${_flag})
endmacro()

macro(_linker_has_flag _lang _flag)
  string(REGEX REPLACE "-|,|=" "_" _flag_name ${_flag})
  string(TOUPPER "${_lang}" _lang_upper)

  check_linker_flag(${_lang} "${_flag}" HAS_${_flag_name}_LINKER_${_lang_upper})
  if(HAS_${_flag_name}_LINKER_${_lang_upper})
    set(LINKER_FLAG_FOUND True)
  else()
    set(LINKER_FLAG_FOUND False)
  endif()
endmacro()

macro(cxx_linker_has_flag _flag)
  _linker_has_flag(CXX ${_flag})
endmacro()

if(UNIX)
  set(COMMON_COMPILE_OPTIONS
    -fstack-protector-strong
    -fno-strict-aliasing
    -ffunction-sections
    -funswitch-loops
    -finline-limit=300
    -Wa,-noexecstack
    )

  if(IS_DEBUG)
    list(APPEND COMMON_COMPILE_OPTIONS -O0)
  else()
    list(APPEND COMMON_COMPILE_OPTIONS -O2)
  endif()

  set(LINKER_OPTIONS
    -rdynamic
    -static-libgcc
    -static-libstdc++
    -fstack-protector-strong
    -fstrict-return
    -shared
    LINKER:-z,now
    LINKER:-z,relro
    LINKER:-z,noexecstack
    LINKER:--no-undefined
    )
endif()

if(APPLE AND BUILD_LIBZIP)
  set(SDK_SUPPORTS_ARM64 False)
  set(SDK_SUPPORTS_X86_64 False)
  execute_process(
    COMMAND xcode-select -p
    RESULT_VARIABLE XCODE_SELECT_RESULT
    OUTPUT_VARIABLE XCODE_DEVELOPER_PATH
    )
  if(NOT ${XCODE_SELECT_RESULT} EQUAL "0")
    message(WARNING "xcode-select failed with result ${XCODE_SELECT_RESULT}")
  else()
    string(STRIP "${XCODE_DEVELOPER_PATH}" XCODE_DEVELOPER_PATH)
    set(SDKSETTINGS_PATH "${XCODE_DEVELOPER_PATH}/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/SDKSettings.plist")

    # CAUTION: do NOT ever remove the '-o -' parameter, without '-o' plutil will overwrite the .plist file
    execute_process(
      COMMAND plutil -extract SupportedTargets.macosx.Archs json -o - "${SDKSETTINGS_PATH}"
      RESULT_VARIABLE PLUTIL_RESULT
      OUTPUT_VARIABLE SDK_ARCHITECTURES
      )
    if(NOT ${PLUTIL_RESULT} EQUAL 0)
      message(WARNING "plutil failed to read ${SDKSETTINGS_PATH}, returned with result ${PLUTIL_RESULT}")
    else()
      string(FIND "${SDK_ARCHITECTURES}" "\"arm64\"" ARCH_POS)
      if(${ARCH_POS} GREATER_EQUAL 0)
        set(SDK_SUPPORTS_ARM64 True)
      endif()

      string(FIND "${SDK_ARCHITECTURES}" "\"x86_64\"" ARCH_POS)
      if(${ARCH_POS} GREATER_EQUAL 0)
        set(SDK_SUPPORTS_X86_64 True)
      endif()
    endif()
  endif()

  unset(MPH_OSX_ARCHITECTURES)
  if(SDK_SUPPORTS_ARM64)
    message(STATUS "SDK at ${XCODE_DEVELOPER_PATH} supports creation of ARM64 binaries")
    list(APPEND LZS_OSX_ARCHITECTURES "arm64")
  endif()

  if(SDK_SUPPORTS_X86_64)
    message(STATUS "SDK at ${XCODE_DEVELOPER_PATH} supports creation of X86_64 binaries")
    list(APPEND LZS_OSX_ARCHITECTURES "x86_64")
  endif()
endif()

#
# Compiler and linker flag checks
#
set(_CHECKED_FLAGS "")
foreach(flag ${COMMON_COMPILE_OPTIONS})
  cxx_compiler_has_flag(${flag})
  if(COMPILER_FLAG_FOUND)
    list(APPEND _CHECKED_FLAGS "${flag}")
  endif()
endforeach()
set(LZS_CXX_FLAGS "${_CHECKED_FLAGS}")

set(_CHECKED_FLAGS "")
foreach(flag ${COMMON_COMPILE_OPTIONS})
  c_compiler_has_flag(${flag})
  if(COMPILER_FLAG_FOUND)
    list(APPEND _CHECKED_FLAGS "${flag}")
  endif()
endforeach()
set(LZS_C_FLAGS "${_CHECKED_FLAGS}")

set(_CHECKED_FLAGS "")
foreach(flag ${LINKER_OPTIONS})
  cxx_linker_has_flag(${flag})
  if(LINKER_FLAG_FOUND)
    list(APPEND _CHECKED_FLAGS "${flag}")
  endif()
endforeach()
set(LZS_CXX_LINKER_FLAGS "${_CHECKED_FLAGS}")

if(BUILD_DEPENDENCIES)
  add_subdirectory(external/zlib-ng)

  target_compile_options(
    zlib
    PRIVATE
    ${LZS_C_FLAGS}
    -fvisibility=hidden
    )

if(NOT WIN32)
  add_subdirectory(external/xz)

  target_compile_options(
    liblzma
    PRIVATE
    ${LZS_C_FLAGS}
    -fvisibility=hidden
    )
endif()

    set(BZIP2_SOURCES external/bzip2)
    add_library(
      bz2
      STATIC
      ${BZIP2_SOURCES}/blocksort.c
      ${BZIP2_SOURCES}/huffman.c
      ${BZIP2_SOURCES}/crctable.c
      ${BZIP2_SOURCES}/randtable.c
      ${BZIP2_SOURCES}/compress.c
      ${BZIP2_SOURCES}/decompress.c
      ${BZIP2_SOURCES}/bzlib.c
      )

    target_compile_options(
      bz2
      PRIVATE
      ${LZS_C_FLAGS}
      -fvisibility=hidden
      )

    configure_file("${CMAKE_SOURCE_DIR}/data/bzip2.pc.in" "${CMAKE_BINARY_DIR}/bzip2.pc")

    install(TARGETS bz2
      RUNTIME DESTINATION bin
      ARCHIVE DESTINATION lib
      LIBRARY DESTINATION lib
      )

    install(FILES
      ${BZIP2_SOURCES}/bzlib.h
      DESTINATION include
      )

    install(FILES
      "${CMAKE_BINARY_DIR}/bzip2.pc"
      DESTINATION lib/pkgconfig
      )
else()
  if(NOT ARTIFACTS_ROOT_DIR)
    message(FATAL_ERROR "ARTIFACTS_ROOT_DIR must be defined on command line")
  endif()

  set(ZLIB_ROOT "${ARTIFACTS_ROOT_DIR}" CACHE STRING "" FORCE)
  set(BZip2_ROOT "${ARTIFACTS_ROOT_DIR}" CACHE STRING "" FORCE)

  if(WIN32)
    find_package(LibLZMA CONFIG REQUIRED)
  else()
    set(LibLZMA_ROOT "${ARTIFACTS_ROOT_DIR}" CACHE STRING "" FORCE)
  endif()

  list(PREPEND CMAKE_PREFIX_PATH "${ARTIFACTS_ROOT_DIR}")

  add_subdirectory(external/libzip)

  target_compile_options(
    zip
    PRIVATE
    ${LZS_C_FLAGS}
    )

  if(UNIX)
    set(ZIP_EXTERN "-DZIP_EXTERN=__attribute__((visibility(\"default\")))")
    target_compile_options(
      zip
      PRIVATE
      -fvisibility=hidden
      )
  else()
    set(ZIP_EXTERN "-DZIP_EXTERN=__declspec(dllexport)")
  endif()
  target_compile_options(
    zip
    PRIVATE
    "${ZIP_EXTERN}"
    )

  if(APPLE)
    set_target_properties(
      zip
      PROPERTIES
      OSX_ARCHITECTURES "${LZS_OSX_ARCHITECTURES}"
      )
  endif()

  add_library(
    ${PROJECT_NAME}
    SHARED
    native/version.cc
    )

  add_dependencies(${PROJECT_NAME} zip)

  target_compile_definitions(
    ${PROJECT_NAME}
    PRIVATE
    LIBZIPSHARP_VERSION="${LZS_VERSION}"
    )

  message(STATUS "LZMA: ${LIBLZMA_INCLUDE_DIR}")
  message(STATUS "ZLIB: ${ZLIB_INCLUDE_DIR}")
  message(STATUS "BZ2: ${BZIP2_INCLUDE_DIR}")

  target_include_directories(
    ${PROJECT_NAME}
    PRIVATE
    ${ZLIB_INCLUDE_DIR}
    ${LIBLZMA_INCLUDE_DIR}
    ${BZIP2_INCLUDE_DIR}
    ${CMAKE_BINARY_DIR}/external/libzip
    )
  target_compile_options(
    ${PROJECT_NAME}
    PRIVATE
    ${LZS_CXX_FLAGS}
    )

  if(APPLE)
    set_target_properties(
      ${PROJECT_NAME}
      PROPERTIES
      OSX_ARCHITECTURES "${LZS_OSX_ARCHITECTURES}"
      )
  elseif(WIN32)
    set_target_properties(
      ${PROJECT_NAME}
      PROPERTIES
      OUTPUT_NAME "lib${PROJECT_NAME}"
      )
  endif()

  target_link_options(
    ${PROJECT_NAME}
    PRIVATE
    ${LZS_CXX_LINKER_FLAGS}
    )

  if(APPLE)
    target_link_libraries(
      ${PROJECT_NAME}
      PRIVATE
      -Wl,-force_load,$<TARGET_FILE:zip>
      )
  elseif(UNIX)
    # This works with GNU ld, should clang's lld be used we need to switch to the same code as APPLE above
    target_link_libraries(
      ${PROJECT_NAME}
      PRIVATE
      -Wl,--whole-archive
      $<TARGET_FILE:zip>
      -Wl,-no-whole-archive
      )
  elseif(MSVC)
    target_link_options(
      ${PROJECT_NAME}
      PRIVATE
      /wholearchive:$<TARGET_FILE:zip>
      )
  endif()

  if(APPLE AND ARTIFACTS_OTHER_ROOT_DIR)
    macro(make_fat_archive INPUT1 INPUT2 OUTPUT)
      execute_process(
        COMMAND lipo -create "${INPUT1}" "${INPUT2}" -output "${OUTPUT}"
        WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
        )
    endmacro()

    set(ZLIB_PATH "${CMAKE_BINARY_DIR}/libz-fat.a")
    set(BZ2_PATH "${CMAKE_BINARY_DIR}/libbz2-fat.a")
    set(LZMA_PATH "${CMAKE_BINARY_DIR}/liblzma-fat.a")

    make_fat_archive("${ARTIFACTS_ROOT_DIR}/lib/libz.a" "${ARTIFACTS_OTHER_ROOT_DIR}/lib/libz.a" "${ZLIB_PATH}")
    make_fat_archive("${ARTIFACTS_ROOT_DIR}/lib/libbz2.a" "${ARTIFACTS_OTHER_ROOT_DIR}/lib/libbz2.a" "${BZ2_PATH}")
    make_fat_archive("${ARTIFACTS_ROOT_DIR}/lib/liblzma.a" "${ARTIFACTS_OTHER_ROOT_DIR}/lib/liblzma.a" "${LZMA_PATH}")

    set(LIBS
      ${ZLIB_PATH}
      ${BZ2_PATH}
      ${LZMA_PATH}
      )
  else()
    if(WIN32)
      set(LIBS
        ${ARTIFACTS_ROOT_DIR}/lib/zlib.lib
        ${ARTIFACTS_ROOT_DIR}/lib/bz2.lib
        LibLZMA::LibLZMA
        )
    else()
      set(LIBS
        ${ARTIFACTS_ROOT_DIR}/lib/libz.a
        ${ARTIFACTS_ROOT_DIR}/lib/libbz2.a
        ${ARTIFACTS_ROOT_DIR}/lib/liblzma.a
        )
    endif()
  endif()

  target_link_libraries(
    ${PROJECT_NAME}
    PRIVATE
    ${LIBS}
    )

  #
  # Place debug info in separate files, if possible
  #
  if(IS_LINUX OR IS_ANDROID)
    if(IS_LINUX)
      set(OBJCOPY "objcopy")
      set(STRIP "strip")
    else()
      set(OBJCOPY "${ANDROID_TOOLCHAIN_ROOT}/bin/llvm-objcopy${ANDROID_TOOLCHAIN_SUFFIX}")
      set(STRIP "${ANDROID_STRIP}")
    endif()

    add_custom_command(
      TARGET ${PROJECT_NAME}
      POST_BUILD
      WORKING_DIRECTORY "$<TARGET_FILE_DIR:${PROJECT_NAME}>"
      COMMAND ${OBJCOPY} --only-keep-debug "$<TARGET_FILE_NAME:${PROJECT_NAME}>" "$<TARGET_FILE_NAME:${PROJECT_NAME}>.debug"
      COMMAND ${STRIP} --strip-debug --strip-unneeded "$<TARGET_FILE_NAME:${PROJECT_NAME}>"
      COMMAND ${OBJCOPY} --add-gnu-debuglink="$<TARGET_FILE_NAME:${PROJECT_NAME}>.debug" "$<TARGET_FILE_NAME:${PROJECT_NAME}>"
      )
  endif()
endif()
