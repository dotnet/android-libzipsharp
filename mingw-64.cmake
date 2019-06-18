# the name of the target operating system
SET(CMAKE_SYSTEM_NAME Windows)

# which compilers to use for C and C++
SET(CMAKE_C_COMPILER x86_64-w64-mingw32-gcc)
SET(CMAKE_CXX_COMPILER x86_64-w64-mingw32-g++)
SET(CMAKE_RC_COMPILER x86_64-w64-mingw32-windres)

# here is the target environment located
if(CMAKE_HOST_SYSTEM_NAME STREQUAL Linux)
	SET(CMAKE_FIND_ROOT_PATH /usr/x86_64-w64-mingw32)
else()
	SET(CMAKE_FIND_ROOT_PATH /usr/local/opt/mingw-w64/toolchain-x86_64)
endif()

# adjust the default behaviour of the FIND_XXX() commands:
# search headers and libraries in the target environment, search
# programs in the host environment
set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)

set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)
set(BUILD_SHARED_LIBS ON)

set(XA_COMPILER_FLAGS "-static-libgcc")

# Set or retrieve the cached flags.
# This is necessary in case the user sets/changes flags in subsequent
# configures. If we included our flags in here, they would get
# overwritten.
set(CMAKE_C_FLAGS ""
        CACHE STRING "Flags used by the compiler during all build types.")
set(CMAKE_CXX_FLAGS ""
        CACHE STRING "Flags used by the compiler during all build types.")

set(CMAKE_C_FLAGS             "${XA_COMPILER_FLAGS} ${CMAKE_C_FLAGS}")
set(CMAKE_CXX_FLAGS           "${XA_COMPILER_FLAGS} ${CMAKE_CXX_FLAGS}")
