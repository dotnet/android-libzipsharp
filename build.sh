#!/bin/bash -e
MY_NAME=$(basename "$0")
TRUE_PATH=$(readlink "$0" || echo "$0")
MY_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
BUILD_DIR_ROOT="${MY_DIR}/lzsbuild"
OS=$(uname -s)
LZS_BUILD_DIR="${BUILD_DIR_ROOT}/lib/${OS}"
DEPS_BASE_BUILD_DIR="${BUILD_DIR_ROOT}/deps/${OS}"
ZLIB_DIR="external/zlib"

if [ "${OS}" == "Darwin" ]; then
    DEPS_BUILD_DIR="${DEPS_BASE_BUILD_DIR}/native"
    DEPS_OTHER_BUILD_DIR="${DEPS_BASE_BUILD_DIR}/other"
    ARTIFACTS_DIR_ROOT="${MY_DIR}/artifacts/native"
    ARTIFACTS_OTHER_DIR_ROOT="${MY_DIR}/artifacts/other"
else
    DEPS_BUILD_DIR="${DEPS_BASE_BUILD_DIR}"
    DEPS_OTHER_BUILD_DIR="${DEPS_BUILD_DIR}"
    ARTIFACTS_DIR_ROOT="${MY_DIR}/artifacts"
    ARTIFACTS_OTHER_DIR_ROOT="${ARTIFACTS_DIR_ROOT}"
fi

#
# Defaults
#
NINJA="ninja"
CMAKE="cmake"
GENERATOR="Ninja"
JOBS=""
CONFIGURATION="RelWithDebInfo"
REBUILD="no"
VERBOSE="no"
USE_ZLIBNG="yes"

# The color block is pilfered from the dotnet installer script
#
# Setup some colors to use. These need to work in fairly limited shells, like the Ubuntu Docker container where there are only 8 colors.
# See if stdout is a terminal
#
BRIGHT_YELLOW=""
BRIGHT_GREEN=""
BRIGHT_BLUE=""
BRIGHT_CYAN=""
BRIGHT_WHITE=""
NORMAL=""

if [ -t 1 ] && command -v tput > /dev/null; then
    # see if it supports colors
    ncolors=$(tput colors)
    if [ -n "$ncolors" ] && [ $ncolors -ge 8 ]; then
        BRIGHT_YELLOW="$(tput bold || echo)$(tput setaf 3 || echo)"
        BRIGHT_GREEN="$(tput bold || echo)$(tput setaf 2 || echo)"
        BRIGHT_BLUE="$(tput bold || echo)$(tput setaf 4 || echo)"
        BRIGHT_CYAN="$(tput bold || echo)$(tput setaf 6 || echo)"
        BRIGHT_WHITE="$(tput bold || echo)$(tput setaf 7 || echo)"
        NORMAL="$(tput sgr0 || echo)"
    fi
fi

function die()
{
    echo -e "$@" >&2
    exit 1
}

function missing_argument()
{
    die "Option '$1' requires an argument"
}

