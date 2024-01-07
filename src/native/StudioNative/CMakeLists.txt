cmake_minimum_required(VERSION 3.26)
project(StudioNative)

set(CMAKE_CXX_STANDARD 17)

add_library(StudioNative SHARED windows/platform_windows.cpp)
