using Assets.Scripts.Unity.UI_New.ChallengeEditor;
using Harmony;
using MelonLoader;
using TMPro;

namespace NoLimitsEditor
{
    public class Mod : MelonMod { }

    [HarmonyPatch(typeof(ChallengeEditor), "ValidateInputFields")]
    public class Validation_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(ChallengeEditor), "SetupUI")]
    public class ChallengeEditor_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ChallengeEditor __instance)
        {
            __instance.cash.characterLimit = __instance.lives.characterLimit = __instance.maxLives.characterLimit = __instance.round.characterLimit = 
                __instance.endRound.characterLimit = 9999;
            __instance.bloonSpeed.maxValue = __instance.moabSpeed.maxValue = __instance.ceramicHealth.maxValue = __instance.moabHealth.maxValue = 9999;
            __instance.chalName.characterLimit = 9999;
            __instance.chalName.characterValidation = TMP_InputField.CharacterValidation.None;    
        }
    }
}