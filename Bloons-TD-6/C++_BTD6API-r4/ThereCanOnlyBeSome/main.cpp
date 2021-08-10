// Generated C++ file by Il2CppInspector - http://www.djkaty.com - https://github.com/djkaty
// Custom injected code entry point

#include "pch.hpp"
#include <random>

using namespace app;

// Injected code entry point
void Run()
{
	AllocConsole();
	freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);

	std::cout << "Initializing..." << std::endl;

	size_t size = 0;
	const Il2CppAssembly** assemblies = il2cpp_domain_get_assemblies(nullptr, &size);

	const Il2CppAssembly* assembly = BTD6API::Assembly::get(assemblies, "Assembly-CSharp", size);

	if (assembly == nullptr)
	{
		std::cout << "Error: Could not find Assembly-CSharp." << std::endl;
		return;
	}

	// setup game instance
	Il2CppClass* gameClass = il2cpp_class_from_name(assembly->image, "Assets.Scripts.Unity", "Game");
	FieldInfo* gameInstanceInfo = il2cpp_class_get_field_from_name(gameClass, "instance");
	Game* gameInstAddr = 0;
	il2cpp_field_static_get_value(gameInstanceInfo, &gameInstAddr);

	if (gameInstAddr == NULL)
	{
		std::cout << "Some error occurred when trying to access the game model." << std::endl;
		return;
	}

	Game* gameInstance = (Game*)gameInstAddr;
	GameModel* gameModel = gameInstance->fields.model;

	// hide towers, give 4 banana farms
	TowerDetailsModel__Array* towerSetArr = gameModel->fields.towerSet;
	TowerDetailsModel** towerSet = towerSetArr->vector;
	for (int i = 0; i < towerSetArr->max_length; ++i)
	{
		towerSet[i]->fields.towerCount = (BTD6API::StringUtils::toString(towerSet[i]->fields.towerId) == "BananaFarm") * 4;
	}
	std::cout << "Hid towers, gave 4 banana farms" << std::endl;

	// disable all powers except farmer, pontoon, and pool
	PowerModel__Array* powersArr = gameModel->fields.powers;
	PowerModel** powers = powersArr->vector;
	for (int i = 0; i < powersArr->max_length; ++i)
	{
		std::string name = BTD6API::StringUtils::toString(powers[i]->fields._.name);
		powers[i]->fields.isHidden = !(name == "BananaFarmer" || name == "Pontoon" || name == "PortableLake");
	}
	std::cout << "Hid powers, gave banana farmer, pontoon, and portable lake" << std::endl;

	// make heroes cost a ton cuz i can't think of a better way to do it, heroes are weird
	TowerModel__Array* towersArr = gameModel->fields.towers;
	TowerModel** towers = towersArr->vector;
	for (int i = 0; i < towersArr->max_length; ++i)
	{
		if (BTD6API::StringUtils::toString(towers[i]->fields.towerSet) == "Hero")
		{
			towers[i]->fields.cost = 2000000000.0f;
		}
	}
	std::cout << "Disabled heroes" << std::endl;

	// disable camo and damage type immunities
	BloonModel__Array* bloonsArr = gameModel->fields.bloons;
	BloonModel** bloons = bloonsArr->vector;
	for (int i = 0; i < bloonsArr->max_length; ++i)
	{
		bloons[i]->fields.isCamo = false;

		DamageTypeImunityModel__Array* immunitiesArr = bloons[i]->fields.damageTypeImmunities;
		DamageTypeImunityModel** immunities = immunitiesArr->vector;
		for (int j = 0; j < immunitiesArr->max_length; ++j)
		{
			immunities[j]->fields.destroyProjectile = false;
			immunities[j]->fields.multiplier = 1.0f;
			immunities[j]->fields.addition = 1.0f;
		}
	}
	std::cout << "Removed bloon camo property and immunities" << std::endl;

	// rng (0 - tower set size), make tower available at index of rng
	std::random_device rand;
	std::uniform_int_distribution<int> uniform_dist(0, towerSetArr->max_length-1);
	TowerDetailsModel* randomTower = towerSet[uniform_dist(rand)];
	randomTower->fields.towerCount = 1;
	std::cout << "Enabled " << BTD6API::StringUtils::toString(randomTower->fields._.name) << " for use" << std::endl;
}