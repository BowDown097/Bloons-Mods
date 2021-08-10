using Assets.Scripts.Data.MapSets;
using Assets.Scripts.Models.Map;
using Assets.Scripts.Unity.Display;
using Assets.Scripts.Unity.UI_New;
using Assets.Scripts.Unity.UI_New.InGame;
using Assets.Scripts.Unity.UI_New.Popups;
using Assets.Scripts.Unity.UI_New.Utils;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using System.IO;
using System.Linq;
using UnhollowerBaseLib;
using UnityEngine;
using static TMPro.TMP_InputField;

namespace MapEditor
{
    public class Editor : MelonMod
    {
        // editor globals
        public static System.Random r = new System.Random();
        public static bool writingPoint = false;
        public static bool writingArea = false;
        public static bool isEditor = false;
        public static int index = 0;
        public static AreaType type = AreaType.track;

        // gui globals
        public static bool initializedGui = false;
        public static bool hideMainGui = false;
        public static bool hideTips = false;
        public static GUIStyle editorStyle = null;
        public static GUIStyle pathButtonStyle = null;
        public static GUIStyle maskButtonStyle = null;
        public static GUIStyle trackMaskButtonStyle = null;
        public static GUIStyle waterMaskButtonStyle = null;
        public static GUIStyle landMaskButtonStyle = null;
        public static GUIStyle unplaceableMaskButtonStyle = null;
        public static GUIStyle iceMaskButtonStyle = null;
        public static GUIStyle mapPicButtonStyle = null;
        public static List<Rect> boxPoints = new List<Rect>();
        public static Texture2D lineTex = null;

        public Texture2D Texture2DFromColor(Color color)
        {
            Texture2D text = new Texture2D(1, 1);
            text.SetPixel(0, 0, color);
            text.Apply();

            return text;
        }

        public GUIStyle CreateButtonStyle(Texture2D active, Texture2D normal)
        {
            GUIStyle style = new GUIStyle();
            style.active.background = active;
            style.normal.background = normal;

            return style;
        }

