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

static BYTE moddedBytes[3] = { 0xB0, 0x00, 0xC3 };
static BYTE originalBytes[sizeof(moddedBytes)];
static BYTE deg2RadBytes[9];
static int patchOffset = 0;
static int deg2RadOffset = 0;
static int raceTimeOffset = 0;

//This method is called when mod checkbox is ticked
extern "C" __declspec(dllexport) bool applyMod() {
	string path;
	stringstream pathStream;
	pathStream << getenv("APPDATA") << "\\BloonsModLauncher\\offsets\\Unlimited5TierMod.txt";
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
		patchOffset = grabOffsetFromCache(path, "Unlimited5TierMod");
		deg2RadOffset = grabOffsetFromCache(path, "Deg2Rad");
		raceTimeOffset = grabOffsetFromCache(path, "GetRaceTime");
	}
	else
	{
		// since there is no cache file, grab the offset from dump.cs
		patchOffset = grabOffsetFromDump("public bool IsLocked();", "UpgradeButton : MonoBehaviour");
		deg2RadOffset = grabOffsetFromDump("public static float get_Deg2Rad();", "Math");
		raceTimeOffset = grabOffsetFromDump("public float GetRaceTime();", "UnityToSimulation");
		saveOffsetToCache(path, patchOffset, "Unlimited5TierMod");
		saveOffsetToCache(path, deg2RadOffset, "Deg2Rad");
		saveOffsetToCache(path, raceTimeOffset, "GetRaceTime");
	}
	BOOL read = readFromMemory(originalBytes, patchOffset, sizeof(moddedBytes));
	if (!(read > 0)) {
		return false;
	}
	BOOL readDos = readFromMemory(deg2RadBytes, deg2RadOffset, sizeof(deg2RadBytes));
	if (!(readDos > 0)) {
		return false;
	}
	patchToMemory(deg2RadBytes, raceTimeOffset, sizeof(deg2RadBytes));
	return patchToMemory(moddedBytes, patchOffset, sizeof(moddedBytes));
}

//This method is called when mod checkbox is unticked
extern "C" __declspec(dllexport) bool removeMod() {
	return patchToMemory(originalBytes, patchOffset, sizeof(originalBytes));
}

