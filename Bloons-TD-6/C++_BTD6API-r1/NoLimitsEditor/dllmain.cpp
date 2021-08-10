// dllmain.cpp : Defines the entry point for the DLL application.

#include "framework.h"

#include <TlHelp32.h>
#include <vector>
#include <stdint.h>
#include "dllmain.h"
#include "OffsetGrabber/OffsetGrabber.h"
#include <sstream>
#include <fstream>
#include <iostream>
#pragma warning(disable:4996)

using namespace std;

//
// This examples is the hypersonic mod.
//

//This array is used for storing the original bytes

static BYTE moddedBytes[1] = { 0xC3 };
static BYTE originalBytes[sizeof(moddedBytes)];
static int patchOffset = 0;

//This method is called when mod checkbox is ticked
extern "C" __declspec(dllexport) bool applyMod() {
	string path;
	stringstream pathStream;
	pathStream << getenv("APPDATA") << "\\BloonsModLauncher\\offsets\\NoLimitsMod.txt";
	pathStream >> path;
	string content;

	patchOffset = 0;
	BOOL pathEmpty = true;
	// read the file at the path of cache file, extract contents
	if ((stat(path.c_str(), new struct stat) == 0))
	{
		ifstream ifs(path);
		content.assign((istreambuf_iterator<char>(ifs)), (istreambuf_iterator<char>()));
		if (content != "")
		{
			pathEmpty = false;
		}
		else
		{
			pathEmpty = true;
		}
	}

	if (!pathEmpty)
	{
		// since the cache file exists and is not empty, grab the offset from cache
		patchOffset = grabOffsetFromCache(path, "NoLimitsMod");
	}
	else
	{
		// since there is no cache file, grab the offset from dump.cs
		patchOffset = grabOffsetFromDump("private void ValidateInputFields();", "ChallengeEditor : MonoBehaviour");
		saveOffsetToCache(path, patchOffset, "NoLimitsMod");
	}
	BOOL read = readFromMemory(originalBytes, patchOffset, sizeof(moddedBytes));
	if (!(read > 0)) {
		return false;
	}
	return patchToMemory(moddedBytes, patchOffset, sizeof(moddedBytes));
}

//This method is called when mod checkbox is unticked
extern "C" __declspec(dllexport) bool removeMod() {
	return patchToMemory(originalBytes, patchOffset, sizeof(originalBytes));
}

