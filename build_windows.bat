@echo off
set BUILD_DIR_ROOT=%CD%\lzsbuild

set DEPS_BUILD_DIR_ROOT=%BUILD_DIR_ROOT%\deps
set DEPS_BUILD_DIR_ROOT_32=%DEPS_BUILD_DIR_ROOT%\win32
set DEPS_BUILD_DIR_ROOT_64=%DEPS_BUILD_DIR_ROOT%\win64

set LIB_BUILD_DIR_ROOT=%BUILD_DIR_ROOT%\lib
set LIB_BUILD_DIR_ROOT_32=%LIB_BUILD_DIR_ROOT%\win32
set LIB_BUILD_DIR_ROOT_64=%LIB_BUILD_DIR_ROOT%\win64

set ARTIFACTS_DIR_ROOT=%CD%\artifacts
set ARTIFACTS_DIR_ROOT_64=%ARTIFACTS_DIR_ROOT%\win64
set ARTIFACTS_DIR_ROOT_32=%ARTIFACTS_DIR_ROOT%\win32

set CONFIG=RelWithDebInfo
set COMMON_CMAKE_PARAMS=-DCMAKE_BUILD_TYPE=%CONFIG% -G "Visual Studio 16 2019"

echo Common cmake params: %COMMON_CMAKE_PARAMS%
echo 32-bit dependencies artifacts dir: %ARTIFACTS_DIR_ROOT_32%
echo 64-bit dependencies artifacts dir: %ARTIFACTS_DIR_ROOT_64%
echo 32-bit dependencies build root: %DEPS_BUILD_DIR_ROOT_32%
echo 64-bit dependencies build root: %DEPS_BUILD_DIR_ROOT_64%
echo 32-bit library build root: %LIB_BUILD_DIR_ROOT_32%
echo 64-bit library build root: %LIB_BUILD_DIR_ROOT_64%

pushd .
cd external\vcpkg
call bootstrap-vcpkg.bat
popd
external\vcpkg\vcpkg.exe integrate install
if %errorlevel% neq 0 exit /b %errorlevel%

external\vcpkg\vcpkg.exe install liblzma:x64-windows-static liblzma:x86-windows-static
if %errorlevel% neq 0 exit /b %errorlevel%

REM 64-bit deps
mkdir "%DEPS_BUILD_DIR_ROOT_64%"
cmake %COMMON_CMAKE_PARAMS% -B "%DEPS_BUILD_DIR_ROOT_64%" -DVCPKG_TARGET_TRIPLET=x64-windows-static -DBUILD_DEPENDENCIES=ON "-DARTIFACTS_ROOT_DIR=%ARTIFACTS_DIR_ROOT_64%" "-DCMAKE_INSTALL_PREFIX=%ARTIFACTS_DIR_ROOT_64%" -A x64 .
if %errorlevel% neq 0 exit /b %errorlevel%

cmake --build "%DEPS_BUILD_DIR_ROOT_64%" -v --config %CONFIG%
if %errorlevel% neq 0 exit /b %errorlevel%

cmake --install "%DEPS_BUILD_DIR_ROOT_64%" --config %CONFIG%
if %errorlevel% neq 0 exit /b %errorlevel%

REM 32-bit deps
mkdir "%DEPS_BUILD_DIR_ROOT_32%"
cmake %COMMON_CMAKE_PARAMS% -B "%DEPS_BUILD_DIR_ROOT_32%" -DVCPKG_TARGET_TRIPLET=x86-windows-static -DBUILD_DEPENDENCIES=ON "-DARTIFACTS_ROOT_DIR=%ARTIFACTS_DIR_ROOT_32%" "-DCMAKE_INSTALL_PREFIX=%ARTIFACTS_DIR_ROOT_32%" -A Win32 .
if %errorlevel% neq 0 exit /b %errorlevel%

cmake --build "%DEPS_BUILD_DIR_ROOT_32%" --config %CONFIG% -v
if %errorlevel% neq 0 exit /b %errorlevel%

cmake --install "%DEPS_BUILD_DIR_ROOT_32%" --config %CONFIG%
if %errorlevel% neq 0 exit /b %errorlevel%


REM 64-bit library
mkdir "%LIB_BUILD_DIR_ROOT_64%"
cmake %COMMON_CMAKE_PARAMS% -B "%LIB_BUILD_DIR_ROOT_64%" -DVCPKG_TARGET_TRIPLET=x64-windows-static -DBUILD_LIBZIP=ON "-DARTIFACTS_ROOT_DIR=%ARTIFACTS_DIR_ROOT_64%" "-DCMAKE_INSTALL_PREFIX=%ARTIFACTS_DIR_ROOT_64%" -A x64 .
if %errorlevel% neq 0 exit /b %errorlevel%

cmake --build "%LIB_BUILD_DIR_ROOT_64%" --config %CONFIG% -v
if %errorlevel% neq 0 exit /b %errorlevel%

REM 32-bit library
mkdir "%LIB_BUILD_DIR_ROOT_32%"
cmake %COMMON_CMAKE_PARAMS% -B "%LIB_BUILD_DIR_ROOT_32%" -DVCPKG_TARGET_TRIPLET=x86-windows-static -DBUILD_LIBZIP=ON "-DARTIFACTS_ROOT_DIR=%ARTIFACTS_DIR_ROOT_32%" "-DCMAKE_INSTALL_PREFIX=%ARTIFACTS_DIR_ROOT_32%" -A Win32 .
if %errorlevel% neq 0 exit /b %errorlevel%

cmake --build "%LIB_BUILD_DIR_ROOT_32%" --config %CONFIG% -v
if %errorlevel% neq 0 exit /b %errorlevel%

