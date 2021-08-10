#include "hypersonic.hpp"
#include "BTDMAPI/errors.hpp"

#define WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <tchar.h>

IMPORT void InitialiseInternalsEarly();
IMPORT uint64_t GetMethodRVA(std::string assembly, std::string namespaze, std::string klass, std::string method, int argc);
IMPORT ERRORCODE patchBytes(uint64_t offset, uint8_t* bytes, uint64_t bc, uint8_t* originalBytes);


DLL_PUBLIC void Initialise() {

	MessageBox(NULL, _TEXT("Hypersonic Mod"), _TEXT("Sppedy Nosies"), MB_OK);

	InitialiseInternalsEarly();

	uint64_t offset = GetMethodRVA(
		"Assembly-CSharp", 
		"Assets.Scripts.Models.Towers.Weapons",
		"WeaponModel", 
		"get_Rate", 
		0);


	uint8_t hypersonicBytes[8] = { 0xF3, 0x0F, 0x10, 0x05, 0x00, 0x1C, 0x03, 0x01 };
	uint8_t originalBytes[8];

	patchBytes(offset, hypersonicBytes, 8, originalBytes);

}
