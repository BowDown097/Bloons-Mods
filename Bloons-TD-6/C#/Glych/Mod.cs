using Assets.Main.Scenes;
using Assets.Scripts.Simulation;
using Assets.Scripts.Simulation.Bloons;
using Assets.Scripts.Unity.Bridge;
using Assets.Scripts.Unity.UI_New.InGame;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using MelonLoader;
using NinjaKiwi.Common;
using UnityEngine;

[assembly: MelonInfo(typeof(Glych.Mod), "Glych Boss", "1.0.0", "BowDown097")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
namespace Glych;
public class Mod : BloonsTD6Mod
{
    [HarmonyPatch(typeof(TitleScreen), nameof(TitleScreen.Start))]
    public static class AddBoss
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            BossBuilder boss = new("Glych", typeof(BossDisplay));
            for (int i = 1; i <= 100; i++)
            {
                boss.AddStage(100 * (i-2), i);
            }
        }
    }

    public override void OnRoundStart()
    {
        base.OnRoundStart();
        foreach (BossBuilder boss in BossBuilder.Bosses)
        {
            BossStage currentStage = boss.Stages.Find(s => s.round == InGame.instance.bridge.GetCurrentRound() + 1);
            if (currentStage.Equals(default))
                continue;
            boss.BaseModel.maxHealth = currentStage.health;
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (BossBuilder.Bosses.Count == 0 || InGame.instance?.bridge?.GetAllBloons() == null)
            return;

        GameObject? roundPanel = InGame.instance?.mapRect?.transform?.Find("MainHudRightAlign(Clone)/RoundPanel").gameObject;
        if (roundPanel == null)
            return;
        NK_TextMeshProUGUI roundTitle = roundPanel.GetComponent<NK_TextMeshProUGUI>("Title");

        foreach (BossBuilder boss in BossBuilder.Bosses)
        {
            Bloon? bossBloon = InGame.instance?.GetBloons()?.FirstOrDefault(b => b.bloonModel.name == boss.Name);
            if (bossBloon == null)
            {
                if (roundTitle.transform.localPosition.x == -850)
                {
                    roundTitle.transform.localPosition = new Vector3(-278, 84, 0);
                    roundTitle.transform.localScale = new Vector3(1, 1, 1);
                    roundTitle.text = LocalizationManager.Instance.GetText("Round");
                }
                continue;
            }

            BossStage currentStage = boss.Stages.Find(s => s.round == InGame.instance?.bridge.GetCurrentRound() + 1);
            if (bossBloon.bloonModel.maxHealth != boss.BaseModel.maxHealth)
                bossBloon.bloonModel.maxHealth = boss.BaseModel.maxHealth;
            if (bossBloon.health > boss.BaseModel.maxHealth)
                bossBloon.SetHealth(boss.BaseModel.maxHealth);

            roundTitle.transform.localPosition = new Vector3(-850, 130, 0);
            roundTitle.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            roundTitle.text = $"{bossBloon.bloonModel.name}: {bossBloon.health}/{boss.BaseModel.maxHealth}";
        }
    }
}