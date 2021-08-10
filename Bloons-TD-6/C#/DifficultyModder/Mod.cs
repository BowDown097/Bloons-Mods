using Assets.Main.Scenes;
using Assets.Scripts.Models.Difficulty;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.Popups;
using Harmony;
using MelonLoader;
using System;
using System.Collections.Generic;

namespace DifficultyModder
{
    public class Mod : MelonMod { }

    [HarmonyPatch(typeof(TitleScreen), "UpdateVersion")]
    public class TitleScreen_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            Dictionary<string, int> difficultyIndexes = new Dictionary<string, int>()
            {
                {"easy", 0},
                {"medium", 1},
                {"hard", 2},
                {"tutorial", 3}
            };

            DifficultyModel[] difficultyModels = Game.instance.model.difficultyModels;
            string diffStr = string.Empty;

            PopupScreen.instance.ShowSetNamePopup("What difficulty do you want to modify?",
                "Available difficulties are \"Easy,\" \"Medium,\" \"Hard,\" and \"Tutorial.\"",
                (Action<string>)delegate (string diffIn)
                {
                    diffStr = diffIn.ToLower();
                    DifficultyModel difficulty = difficultyModels[difficultyIndexes[diffStr]];
                    PopupScreen.instance.ShowSetValuePopup("What do you want the start round to be for your difficulty?",
                        "Any round 1-3865535 works!",
                        (Action<int>)delegate (int startRound)
                        {
                            difficulty.startRound = startRound;
                            PopupScreen.instance.ShowSetValuePopup("What do you want the end round to be for your difficulty?",
                                "Once again, any round 1-3865535 works!",
                                (Action<int>)delegate (int endRound)
                                {
                                    difficulty.endRound = endRound;
                                    PopupScreen.instance.ShowSetValuePopup("What do you want the reward monkey money to be for your difficulty?",
                                        "Be careful with this one, too high values could land you in the hacker pool or worse mess with your save!",
                                        (Action<int>)delegate (int mm)
                                        {
                                            difficulty.monkeyMoney = mm;
                                            PopupScreen.instance.ShowSetValuePopup("What do you want the intermediate monkey money scale to be for your difficulty?",
                                                "Just as the last one, be careful!",
                                                (Action<int>)delegate (int interScale)
                                                {
                                                    difficulty.intermediateMonkeyMoneyScale = interScale;
                                                    PopupScreen.instance.ShowSetValuePopup("What do you want the advanced monkey money scale to be for your difficulty?",
                                                        "Still be careful!",
                                                        (Action<int>)delegate (int advancedScale)
                                                        {
                                                            difficulty.advancedMonkeyMoneyScale = advancedScale;
                                                            PopupScreen.instance.ShowSetValuePopup("What do you want the expert monkey money scale to be for your difficulty?",
                                                                "This is the last one, and you still have to be careful!",
                                                                (Action<int>)delegate (int expertScale)
                                                                {
                                                                    difficulty.expertMonkeyMoneyScale = expertScale;
                                                                    difficultyModels[difficultyIndexes[diffStr]] = difficulty;
                                                                },
                                                                1
                                                            ); // SetValuePopup for difficulty.expertMonkeyMoneyScale
                                                        },
                                                        1
                                                    ); // SetValuePopup for difficulty.advancedMonkeyMoneyScale
                                                },
                                                1
                                            ); // SetValuePopup for difficulty.intermediateMonkeyMoneyScale
                                        },
                                        1
                                    ); // SetValuePopup for difficulty.monkeyMoney
                                },
                                1
                            ); // SetValuePopup for difficulty.endRound
                        },
                        1
                    ); // SetValuePopup for difficulty.startRound
                },
                string.Empty
            ); // SetNamePopup for selecting difficulty
        }
    }
}
