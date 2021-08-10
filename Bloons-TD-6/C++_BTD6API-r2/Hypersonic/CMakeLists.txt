cmake_minimum_required(VERSION 3.13)

message(STATUS "Configruing cypersonic mod")

add_library(MOD_HYPERSOINC SHARED "")

target_sources(MOD_HYPERSOINC

	PRIVATE
	src/hypersonic.cpp
	src/hypersonic.hpp

)

target_include_directories(MOD_HYPERSOINC
PRIVATE src
)

target_link_libraries(MOD_HYPERSOINC
SHAREDUTILS
BTD6MAPIAPI
UNITYDUMMYDLL
)
