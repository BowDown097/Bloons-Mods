using Assets.Main.Scenes;
using Assets.Scripts.Unity;
using Assets.Scripts.Unity.UI_New.Popups;
using Harmony;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BetterJukebox
{
    public class Mod : MelonMod 
    {
        // forgive me allah that i have to do this
        public static List<string> clipNames = new List<string>();
        public static List<string> clipLocs = new List<string>();
    }

    [HarmonyPatch(typeof(AudioJukeBox), "Init")]
    public class AudioJukeBox_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(AudioJukeBox __instance)
        {
            // unlock every track, i hate NK for making the jukebox system horrible
            for (int i = 0; i < __instance.purchasedTracks.Count; i++)
            {
                __instance.purchasedTracks[i] = true;
            }
        }
    }

    [HarmonyPatch(typeof(MusicLocalizationManager), "Init")]
    public class MusicLoc_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(MusicLocalizationManager __instance)
        {
            // have to update localization manager manually too, my face when
            if (__instance.locDictionary.ContainsKey(Mod.clipNames[0])) return;

            for (int i = 0; i < Mod.clipNames.Count; i++)
            {
                __instance.locDictionary.Add(Mod.clipNames[i], Mod.clipLocs[i]);
            }
        }
    }

    [HarmonyPatch(typeof(TitleScreen), "UpdateVersion")]
    public class TitleScreen_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(TitleScreen __instance)
        {
            Action<int> loadTracksAction = new Action<int>(delegate (int i)
            {
                foreach (string mp3 in Directory.GetFiles("Jukebox", "*.mp3", SearchOption.AllDirectories))
                {
                    string clipName = Path.GetFileNameWithoutExtension(mp3);
                    AudioClip clip = AudioUtils.AudioClipFromMP3(mp3, clipName, true);
                    Mod.clipNames.Add(clipName);
                    Mod.clipLocs.Add("JukeboxMusicCustom" + clipName);
                    AudioUtils.AddAudioClipToJukebox(Game.instance.jukeBoxTracks, clip, clipName);
                }
            });

            Directory.CreateDirectory("Jukebox");
            PopupScreen.instance.ShowSetValuePopup("Give me your MP3s!",
                "Place MP3 files that you want to add to the jukebox into the newly created 'Jukebox' folder in the BTD6 directory, then click 'OK.' " +
                "The game will likely have a lag spike, but don't worry, it won't crash. However, if the number below is not 0, be afraid. Something bad might happen.", 
                loadTracksAction, Marshal.GetLastWin32Error());
        }
    }
}