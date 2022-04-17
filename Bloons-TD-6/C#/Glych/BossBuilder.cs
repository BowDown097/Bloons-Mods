using System.Reflection;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Bloons;
using Assets.Scripts.Models.Bloons.Behaviors;
using Assets.Scripts.Models.Rounds;
using Assets.Scripts.Unity;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
#pragma warning disable CS8600, CS8601, CS8602, CS8618

namespace Glych;
public struct BossStage
{
    public float health;
    public int round;
    public Action? twentyPercentAction;

    public BossStage(float health, int round, Action? twentyPercentAction)
    {
        this.health = health;
        this.round = round;
        this.twentyPercentAction = twentyPercentAction;
    }
}

public class BossBuilder
{
    public static List<BossBuilder> Bosses { get; set; } = new();
    public BloonModel BaseModel { get; set; }
    public string Name { get; set; }
    public List<BossStage> Stages { get; set; } =  new();

    public BossBuilder(string name, Type displayType, BloonProperties properties = BloonProperties.None)
    {
        Name = name;
        BaseModel = Array.Find<BloonModel>(Game.instance.model.bloons, b => b.name == "BadFortified").Clone().Cast<BloonModel>();

        MethodInfo getDisplayGUID = typeof(ModContent).GetMethod("GetDisplayGUID");
        getDisplayGUID = getDisplayGUID.MakeGenericMethod(displayType);

        BaseModel.display = (string)getDisplayGUID.Invoke(null, null);
        BaseModel.bloonProperties = properties;
        BaseModel.tags.AddItem("Boss");
        BaseModel.damageDisplayStates.ForEach(state => state.displayPath = BaseModel.display);
        BaseModel.isBoss = true;
        BaseModel.id = name;
        BaseModel.updateChildBloonModels = true;
        BaseModel.childBloonModels = new Il2CppSystem.Collections.Generic.List<BloonModel>();
        BaseModel.behaviors.GetItemOfType<Model, SpawnChildrenModel>().children = Array.Empty<string>();

        Game.instance.model.bloons = Game.instance.model.bloons.AddTo(BaseModel);
        Bosses.Add(this);
    }

    public void AddStage(float health, int round, Action? twentyPercentAction = null)
    {
        Stages.Add(new BossStage(health, round, twentyPercentAction));
        foreach (RoundSetModel roundSet in Game.instance.model.roundSets)
        {
            BloonGroupModel bloonGroup = new($"Round{round}-{Name}", Name, 100, 0, 1);
            if (round - 1 < roundSet.rounds.Length)
                roundSet.rounds[round - 1].groups = roundSet.rounds[round - 1].groups.Prepend(bloonGroup).ToIl2CppReferenceArray();
        }
    }
}