using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Unity;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using NKHook6;
using System;
using System.Linq;

namespace CaveMonkey
{
    public class Utils
    {
        public static void registerTowerInInventory(ShopTowerDetailsModel details, string insertBefore, List<TowerDetailsModel> allTowersInTheGame)
        {
            // get the tower details for the tower insertBefore and insert the new tower into the index towerBefore is at, shifting everything after it by 1
            TowerDetailsModel towerAfter = allTowersInTheGame.ToArray().FirstOrDefault(tower => tower.towerId == insertBefore);
            allTowersInTheGame.Insert(allTowersInTheGame.IndexOf(towerAfter), details);
        }
    }

    public class Mod : MelonMod
    {
        public override void OnApplicationStart()
        {
            HarmonyInstance.Create("BowDown097.Cave Monkey").PatchAll();
        }
    }

    [HarmonyPatch(typeof(ProfileModel), "Validate")] // this method is called after the profile data is parsed, hence why it's used to modify said profile data
    public class ProfileModel_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ProfileModel __instance)
        {
            List<string> unlockedTowers = __instance.unlockedTowers;

            if (unlockedTowers.Contains("CaveMonkey"))
            {
                return;
            }

            unlockedTowers.Add("CaveMonkey");

            Logger.Log("Added Cave Monkey to the list of unlocked towers", (int)ConsoleColor.Cyan, Logger.Level.Normal, "CaveMonkeyMod");
        }
    }

    [HarmonyPatch(typeof(Game), "GetVersionString")] // this method is called soon after the game is done initializing the models, hence why it's used to modify said models
    public class GameModel_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Game __instance)
        {
            PowerModel caveModel = __instance.model.powers.FirstOrDefault(power => power.name == "CaveMonkey");
            if (caveModel.tower.cost == 170)
            {
                return;
            }

            caveModel.tower.cost = 170;
            caveModel.tower.towerSet = "Primary";

            Logger.Log("Configured Cave Monkey tower model", (int)ConsoleColor.Cyan, Logger.Level.Normal, "CaveMonkeyMod");
        }
    }

    [HarmonyPatch(typeof(TowerInventory), "Init")] // this method tells the game to create buttons for a given list of towers, allTowersInTheGame, which we modify here
    public class TowerInit_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(TowerInventory __instance, ref List<TowerDetailsModel> allTowersInTheGame)
        {
            if (allTowersInTheGame.ToArray().Any(tower => tower.name.Contains("CaveMonkey")))
            {
                return true;
            }

            ShopTowerDetailsModel caveDetails = new ShopTowerDetailsModel("CaveMonkey", 1, 0, 0, 0, -1, null);
            Utils.registerTowerInInventory(caveDetails, "BoomerangMonkey", allTowersInTheGame);

            Logger.Log("Registered Cave Monkey in tower inventory", (int)ConsoleColor.Cyan, Logger.Level.Normal, "CaveMonkeyMod");
            return true;
        }
    }
}