        // mostly from https://wiki.unity3d.com/index.php/DrawLine
        public void DrawNodes()
        {
            for (int i = 0; i < boxPoints.Count; i++)
            {
                GUI.Box(boxPoints[i], GUIContent.none);

                if (boxPoints.Count != 1 && i < boxPoints.Count - 1)
                {
                    Matrix4x4 matrix = GUI.matrix;

                    Vector2 pointA = new Vector2(boxPoints[i].x, boxPoints[i].y);
                    Vector2 pointB = new Vector2(boxPoints[i + 1].x, boxPoints[i + 1].y);
                    float angle = Vector3.Angle(pointB - pointA, Vector2.right);
                    if (pointA.y > pointB.y)
                    {
                        angle = -angle;
                    }

                    GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, 1), new Vector2(pointA.x, pointA.y + 0.5f));
                    GUIUtility.RotateAroundPivot(angle, pointA);
                    GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);
                    GUI.matrix = matrix;
                }
            }
        }

        public override void OnGUI()
        {
            if (InGame.instance != null && InGame.instance.bridge != null)
            {
                DrawNodes();

                if (writingPoint && !hideTips)
                {
                    GUI.Box(new Rect(Screen.width / 2 - 400, 40, 800, 50f), 
                        "Click to create nodes. Press F1 to exit node creation mode. Press F3 to hide this box.", 
                        editorStyle);
                }

                if (writingArea && !hideTips)
                {
                    GUI.Box(new Rect(Screen.width / 2 - 400, 40, 800, 50f),
                        $"Click to define {Enum.GetName(typeof(AreaType), type)} areas, or select one of the areas using the buttons below. Press F2 to exit area creation mode. Press F3 to hide this box.", 
                        editorStyle);

                    // yes ik this is disgusting, but this is literally how official unity documentation does it, i don't think there's a better way..
                    if (GUI.Button(new Rect(Screen.width / 2 - 350, 100, 42, 42), string.Empty, trackMaskButtonStyle))
                        type = AreaType.track;
                    if (GUI.Button(new Rect(Screen.width / 2 - 300, 100, 42, 42), string.Empty, waterMaskButtonStyle))
                        type = AreaType.water;
                    if (GUI.Button(new Rect(Screen.width / 2 - 250, 100, 42, 42), string.Empty, landMaskButtonStyle))
                        type = AreaType.land;
                    if (GUI.Button(new Rect(Screen.width / 2 - 200, 100, 42, 42), string.Empty, unplaceableMaskButtonStyle))
                        type = AreaType.unplaceable;
                    if (GUI.Button(new Rect(Screen.width / 2 - 150, 100, 42, 42), string.Empty, iceMaskButtonStyle))
                        type = AreaType.ice;
                }

                if (isEditor && !hideMainGui)
                {
                    // initialize gui if needed
                    if (!initializedGui)
                    {
                        lineTex = Texture2DFromColor(Color.red);

                        editorStyle = new GUIStyle { alignment = TextAnchor.MiddleCenter };
                        editorStyle.normal.textColor = Color.white;
                        editorStyle.normal.background = Texture2DFromColor(Color.black);
                        GUI.skin.box.normal.background = Texture2DFromColor(Color.red);

                        pathButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromLink("Mods/MapEditor/EditorAssets/edit_path_node_selected.png", "https://i.imgur.com/vyxMs9W.png"),
                            SpriteRegister.TextureFromLink("Mods/MapEditor/EditorAssets/edit_path_node_unselected.png", "https://i.imgur.com/7anJu50.png"));
                        maskButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromLink("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_selected.png", "https://i.imgur.com/NNV3ubR.png"),
                            SpriteRegister.TextureFromLink("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_unselected.png", "https://i.imgur.com/Tbv53Ij.png"));
                        trackMaskButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_selected.png"),
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_unselected.png"));
                        waterMaskButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_selected.png"),
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_unselected.png"));
                        landMaskButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_selected.png"),
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_unselected.png"));
                        unplaceableMaskButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_selected.png"),
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_unselected.png"));
                        iceMaskButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_selected.png"),
                            SpriteRegister.TextureFromPNG("Mods/MapEditor/EditorAssets/edit_mask_paintbrush_unselected.png"));
                        mapPicButtonStyle = CreateButtonStyle(
                            SpriteRegister.TextureFromLink("Mods/MapEditor/EditorAssets/map_select_selected.png", "https://i.imgur.com/q2fFytq.png"),
                            SpriteRegister.TextureFromLink("Mods/MapEditor/EditorAssets/map_select_unselected.png", "https://i.imgur.com/7m1WtEy.png"));

                        initializedGui = true;
                    }

                    GUI.Box(new Rect(Screen.width / 2 - 400, 40, 800, 50f), "Map Editor", editorStyle);

                    if (GUI.Button(new Rect(Screen.width / 2 - 400, 90, 42, 42), string.Empty, pathButtonStyle))
                    {
                        File.AppendAllText("map.cs", "List<PointInfo> list = new List<PointInfo>();\n");
                        hideMainGui = true;
                        writingPoint = !writingPoint;
                    }

                    if (GUI.Button(new Rect(Screen.width / 2 - 350, 90, 42, 42), string.Empty, maskButtonStyle))
                    {
                        if (!File.ReadAllText("map.cs").Contains("List<AreaModel> newAreas"))
                        {
                            File.AppendAllText("map.cs", "List<AreaModel> newAreas = new List<AreaModel>();\n");
                        }

                        File.AppendAllText("map.cs", "var area" + index + " = new Il2CppSystem.Collections.Generic.List<Assets.Scripts.Simulation.SMath.Vector2>();\n");
                        hideMainGui = true;
                        writingArea = !writingArea;
                    }

                    if (GUI.Button(new Rect(Screen.width / 2 - 300, 90, 42, 42), string.Empty, mapPicButtonStyle))
                    {
                        PopupScreen.instance.ShowSetNamePopup("Map texture",
                            "Put the path of the PNG file which will be the map texture", (Action<string>)delegate (string s)
                            {
                                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                cube.transform.position = new Vector3(0, -0.5001f, 0);
                                cube.transform.localScale = new Vector3(-300, 1f, -235);

                                foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>())
                                {
                                    foreach (Renderer r in obj.GetComponents<Renderer>())
                                    {
                                        if (r.name.Contains("Terrain"))
                                        {
                                            cube.GetComponent<Renderer>().material = r.material;
                                            cube.GetComponent<Renderer>().material.mainTexture = SpriteRegister.TextureFromPNG(s);
                                        }
                                        else if (r.name != "Cube" && r.name != "Blank" && !r.name.Contains("Project"))
                                        {
                                            UnityEngine.Object.Destroy(r.gameObject);
                                        }
                                    }
                                }
                            }, string.Empty);

                        foreach (Popup popup in new List<Popup>(PopupScreen.instance.AllActivePopups()))
                        {
                            DebugValueScreen valScreen = popup.GetComponent<DebugValueScreen>();
                            if (valScreen != null)
                            {
                                valScreen.input.characterLimit = 9999;
                                valScreen.input.characterValidation = CharacterValidation.None;
                            }
                        }
                    }
                }
            }
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
            NKHook6.Logger.Log("Map Editor initialized! Note: If the console throws an error when you exit the map, you can ignore it.");
            File.Create("map.cs");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (InGame.instance != null && InGame.instance.bridge != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 v3 = Input.mousePosition;
                    v3 = InGame.instance.sceneCamera.ScreenToWorldPoint(v3);
                    float x = v3.x;
                    float y = v3.y * -2.3f;
                    if (writingPoint)
                    {
                        boxPoints.Add(new Rect(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y), new Vector2(5, 5)));
                        File.AppendAllText("map.cs", @"list.Add(new PointInfo()
{
    bloonScale = 1, 
    bloonsInvulnerable = false, 
    distance = 1, 
    id = r.NextDouble + string.Empty, 
    moabScale = 1, 
    moabsInvulnerable = false, 
    rotation = 0, 
    point = new Assets.Scripts.Simulation.SMath.Vector3(" + x + @"f, " + y + @"f) 
});\n");
                    }
                    if (writingArea)
                    {
                        File.AppendAllText("map.cs", "area" + index + ".Add(new Assets.Scripts.Simulation.SMath.Vector2(" + x + "f, " + y + "f));\n");
                    }
                }

                if (Input.GetKeyDown(KeyCode.F1))
                {
                    if (writingPoint)
                    {
                        File.AppendAllText("map.cs", "return list.ToArray();\n");
                    }
                    hideMainGui = !hideMainGui;
                    hideTips = false;
                    writingPoint = !writingPoint;
                }

                if (Input.GetKeyDown(KeyCode.F2))
                {
                    if (writingArea)
                    {
                        File.AppendAllText("map.cs", 
                            "newAreas.Add(new AreaModel(\"lol" + index + "\", new Assets.Scripts.Simulation.SMath.Polygon(area" + index + "), 10, (AreaType)" + (int)type + "));\n");
                        index++;
                    }
                    hideMainGui = !hideMainGui;
                    hideTips = false;
                    writingArea = !writingArea;
                }

                if (Input.GetKeyDown(KeyCode.F3))
                {
                    hideTips = !hideTips;
                }

                if (Input.GetKeyDown(KeyCode.F4))
                {
                    isEditor = !isEditor;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Factory), "Flush")]
    public class Flush_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            Editor.boxPoints.Clear();
            Editor.hideMainGui = false;
            Editor.hideTips = false;
            Editor.initializedGui = false;
            Editor.isEditor = false;
            Editor.writingArea = false;
            Editor.writingPoint = false;
            Editor.index = 0;
            Editor.type = AreaType.track;

            return true;
        }
    }

    [HarmonyPatch(typeof(UI), "Awake")]
    public class Awake_Patch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            // god i hate this, there's surely a better way right?
            MapDetails[] maps = new MapDetails[UI.instance.mapSet.Maps.items.Count];
            UI.instance.mapSet.Maps.items.CopyTo(maps, 0);
            UI.instance.mapSet.Maps.items = maps.Where(map => map.id == "Tutorial").ToArray();
        }
    }
}