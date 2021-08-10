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
#include <commdlg.h>
#include "../BTD6MAPIBridge/src/il2cpp/api.h"
#include "Windows/io.h"
#include <winnt.h>
#pragma warning(disable:4996)

using namespace std;

//
// This examples is the hypersonic mod.
//

//This array is used for storing the original bytes

static BYTE moddedBytes[8];
static BYTE originalBytes[sizeof(moddedBytes)];
static uint64_t GetRateOffset = 0;
static uint64_t Deg2RadOffset = 0;
static uint64_t RaceTimeOffset = 0;

//This method is called when mod checkbox is ticked
extern "C" __declspec(dllexport) bool applyMod() {
	string path;
	stringstream pathStream;
	pathStream << getenv("APPDATA") << "\\BloonsModLauncher\\GameAssembly.txt";
	pathStream >> path;
	string content;
	string assemblyPath;

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
	if (pathEmpty)
	{
		ofstream CacheStreamO;
		assemblyPath = loadFileDialog(L"PE Binary file", L"*.dll;exe");
		CacheStreamO.open(path);
		CacheStreamO << assemblyPath;
	}
	else
	{
		ifstream CacheStreamI(path);
		assemblyPath.assign((std::istreambuf_iterator<char>(CacheStreamI)), std::istreambuf_iterator<char>());
	}
	HMODULE peFile = LoadLibrary(reinterpret_cast<LPCWSTR>(assemblyPath.c_str()));
	il2cpp::initalise(peFile, std::string(assemblyPath) + std::string("\\.."));
	GetRateOffset = il2cpp::GetBTD6Method("Assembly-CSharp", "Assets.Scripts.Models.Towers.Weapons", "WeaponModel", "get_Rate", 0);
	Deg2RadOffset = il2cpp::GetBTD6Method("Assembly-CSharp", "Assets.Scripts.Simulation.SMath", "Math", "get_Deg2Rad", 0);
	RaceTimeOffset = il2cpp::GetBTD6Method("Assembly-CSharp", "Assets.Scripts.Unity.Bridge", "UnityToSimulation", "GetRaceTime", 0);
	BOOL read = readFromMemory(originalBytes, GetRateOffset + 0xC00, sizeof(moddedBytes));
	if (!(read > 0)) {
		return false;
	}
	BOOL readDos = readFromMemory(moddedBytes, Deg2RadOffset + 0xC00, sizeof(moddedBytes));
	if (!(readDos > 0)) {
		return false;
	}
	patchToMemory(moddedBytes, RaceTimeOffset + 0xC00, sizeof(moddedBytes));
	return patchToMemory(moddedBytes, GetRateOffset + 0xC00, sizeof(moddedBytes));
}

//This method is called when mod checkbox is unticked
extern "C" __declspec(dllexport) bool removeMod() {
	return patchToMemory(originalBytes, GetRateOffset + 0xC00, sizeof(originalBytes));
}

