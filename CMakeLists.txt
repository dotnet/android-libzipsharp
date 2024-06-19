cmake_minimum_required(VERSION 3.18)

#
# TODO:
#
#  * Windows: use vcpkg to build bzip2 (vcpkg can be used as a cmake subproject)
#  * Include portions of Mono.Posix native code (if necessary and when the new Mono.Posix is ready)
#  * Add support Android builds
#  * Add support for iOS/tvOS/macCatalyst builds
#  * Decide what to do with crypto support
#

option(BUILD_DEPENDENCIES "Build only libzip dependencies" OFF)
option(BUILD_LIBZIP "Build libzip and libZipSharp" OFF)
option(ENABLE_ZLIBNG "Use zlib-ng instead of zlib" ON)

set(CMAKE_POSITION_INDEPENDENT_CODE True CACHE BOOL "Always build position independent code" FORCE)

#
# Options for subprojects
#

#
# libzip, zlib-ng
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
set(ENABLE_ZSTD ON CACHE BOOL "Use zstd in libzip" FORCE)

#
# zstd
#
set(ZSTD_MULTITHREAD_SUPPORT OFF CACHE BOOL "Do not build zstd with multi-thread support" FORCE)
set(ZSTD_BUILD_STATIC ON CACHE BOOL "Build static zstd" FORCE)
set(ZSTD_BUILD_SHARED OFF CACHE BOOL "Do not build shared zstd" FORCE)
set(ZSTD_BUILD_PROGRAMS OFF CACHE BOOL "Do not build zstd programs" FORCE)
set(ZSTD_BUILD_TESTS OFF CACHE BOOL "Do not build zstd tests" FORCE)

#
# zlib-ng
#
if(ENABLE_ZLIBNG)
  set(ZLIB_COMPAT ON CACHE BOOL "Build zlib-ng for compatibility with zlib" FORCE)
  set(ZLIB_ENABLE_TESTS OFF CACHE BOOL "Do not build zlib-ng tests" FORCE)
  set(WITH_NEW_STRATEGIES OFF CACHE BOOL "Disable faster, but with worse compression, deflate strategies" FORCE)
endif()

#
# Read product version
#
file(STRINGS "LibZipSharp.props" LZS_PRODUCT_VERSION_XML_MAJOR REGEX "^[ \t]*<_LibZipSharpAssemblyVersionMajor>(.*)</_LibZipSharpAssemblyVersionMajor>")
file(STRINGS "LibZipSharp.props" LZS_PRODUCT_VERSION_XML_MINOR REGEX "^[ \t]*<_LibZipSharpAssemblyVersionMinor>(.*)</_LibZipSharpAssemblyVersionMinor>")
file(STRINGS "LibZipSharp.props" LZS_PRODUCT_VERSION_XML_PATCH REGEX "^[ \t]*<_LibZipSharpAssemblyVersionPatch>(.*)</_LibZipSharpAssemblyVersionPatch>")

string(REGEX REPLACE "^[ \t]*<_LibZipSharpAssemblyVersionMajor>([0-9.]+).*</_LibZipSharpAssemblyVersionMajor>" "\\1" LZS_VERSION_MAJOR "${LZS_PRODUCT_VERSION_XML_MAJOR}")
string(REGEX REPLACE "^[ \t]*<_LibZipSharpAssemblyVersionMinor>([0-9.]+).*</_LibZipSharpAssemblyVersionMinor>" "\\1" LZS_VERSION_MINOR "${LZS_PRODUCT_VERSION_XML_MINOR}")
string(REGEX REPLACE "^[ \t]*<_LibZipSharpAssemblyVersionPatch>([0-9.]+).*</_LibZipSharpAssemblyVersionPatch>" "\\1" LZS_VERSION_PATCH "${LZS_PRODUCT_VERSION_XML_PATCH}")

set(LZS_VERSION "${LZS_VERSION_MAJOR}.${LZS_VERSION_MINOR}.${LZS_VERSION_PATCH}")

if(WIN32)
  set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded" CACHE STRING "Statically link MSVC runtime library")
  set(CMAKE_TOOLCHAIN_FILE ${CMAKE_SOURCE_DIR}/external/vcpkg/scripts/buildsystems/vcpkg.cmake CACHE STRING "Vcpkg toolchain file")
