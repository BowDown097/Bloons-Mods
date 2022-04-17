using Assets.Scripts.Unity.Display;
using BTD_Mod_Helper.Api.Display;

namespace Glych;
public class BossDisplay : ModDisplay
{
    public override string BaseDisplay => "c8a44811c9166a745987fcdb5a92567b";

    public override void ModifyDisplayNode(UnityDisplayNode node) => Set2DTexture(node, "glych");
}