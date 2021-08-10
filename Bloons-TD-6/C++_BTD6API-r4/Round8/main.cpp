// Generated C++ file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
// Custom injected code entry point

#include "../../il2cpp/il2cpp-utils.hpp"
#include "helpers.h"
#include <iostream>

using namespace app;

// Set the name of your log file here
extern const LPCWSTR LOG_FILE = L"il2cpp-log.txt";

// Injected code entry point
void Run()
{
	AllocConsole();
	freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);

	std::cout << "Initializing..." << std::endl;

	size_t size = 0;
	const Il2CppAssembly** assemblies = il2cpp_domain_get_assemblies(nullptr, &size);

	const Il2CppAssembly* assembly = BTD6API::Assembly::init(assemblies, size);

	if (assembly == nullptr)
	{ 
		std::cout << "Error: Assembly-CSharp not found." << std::endl;
		return;
	}

	Il2CppClass* gameClass = il2cpp_class_from_name(assembly->image, "Assets.Scripts.Unity", "Game");
	FieldInfo* instance = il2cpp_class_get_field_from_name(gameClass, "instance");
	Game* gameInstAddr = 0;
	il2cpp_field_static_get_value(instance, &gameInstAddr);

	if (gameInstAddr == NULL)
	{
		std::cout << "Some error occurred when trying to access the game model." << std::endl;
		return;
	}

	Game* gameInstance = (Game*)(gameInstAddr);

	auto roundSetsArr = gameInstance->fields.model->fields.roundSets;
	RoundSetModel** roundSets = roundSetsArr->vector;

	for (int i = 0; i < roundSetsArr->max_length; ++i)
	{
		auto roundModelsArr = roundSets[i]->fields.rounds;
		RoundModel** rounds = roundModelsArr->vector;
		for (int j = 7; j < roundModelsArr->max_length; ++j)
		{
			auto bloonGroupsArr = rounds[j]->fields.groups;
			BloonGroupModel** bloonGroups = bloonGroupsArr->vector;
			bloonGroups[0]->fields.bloon = (String*)il2cpp_string_new("YellowRegrow");
			bloonGroups[0]->fields.end = 6000.0f;
			bloonGroups[0]->fields.count = 888 * (1.5 * (j-7));
			std::cout << "Round " << std::to_string(j-7) << " yellow count: " << std::to_string(888 * (1.5 * (j - 7)));

			for (int k = 1; k < bloonGroupsArr->max_length; ++k)
			{
				bloonGroups[k]->fields.count = 0;
			}
		}
	}
}