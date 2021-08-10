using Assets.Main.Scenes;
using Assets.Scripts.Models.Powers;
using Assets.Scripts.Models.Profile;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.TowerSets;
using Assets.Scripts.Simulation.Input;
using Assets.Scripts.Unity;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using NKHook6;
using System;
using System.Linq;

namespace AllTheTowers
{
    internal static class Constants
    {
        internal static readonly string[] powers = { "BananaFarmer", "CaveMonkey", "EnergisingTotem", "Pontoon", "PortableLake", "TechBot" };
        internal static readonly int[] powerCosts = { 2500, 215, 1000, 1250, 1250, 2000 };
        internal static readonly string[] otherTowers = { "Marine", "Sentry", "SentryBoom", "SentryCold", "SentryCrushing", "SentryEnergy", "SentryParagon",
        "SunAvatarMini", "TransformedBaseMonkey" };
        internal static readonly int[] towerCosts = { 2270, 95, 470, 470, 470, 470, 800, 8000, 5500 };
        internal static readonly string[] otherTowerTowersets = { "Military", "Support", "Support", "Support", "Support", "Support", "Support", "Magic", "Magic" };
    }

    public class Utils
    {
        public static void RegisterTowerInInventory(ShopTowerDetailsModel details, string insertBefore, List<TowerDetailsModel> allTowersInTheGame)
        {
            // get the tower details for the tower insertBefore and insert the new tower into the index towerBefore is at, shifting everything after it by 1
            TowerDetailsModel towerAfter = allTowersInTheGame.ToArray().FirstOrDefault(tower => tower.towerId == insertBefore);
            allTowersInTheGame.Insert(allTowersInTheGame.IndexOf(towerAfter), details);
        }
    }

    public class Mod : MelonMod { }

    [HarmonyPatch(typeof(ProfileModel), "Validate")] // this method is called after the profile data is parsed, hence why it's used to modify said profile data
    public class ProfileModel_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ProfileModel __instance)
        {
            List<string> unlockedTowers = __instance.unlockedTowers;
            if (unlockedTowers.Contains("TransformedBaseMonkey")) return;

            foreach (string power in Constants.powers) unlockedTowers.Add(power);
            foreach (string otherTower in Constants.otherTowers) unlockedTowers.Add(otherTower);

            Logger.Log("Added needed towers to the list of unlocked towers", (int)ConsoleColor.Cyan, Logger.Level.Normal);
        }
    }

    [HarmonyPatch(typeof(TitleScreen), "UpdateVersion")] // this method is called soon after the game is done initializing the models, hence why it's used to modify said models
    public class GameModel_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            for (int i = 0; i < Constants.powers.Length; i++)
            {
                PowerModel powerModel = Game.instance.model.GetPowerWithName(Constants.powers[i]);

                if (powerModel.tower.icon == null) powerModel.tower.icon = powerModel.icon;

                powerModel.tower.cost = Constants.powerCosts[i];
                powerModel.tower.towerSet = "Support";
            }

            for (int i = 0; i < Constants.otherTowers.Length; i++)
            {
                TowerModel otherTowerModel = Game.instance.model.GetTowerWithName(Constants.otherTowers[i]);

                if (otherTowerModel.icon == null) otherTowerModel.icon = otherTowerModel.portrait;
                if (Constants.otherTowers[i] == "SunAvatarMini") otherTowerModel.icon = Game.instance.model.GetTowerWithName("SuperMonkey-300").portrait;
                if (Constants.otherTowers[i] == "TransformedBaseMonkey") otherTowerModel.icon = Game.instance.model.GetTowerWithName("Alchemist-050").portrait;

                otherTowerModel.cost = Constants.towerCosts[i];
                otherTowerModel.towerSet = Constants.otherTowerTowersets[i];
            }

            Logger.Log("Configured tower models", (int)ConsoleColor.Cyan, Logger.Level.Normal);
        }
    }

    [HarmonyPatch(typeof(TowerInventory), "Init")] // this method tells the game to create buttons for a given list of towers, allTowersInTheGame, which we modify here
    public class TowerInit_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref List<TowerDetailsModel> allTowersInTheGame)
        {
            for (int i = 0; i < Constants.powers.Length; i++)
            {
                ShopTowerDetailsModel powerDetails = new ShopTowerDetailsModel(Constants.powers[i],
                    allTowersInTheGame.ToArray().FirstOrDefault(tower => tower.name.Contains("EngineerMonkey")).towerIndex, 0, 0, 0, -1, -1, null);
                Utils.RegisterTowerInInventory(powerDetails, "EngineerMonkey", allTowersInTheGame);
            }

            for (int i = 0; i < Constants.otherTowers.Length; i++)
            {
                ShopTowerDetailsModel otherTowerDetails = new ShopTowerDetailsModel(Constants.otherTowers[i], -1, 0, 0, 0, -1, -1, null);

                string insertBefore = "EngineerMonkey"; // support had the most occurrences, so have it default to mortar to eliminate the conditional for military

                if (Constants.otherTowerTowersets[i] == "Military") insertBefore = "MortarMonkey"; // military towers
                if (Constants.otherTowerTowersets[i] == "Magic") insertBefore = "Druid"; // magic towers

                otherTowerDetails.towerIndex = allTowersInTheGame.ToArray().FirstOrDefault(tower => tower.name.Contains(insertBefore)).towerIndex;
                Utils.RegisterTowerInInventory(otherTowerDetails, insertBefore, allTowersInTheGame);
            }

            Logger.Log("Registered tower models in tower inventory", (int)ConsoleColor.Cyan, Logger.Level.Normal);
            return true;
        }
    }
}