endif()

message(STATUS "Version: ${LZS_VERSION}")
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
else()
  set(COMMON_COMPILE_OPTIONS
    /Qspectre
    /guard:cf
    /sdl
    /wd4996
  )

  set(LINKER_OPTIONS
    LINKER:/PROFILE
    LINKER:/DYNAMICBASE
    LINKER:/CETCOMPAT
    LINKER:/guard:cf
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
  if(ENABLE_ZLIBNG)
    add_subdirectory(external/zlib-ng)
    set(ZLIB_STATIC_TARGET_NAME "zlib")
  else()
    set(ZLIB_BUILD_EXAMPLES False CACHE BOOL "Don't build zlib examples")
    add_subdirectory(external/zlib)
    set(ZLIB_STATIC_TARGET_NAME "zlibstatic")
  endif()

  if(UNIX)
    set(ZLIB_EXTERN "-DZEXTERN=__attribute__((visibility(\"hidden\")))")
    target_compile_options(
      ${ZLIB_STATIC_TARGET_NAME}
      PRIVATE
      -fvisibility=hidden
      )
  endif()

  target_compile_options(
    ${ZLIB_STATIC_TARGET_NAME}
    PRIVATE
    ${LZS_C_FLAGS}
    "${ZLIB_EXTERN}"
    )

  add_subdirectory(external/zstd/build/cmake)
  if(UNIX)
    set(ZSTD_EXTERN "-DZSTDLIB_VISIBILITY=__attribute__((visibility(\"hidden\")))")
    target_compile_options(
      libzstd_static
      PRIVATE
      -fvisibility=hidden
      )
  endif()

  target_compile_options(
    libzstd_static
    PRIVATE
    ${LZS_C_FLAGS}
    "${ZSTD_EXTERN}"
    )

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

    if(UNIX)
      target_compile_options(
        bz2
        PRIVATE
        -fvisibility=hidden
        ${LZS_C_FLAGS}
        )
    endif()

    target_compile_options(
      bz2
      PRIVATE
      ${LZS_C_FLAGS}
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
  set(ENABLE_LZMA False CACHE BOOL "Disable lzma support, even if detected" FORCE)

  list(PREPEND CMAKE_PREFIX_PATH "${ARTIFACTS_ROOT_DIR}")

  if(WIN32)
    # If we don't rename the library, it won't be detected by libzip's cmake
    file(RENAME ${ARTIFACTS_ROOT_DIR}/lib/zstd_static.lib ${ARTIFACTS_ROOT_DIR}/lib/zstd.lib)
  endif()

  add_subdirectory(external/libzip)

  target_compile_options(
    zip
    PRIVATE
    ${LZS_C_FLAGS}
    )

  if(UNIX)
    set(ZIP_EXTERN "-DZIP_EXTERN=__attribute__((visibility(\"default\")))")
    set(ZSTD_EXTERN "-DZSTDLIB_VISIBILITY=__attribute__((visibility(\"hidden\")))")
    set(ZLIB_EXTERN "-DZEXTERN=__attribute__((visibility(\"hidden\")))")
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
    "${ZSTD_EXTERN}"
    "${ZLIB_EXTERN}"
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
    native/sizes.cc
    native/values.cc
    native/version.cc
    )

  add_dependencies(${PROJECT_NAME} zip)

  target_include_directories(
    ${PROJECT_NAME}
    PRIVATE
    "external/libzip/lib"
  )

  target_compile_definitions(
    ${PROJECT_NAME}
    PRIVATE
    LIBZIPSHARP_VERSION="${LZS_VERSION}"
    )

  message(STATUS "ZSTD: ${Zstd_INCLUDE_DIR}")
  message(STATUS "ZLIB: ${ZLIB_INCLUDE_DIR}")
  message(STATUS "BZ2: ${BZIP2_INCLUDE_DIR}")

  target_include_directories(
    ${PROJECT_NAME}
    PRIVATE
    ${ZLIB_INCLUDE_DIR}
    ${BZIP2_INCLUDE_DIR}
    ${Zstd_INCLUDE_DIR}
    ${CMAKE_BINARY_DIR}/external/libzip
    )

  target_compile_options(
    ${PROJECT_NAME}
    PRIVATE
    ${LZS_CXX_FLAGS}
    )

  set(LT_VERSION "${CMAKE_PROJECT_VERSION_MAJOR}.${CMAKE_PROJECT_VERSION_MINOR}.${CMAKE_PROJECT_VERSION_PATCH}")
  set(LT_VERSION_SHORT "${CMAKE_PROJECT_VERSION_MAJOR}.${CMAKE_PROJECT_VERSION_MINOR}")
  set(LT_RELEASE "${CMAKE_PROJECT_VERSION_MAJOR}-${CMAKE_PROJECT_VERSION_MINOR}")
  set(NATIVE_LIB_OUTPUT_NAME "${PROJECT_NAME}-${LT_RELEASE}")

  if(APPLE)
    set_target_properties(
      ${PROJECT_NAME}
      PROPERTIES
      OSX_ARCHITECTURES "${LZS_OSX_ARCHITECTURES}"
      OUTPUT_NAME "${NATIVE_LIB_OUTPUT_NAME}"
    )
  elseif(UNIX)
    set_target_properties(
      ${PROJECT_NAME}
      PROPERTIES
      VERSION ${LT_VERSION}
      SOVERSION ${LT_RELEASE}
      OUTPUT_NAME "${NATIVE_LIB_OUTPUT_NAME}"
    )
  elseif(WIN32)
    set_target_properties(
      ${PROJECT_NAME}
      PROPERTIES
      OUTPUT_NAME "lib${NATIVE_LIB_OUTPUT_NAME}"
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
    set(ZSTD_PATH "${CMAKE_BINARY_DIR}/libzstd-fat.a")

    make_fat_archive("${ARTIFACTS_ROOT_DIR}/lib/libz.a" "${ARTIFACTS_OTHER_ROOT_DIR}/lib/libz.a" "${ZLIB_PATH}")
    make_fat_archive("${ARTIFACTS_ROOT_DIR}/lib/libbz2.a" "${ARTIFACTS_OTHER_ROOT_DIR}/lib/libbz2.a" "${BZ2_PATH}")
    make_fat_archive("${ARTIFACTS_ROOT_DIR}/lib/libzstd.a" "${ARTIFACTS_OTHER_ROOT_DIR}/lib/libzstd.a" "${ZSTD_PATH}")

    set(LIBS
      ${ZLIB_PATH}
      ${BZ2_PATH}
      ${ZSTD_PATH}
      )
  else()
    if(WIN32)
      if(ENABLE_ZLIBNG)
        set(ZLIB_NAME "zlib.lib")
      else()
        set(ZLIB_NAME "zlibstatic.lib")
      endif()

      set(LIBS
        ${ARTIFACTS_ROOT_DIR}/lib/${ZLIB_NAME}
        ${ARTIFACTS_ROOT_DIR}/lib/bz2.lib
        ${ARTIFACTS_ROOT_DIR}/lib/zstd.lib
        )
    else()
      set(LIBS
        ${ARTIFACTS_ROOT_DIR}/lib/libz.a
        ${ARTIFACTS_ROOT_DIR}/lib/libbz2.a
        ${ARTIFACTS_ROOT_DIR}/lib/libzstd.a
        )
    endif()
  endif()

  target_link_libraries(
    ${PROJECT_NAME}
    PRIVATE
    ${LIBS}
    )

  #
  # Place debug info in separate files, if possible, and get rid of symlinks
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

    add_custom_command(
      TARGET ${PROJECT_NAME}
      POST_BUILD
      WORKING_DIRECTORY "$<TARGET_FILE_DIR:${PROJECT_NAME}>"
      COMMAND rm lib${PROJECT_NAME}-${LT_RELEASE}.so
      COMMAND rm lib${PROJECT_NAME}-${LT_RELEASE}.so.${LT_RELEASE}
      COMMAND ln -f lib${PROJECT_NAME}-${LT_RELEASE}.so.${LT_VERSION} lib${PROJECT_NAME}-${LT_RELEASE}.so.${LT_VERSION_SHORT}
      COMMAND ln -f lib${PROJECT_NAME}-${LT_RELEASE}.so.${LT_VERSION} lib${PROJECT_NAME}-${LT_RELEASE}.so
    )
  endif()
endif()
