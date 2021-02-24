@echo off

set LIBZIP_FEATURES=-DENABLE_COMMONCRYPTO=OFF -DENABLE_GNUTLS=OFF -DENABLE_MBEDTLS=OFF -DENABLE_OPENSSL=OFF -DENABLE_WINDOWS_CRYPTO=OFF -DBUILD_TOOLS=OFF -DBUILD_REGRESS=OFF -DBUILD_EXAMPLES=OFF -DBUILD_DOC=OFF -DENABLE_BZIP2=OFF -DENABLE_LZMA=OFF
set COMMON_CMAKE_PARAMS=-DCMAKE_BUILD_TYPE=Release -G "Visual Studio 16 2019" -DBUILD_SHARED_LIBS=ON -DCMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded -DCMAKE_POLICY_DEFAULT_CMP0074=NEW -DCMAKE_POLICY_DEFAULT_CMP0091=NEW

echo %LIBZIP_FEATURES%
echo %COMMON_CMAKE_PARAMS%

pushd .
cd external\vcpkg
call bootstrap-vcpkg.bat
popd
external\vcpkg\vcpkg.exe integrate install
external\vcpkg\vcpkg.exe install zlib:x64-windows-static zlib:x86-windows-static
pushd .
mkdir .\build\Windows\64
cd .\build\Windows\64
cmake %LIBZIP_FEATURES% %COMMON_CMAKE_PARAMS% -DZLIB_ROOT=..\..\..\external\vcpkg\installed\x64-windows-static -A x64 ..\..\..\external\libzip
cmake --build . --config RelWithDebInfo -v
popd
pushd .

mkdir .\build\Windows\32
cd .\build\Windows\32
cmake %LIBZIP_FEATURES% %COMMON_CMAKE_PARAMS% -DZLIB_ROOT=..\..\..\external\vcpkg\installed\x86-windows-static -A Win32 ..\..\..\external\libzip
cmake --build . --config RelWithDebInfo -v
popd