function usage()
{
    cat <<EOF
Usage: ${BRIGHT_WHITE}${MY_NAME}${NORMAL} ${BRIGHT_CYAN}[OPTIONS]${NORMAL}

where OPTIONS are one or more of:

  -n|--ninja PATH            use ninja at PATH instead of the default of '${NINJA}'
  -m|--cmake PATH            use cmake at PATH instead of the default of '${CMAKE}'

  -g|--zlib-ng               use the zlib-ng library instead of zlib (default: ${USE_ZLIBNG}
  -c|--configuration NAME    build using configuration NAME instead of the default of '${CONFIGURATION}'
  -j|--jobs NUM              run at most this many build jobs in parallel
  -v|--verbose               make cmake and ninja verbose
  -r|--rebuild               rebuild from scratch. The artifacts and build directories will be
                             recursively removed, if they exist.

  -h|--help                  show this message
EOF
  exit 0
}

function print_banner()
{
    echo ${NORMAL}
    echo ${BRIGHT_GREEN}"*************************************************************"
    echo ${BRIGHT_YELLOW}"$@"
    echo ${BRIGHT_GREEN}"*************************************************************"
    echo ${NORMAL}
}

function run_cmake_common()
{
    "${CMAKE}" "$@"
}

function get_jobs()
{
    local jobs=""

    if [ -n "${JOBS}" ]; then
        echo "-j ${JOBS}"
    fi
}

function cmake_configure()
{
    local build_dir="${1}"

    if [ -z "${build_dir}" ]; then
        die Build directory is required by cmake_configure
    fi
    shift

    run_cmake_common \
        -B "${build_dir}" \
        -S "${MY_DIR}" \
        -G "${GENERATOR}" \
        -DCMAKE_BUILD_TYPE="${CONFIGURATION}" \
        "$@"
}

function cmake_build()
{
    local build_dir="${1}"
    local verbose=""

    if [ "${VERBOSE}" == "yes" ]; then
        verbose="-v"
    fi

    if [ -z "${build_dir}" ]; then
        die Build directory is required by cmake_build
    fi
    shift

    run_cmake_common --build "${build_dir}" $(get_jobs) ${verbose} "$@"
}

function cmake_install()
{
    local build_dir="${1}"

    if [ -z "${build_dir}" ]; then
        die Build directory is required by cmake_install
    fi
    shift

    #
    # We don't need to pass --prefix, it's set in CMakeLists.txt and there's no
    # need to override it here
    #
    run_cmake_common --install "${build_dir}"
}

function get_osx_supported_architectures()
{
    OSX_SUPPORTS_ARM64=""
    OSX_SUPPORTS_X86_64=""

    local xcode_path="$(xcode-select -p)"
    local sdksettings_path="${xcode_path}/Platforms/MacOSX.platform/Developer/SDKs/MacOSX.sdk/SDKSettings.plist"

    if [ ! -f "${sdksettings_path}" ]; then
        die Unable to determine supported macOS build architectures
    fi

    local architectures="$(plutil -extract SupportedTargets.macosx.Archs json -o - "${sdksettings_path}")"
    if [ -z "${architectures}" ]; then
        die SDK settings file specifies no supported architectures
    fi

    if $(echo "${architectures}" | grep '"x86_64"' > /dev/null 2>&1); then
        OSX_SUPPORTS_X86_64="x86_64"
    fi

    if $(echo "${architectures}" | grep '"arm64"' > /dev/null 2>&1); then
        OSX_SUPPORTS_ARM64="arm64"
    fi
}

POSITIONAL_ARGS=""
while (( "$#" )); do
    case "$1" in
        -n|--ninja)
            if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
                NINJA="$2";
                shift 2
            else
                missing_argument "$1"
            fi
            ;;

        -m|--cmake)
            if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
                CMAKE="$2";
                shift 2
            else
                missing_argument "$1"
            fi
            ;;

        -j|--jobs)
            if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
                JOBS="$2";
                shift 2
            else
                missing_argument "$1"
            fi
            ;;

        -c|--configuration)
            if [ -n "$2" ] && [ ${2:0:1} != "-" ]; then
                CONFIGURATION="$2";
                shift 2
            else
                missing_argument "$1"
            fi
            ;;

		-g|--zlib-ng) USE_ZLIBNG="yes"; shift ;;

        -v|--verbose) VERBOSE="yes"; shift ;;

        -r|--rebuild) REBUILD="yes"; shift ;;

        -h|--help) usage; shift ;;

        -*|--*=) die "Error: Unsupported flag $1";;

        *) POSITIONAL_ARGS="${POSITIONAL_ARGS} $1"; shift;;
    esac
done
eval set -- "${POSITIONAL_ARGS}"

if [ -z "${CMAKE}" ]; then
    die cmake binary must be specified
fi

if [ -z "${NINJA}" ]; then
    die ninja binary must be specified
fi

if [ "${REBUILD}" == "yes" ]; then
    rm -rf "${BUILD_DIR_ROOT}"
    rm -rf "${ARTIFACTS_DIR_ROOT}"
fi

if [ "${OS}" == "Darwin" ]; then
    get_osx_supported_architectures
fi

if [ "${USE_ZLIBNG}" != "no" ]; then
	ENABLE_ZLIBNG="ON"
else
    ENABLE_ZLIBNG="OFF"
fi

