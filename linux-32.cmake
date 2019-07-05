# the name of the target operating system
SET(CMAKE_SYSTEM_NAME Linux)

set(X86_COMPILER_FLAGS "-m32")
# Set or retrieve the cached flags.
# This is necessary in case the user sets/changes flags in subsequent
# configures. If we included our flags in here, they would get
# overwritten.
set(CMAKE_C_FLAGS ""
        CACHE STRING "Flags used by the compiler during all build types.")
set(CMAKE_CXX_FLAGS ""
        CACHE STRING "Flags used by the compiler during all build types.")

set(CMAKE_C_FLAGS             "${X86_COMPILER_FLAGS} ${CMAKE_C_FLAGS}")
set(CMAKE_CXX_FLAGS           "${X86_COMPILER_FLAGS} ${CMAKE_CXX_FLAGS}")