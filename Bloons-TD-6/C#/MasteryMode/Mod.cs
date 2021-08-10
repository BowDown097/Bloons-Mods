using Assets.Main.Scenes;
using Assets.Scripts.Models.Bloons;
using Assets.Scripts.Models.Rounds;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Track;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.Bridge;
using Harmony;
using MelonLoader;
using System;
using System.Collections.Generic;
using UnhollowerBaseLib;

namespace MasteryMode
{
    /*
	* Bloon conversions (modifiers are respected):
	* Red -> Blue
	* Blue -> Green
	* Green -> Yellow
	* Yellow -> Pink
	* Pink -> Black/White (RNG)
	* Black -> Zebra
	* White -> Purple
	* Purple -> Lead
	* Lead -> Zebra/Rainbow (RNG)
	* Zebra -> Rainbow
	* Rainbow -> Ceramic
	* Ceramic -> MOAB
	* MOAB -> BFB
	* BFB -> DDT
	* DDT -> ZOMG
	* ZOMG -> BAD
	*/

    public class Mod : MelonMod
    {
        public static readonly Dictionary<string, string> promotionMap = new Dictionary<string, string>()
        {
            { "Red", "Blue" },
            { "RedCamo", "BlueCamo" },
            { "RedRegrow", "BlueRegrow" },
            { "RedRegrowCamo", "BlueRegrowCamo" },

            { "Blue", "Green" },
            { "BlueCamo", "GreenCamo" },
            { "BlueRegrow", "GreenRegrow" },
            { "BlueRegrowCamo", "GreenRegrowCamo" },

            { "Green", "Yellow" },
            { "GreenCamo", "YellowCamo" },
            { "GreenRegrow", "YellowRegrow" },
            { "GreenRegrowCamo", "YellowRegrowCamo" },

            { "Yellow", "Pink" },
            { "YellowCamo", "PinkCamo" },
            { "YellowRegrow", "PinkRegrow" },
            { "YellowRegrowCamo", "PinkRegrowCamo" },
			
			// pink is done in rng
			
			{ "Black", "Zebra" },
            { "BlackCamo", "ZebraCamo" },
            { "BlackRegrow", "ZebraRegrow" },
            { "BlackRegrowCamo", "ZebraRegrowCamo" },

            { "White", "Purple" },
            { "WhiteCamo", "PurpleCamo" },
            { "WhiteRegrow", "PurpleRegrow" },
            { "WhiteRegrowCamo", "PurpleRegrowCamo" },

            { "Purple", "LeadFortified" },
            { "PurpleCamo", "LeadFortifiedCamo" },
            { "PurpleRegrow", "LeadRegrowFortified" },
            { "PurpleRegrowCamo", "LeadRegrowFortifiedCamo" },

			// lead is also done in rng

			{ "Zebra", "Rainbow" },
            { "ZebraCamo", "RainbowCamo" },
            { "ZebraRegrow", "RainbowRegrow" },
            { "ZebraRegrowCamo", "RainbowRegrowCamo" },

            { "Rainbow", "Ceramic" },
            { "RainbowCamo", "CeramicCamo" },
            { "RainbowRegrow", "CeramicRegrow" },
            { "RainbowRegrowCamo", "CeramicRegrowCamo" },

            { "Ceramic", "Moab" },
            { "CeramicCamo", "Moab" },
            { "CeramicRegrow", "Moab" },
            { "CeramicRegrowCamo", "Moab" },
            { "CeramicFortified", "MoabFortified" },
            { "CeramicFortifiedCamo", "MoabFortified" },
            { "CeramicRegrowFortified", "MoabFortified" },
            { "CeramicRegrowFortifiedCamo", "MoabFortified" },

            { "Moab", "Bfb" },
            { "MoabFortified", "BfbFortified" },

            { "Bfb", "DdtCamo" },
            { "BfbFortified", "DdtFortifiedCamo" },

            { "DdtCamo", "Zomg" },
            { "DdtFortifiedCamo", "ZomgFortified" },

            { "Zomg", "Bad" },
            { "ZomgFortified", "BadFortified" },

            { "Bad", "BadFortified" },
            { "BadFortified", "BadFortified" }
        };

        public static Random random = new Random();
    }

    // half cash
    [HarmonyPatch(typeof(Simulation), "AddCash")]
    public class AddCash_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref double c, ref Simulation.CashSource source)
        {
            if (source != Simulation.CashSource.CoopTransferedCash && source != Simulation.CashSource.TowerSold) c *= 0.5;
            return true;
        }
    }

    // rng
    [HarmonyPatch(typeof(Spawner), "Emit")]
    public class GetBloonModel_Patch
    {
        public static BloonModel BloonsendRng(BloonModel bloon, string bloonToPatch, bool randCond, string randBloon1, string randBloon2)
        {
            string bloonId = bloon.id;
            if (bloonId.Contains(bloonToPatch))
            {
                string mod = string.Empty;
                if (bloonId != bloonToPatch) // if it's not the same as bloonToPatch but contains, it has modifiers
                {
                    mod = bloonId.Substring(4, bloonId.Length - 4).Replace("Fortified", "");
                }

                return randCond ? Game.instance.model.GetBloon(randBloon1 + mod) : Game.instance.model.GetBloon(randBloon2 + mod);
            }

            return bloon;
        }

        [HarmonyPrefix]
        public static bool Prefix(ref BloonModel bloon)
        {
            bloon = BloonsendRng(bloon, "Pink", Mod.random.Next(1, 3) == 1, "White", "Black");
            bloon = BloonsendRng(bloon, "Lead", Mod.random.Next(1, 12) >= 8, "Zebra", "Rainbow");

            return true;
        }
    }

    // promote bloons in roundsets
    [HarmonyPatch(typeof(TitleScreen), "UpdateVersion")]
    public class Game_Patch
    {
        public static string PromoteBloon(string bloon)
        {
            // setup pink RNG
            if (bloon.Contains("Pink") || bloon.Contains("Lead")) return bloon;

            return Mod.promotionMap[bloon];
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            // promotion
            for (int i = 0; i < Game.instance.model.roundSets.Length; i++)
            {
                RoundSetModel roundSet = Game.instance.model.roundSets[i];
                for (int j = 0; j < roundSet.rounds.Length; j++)
                {
                    RoundModel round = roundSet.rounds[j];

                    if (j == 99) // round 100 patch (3 spaced fortified BADs)
                    {
                        round.groups[0].count += 2;
                        round.groups[0].end += 2666;
                    }

                    for (int k = 0; k < round.groups.Length; k++)
                    {
                        BloonGroupModel bloonGroup = round.groups[k];
                        bloonGroup.bloon = PromoteBloon(bloonGroup.bloon);
                    }
                }
            }
        }
    }
}