if [ "${OS}" == "Darwin" ]; then
    MAKE="gmake"
    X86_NATIVE="no"
    X86_BUILD_DIR=""
    X86_ARTIFACTS_DIR=""
    ARM64_NATIVE="no"
    ARM64_BUILD_DIR=""
    ARM64_ARTIFACTS_DIR=""

    case $(arch) in
        x86_64|x86_64h|i386)
            X86_NATIVE="yes"
            X86_BUILD_DIR="${DEPS_BUILD_DIR}"
            X86_ARTIFACTS_DIR="${ARTIFACTS_DIR_ROOT}"
            ARM64_BUILD_DIR="${DEPS_OTHER_BUILD_DIR}"
            ARM64_ARTIFACTS_DIR="${ARTIFACTS_OTHER_DIR_ROOT}"
            ;;

        arm64|arm64e)
            ARM64_NATIVE="yes"
            ARM64_BUILD_DIR="${DEPS_BUILD_DIR}"
            ARM64_ARTIFACTS_DIR="${ARTIFACTS_DIR_ROOT}"
            X86_BUILD_DIR="${DEPS_OTHER_BUILD_DIR}"
            X86_ARTIFACTS_DIR="${ARTIFACTS_OTHER_DIR_ROOT}"
            ;;
        *) die Usupported host architecture $(arch)
    esac

    if [ -n "${OSX_SUPPORTS_ARM64}" ]; then
        print_banner "Configuring dependency libraries for ${OSX_SUPPORTS_ARM64}"
        cmake_configure "${ARM64_BUILD_DIR}" \
                        -DBUILD_DEPENDENCIES=ON \
                        -DCMAKE_OSX_ARCHITECTURES=${OSX_SUPPORTS_ARM64} \
                        "-DARTIFACTS_ROOT_DIR=${ARM64_ARTIFACTS_DIR}" \
                        "-DCMAKE_INSTALL_PREFIX=${ARM64_ARTIFACTS_DIR}" \
                        -DENABLE_ZLIBNG=${ENABLE_ZLIBNG}

        print_banner "Configuring dependency libraries for ${OSX_SUPPORTS_ARM64}"
        cmake_build "${ARM64_BUILD_DIR}"

        print_banner "Installing dependency libraries locally for ${OSX_SUPPORTS_ARM64}"
        cmake_install "${ARM64_BUILD_DIR}"
    fi

    if [ -n "${OSX_SUPPORTS_X86_64}" ]; then
        print_banner "Configuring dependency libraries for ${OSX_SUPPORTS_X86_64}"
        cmake_configure "${X86_BUILD_DIR}" \
                        -DBUILD_DEPENDENCIES=ON \
                        -DCMAKE_OSX_ARCHITECTURES=${OSX_SUPPORTS_X86_64} \
                        "-DARTIFACTS_ROOT_DIR=${X86_ARTIFACTS_DIR}" \
                        "-DCMAKE_INSTALL_PREFIX=${X86_ARTIFACTS_DIR}" \
                        -DENABLE_ZLIBNG=${ENABLE_ZLIBNG}

        print_banner "Configuring dependency libraries for ${OSX_SUPPORTS_X86_64}"
        cmake_build "${X86_BUILD_DIR}"

        print_banner "Installing dependency libraries locally for ${OSX_SUPPORTS_X86_64}"
        cmake_install "${X86_BUILD_DIR}"
    fi
else
    MAKE="make"

    print_banner "Configuring dependency libraries"
    cmake_configure "${DEPS_BUILD_DIR}" \
                    -DBUILD_DEPENDENCIES=ON \
                    "-DARTIFACTS_ROOT_DIR=${ARTIFACTS_DIR_ROOT}" \
                    "-DCMAKE_INSTALL_PREFIX=${ARTIFACTS_DIR_ROOT}" \
                    -DENABLE_ZLIBNG=${ENABLE_ZLIBNG}

    print_banner "Building dependency libraries"
    cmake_build "${DEPS_BUILD_DIR}"

    print_banner "Installing dependency libraries locally"
    cmake_install "${DEPS_BUILD_DIR}"
fi

print_banner "Configuring libZipSharpNative"
cmake_configure "${LZS_BUILD_DIR}" \
                -DBUILD_LIBZIP=ON \
                "-DARTIFACTS_ROOT_DIR=${ARTIFACTS_DIR_ROOT}" \
                "-DARTIFACTS_OTHER_ROOT_DIR=${ARTIFACTS_OTHER_DIR_ROOT}" \
                -DENABLE_ZLIBNG=${ENABLE_ZLIBNG}

print_banner "Building libZipSharpNative"
cmake_build "${LZS_BUILD_DIR}"
