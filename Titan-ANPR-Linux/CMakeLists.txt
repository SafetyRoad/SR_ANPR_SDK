cmake_minimum_required(VERSION 3.20)

project(TitanANPRLinux LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

option(TITAN_BUILD_SHARED "Build shared library" ON)
option(TITAN_BUILD_SAMPLE "Build sample CLI" ON)
set(TITAN_ONNXRUNTIME_ROOT "" CACHE PATH "Path to ONNX Runtime root (contains include/ and lib/)")

find_package(OpenCV REQUIRED)
find_package(OpenSSL REQUIRED)
find_package(Threads REQUIRED)

find_path(TITAN_ORT_INCLUDE_DIR
    NAMES onnxruntime_cxx_api.h
    HINTS
        "${TITAN_ONNXRUNTIME_ROOT}/include"
        "$ENV{ONNXRUNTIME_ROOT}/include")

find_library(TITAN_ORT_LIBRARY
    NAMES onnxruntime
    HINTS
        "${TITAN_ONNXRUNTIME_ROOT}/lib"
        "${TITAN_ONNXRUNTIME_ROOT}/lib64"
        "$ENV{ONNXRUNTIME_ROOT}/lib"
        "$ENV{ONNXRUNTIME_ROOT}/lib64")

if(NOT TITAN_ORT_INCLUDE_DIR OR NOT TITAN_ORT_LIBRARY)
    message(FATAL_ERROR "ONNX Runtime not found. Set TITAN_ONNXRUNTIME_ROOT or ONNXRUNTIME_ROOT.")
endif()

set(TITAN_LIB_TYPE STATIC)
if(TITAN_BUILD_SHARED)
    set(TITAN_LIB_TYPE SHARED)
endif()

add_library(titan_anpr_linux ${TITAN_LIB_TYPE}
    ../Titan-ANPR/titan_anpr.cpp
    ../Titan-ANPR/titan_anpr_pipeline.cpp
    ../Titan-ANPR/model_crypto.cpp
    src/titan_license_stub.cpp)

target_include_directories(titan_anpr_linux
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
    PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/../Titan-ANPR
        ${TITAN_ORT_INCLUDE_DIR}
        ${OpenCV_INCLUDE_DIRS})

target_link_libraries(titan_anpr_linux
    PRIVATE
        ${TITAN_ORT_LIBRARY}
        ${OpenCV_LIBS}
        OpenSSL::Crypto
        Threads::Threads
        dl)

target_compile_definitions(titan_anpr_linux PRIVATE TITAN_ANPR_LINUX_BUILD=1)

set_target_properties(titan_anpr_linux PROPERTIES
    OUTPUT_NAME "Titan-ANPR"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib")

if(TITAN_BUILD_SAMPLE)
    add_executable(sample_cli src/sample_cli.cpp)
    target_include_directories(sample_cli PRIVATE
        ${CMAKE_CURRENT_SOURCE_DIR}/include
        ${OpenCV_INCLUDE_DIRS})
    target_link_libraries(sample_cli PRIVATE
        titan_anpr_linux
        ${OpenCV_LIBS})
    set_target_properties(sample_cli PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin")
endif()
