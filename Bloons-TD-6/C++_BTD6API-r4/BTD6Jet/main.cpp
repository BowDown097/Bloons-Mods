#include "pch.hpp"
#include "json.hpp" // this is so fucking stupid. i have to include the entirety of the nlohmann json library because the json indenting is broken and this is the only one i know how to use. why?!?
#include "ProgressBar.hpp"
#include <fstream>
#include <filesystem>

using namespace app;
namespace fs = std::filesystem;

template<typename T>
void parseModelArray(T& src, std::string name, JsonSerializerSettings* serializerSettings, const MethodInfo* _serializeObject)
{
	const std::string outDir = "BTD6Jet/Assets/JSON/" + name + "/";
	if (!fs::exists(outDir))
		fs::create_directories(outDir);

	std::cout << "Parsing " << name << ".." << std::endl;
	progresscpp::ProgressBar progress(src->max_length, 70);
	for (int i = 0; i < src->max_length; ++i)
	{
		++progress;
		try
		{
			String* systemSerializedModel = NULL;
			systemSerializedModel = BTD6API::Assembly::callFunction<String*>(_serializeObject, (Object*)src->vector[i], serializerSettings, (MethodInfo*)_serializeObject);
			if (systemSerializedModel != NULL)
			{
				std::ofstream out(outDir + BTD6API::StringUtils::toString(src->vector[i]->fields._.name) + ".json");
				out << nlohmann::json::parse(BTD6API::StringUtils::toString(systemSerializedModel)).dump(4);
				out.close();
			}
		}
		catch (Il2CppExceptionWrapper ile)
		{
			std::wcout << L"Il2CppException occurred while serializing: " << std::wstring(name.begin(), name.end()) << ": " << (wchar_t*)ile.ex->message->chars << std::endl;
		}
		catch (std::exception e)
		{
			std::cout << "Standard exception occurred while serializing: " << name << ": " << e.what() << std::endl;
		}
		progress.display();
	}
	progress.done();
}

// Injected code entry point
void Run()
{
	AllocConsole();
	freopen_s((FILE**)stdout, "CONOUT$", "w", stdout);

	std::cout << "Initializing...\n" << std::endl;

	// get assembly-csharp and newtonsoft assemblies
	size_t size = 0;
	const Il2CppAssembly** assemblies = il2cpp_domain_get_assemblies(nullptr, &size);

	const Il2CppAssembly* assembly = BTD6API::Assembly::get(assemblies, "Assembly-CSharp", size);
	const Il2CppAssembly* newtonsoftAssembly = BTD6API::Assembly::get(assemblies, "Newtonsoft.Json", size);

	if (assembly == nullptr || newtonsoftAssembly == nullptr)
	{
		std::cout << "Error: Could not locate one or more required assemblies." << std::endl;
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

	// setup newtonsoft serializer+settings
	Il2CppClass* jsonConvertClass = il2cpp_class_from_name(newtonsoftAssembly->image, "Newtonsoft.Json", "JsonConvert");
	const MethodInfo* _serializeObject = il2cpp_class_get_method_from_name(jsonConvertClass, "SerializeObject", 2);

	Il2CppClass* jsonSettingsClass = il2cpp_class_from_name(newtonsoftAssembly->image, "Newtonsoft.Json", "JsonSerializerSettings");
	il2cpp_runtime_class_init(jsonSettingsClass); // call .cctor if not called already
	const MethodInfo* _serialSettingsCtor = il2cpp_class_get_method_from_name(jsonSettingsClass, ".ctor", 0);

	JsonSerializerSettings* serialSettings = (JsonSerializerSettings*)il2cpp_object_new(jsonSettingsClass);
	BTD6API::Assembly::callFunction<void*>(_serialSettingsCtor, serialSettings, (MethodInfo*)_serialSettingsCtor);
	serialSettings->fields._referenceLoopHandling.value = ReferenceLoopHandling__Enum_Ignore;
	serialSettings->fields._referenceLoopHandling.has_value = true;
	serialSettings->fields._nullValueHandling.value = NullValueHandling__Enum_Ignore;
	serialSettings->fields._nullValueHandling.has_value = true;

	// make thread (serialize/deserialize methods WILL NOT work without this!)
	Il2CppThread* thread = il2cpp_thread_attach(il2cpp_domain_get());
	// TODO: change this so that the code is less horrendous; have it iterate through the arrays inside of gameModel possibly and then call parseModelArray with them
	parseModelArray(gameModel->fields.towers, "TowerDefinitions", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.upgrades, "UpgradeDefinitions", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.bloons, "BloonDefinitions", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.difficultyModels, "GameModeDefinitions/Difficulty", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.roundSets, "GameModeDefinitions/Default", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.freeplayGroups, "GameModeDefinitions/Freeplay", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.knowledgeSets, "KnowledgeDefinitions", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.rankInfo, "ProfileDefinitions", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.powers, "PowerDefinitions", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.towerSet, "TowerDetailsDefinitions/Towers", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.heroSet, "TowerDetailsDefinitions/Heroes", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.powerSet, "TowerDetailsDefinitions/Powers", serialSettings, _serializeObject);
	parseModelArray(gameModel->fields.skins, "TowerSkinDefinitions", serialSettings, _serializeObject);

	std::cout << "\n\nSaved everything to BTD6Jet folder!" << std::endl;
}