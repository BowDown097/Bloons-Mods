using Assets.Main.Scenes;
using Assets.Scripts.Unity;
using HarmonyLib;
using MelonLoader;
using Assets.Scripts.Data.Music;
using static BetterJukebox.Mod;
using UnityEngine;
using Assets.Scripts.Data;
using NinjaKiwi.Common;
using NLayer;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
#pragma warning disable SYSLIB0011

[assembly: MelonInfo(typeof(BetterJukebox.Mod),"Better Jukebox","1.0.2","BowDown097 & Silentstorm")]
[assembly: MelonGame("Ninja Kiwi","BloonsTD6")]
namespace BetterJukebox;
public class Mod : BloonsTD6Mod
{
    public static readonly List<AudioClip> clips = new();
    private static readonly ModSettingBool enableCache = new(false)
    {
        displayName = "Enable Track Caching"
    };

    public override void OnApplicationStart()
    {
        string jukeboxFolder = Path.Combine(MelonHandler.ModsDirectory, "Jukebox");
        if (!Directory.Exists(jukeboxFolder))
            Directory.CreateDirectory(jukeboxFolder);

        foreach (string mp3 in Directory.GetFiles(jukeboxFolder))
        {
            try
            {
                string samplesFile = mp3.Replace("mp3", "samples");
                string infoFile = mp3.Replace("mp3", "mpeginfo");
                string filename = Path.GetFileNameWithoutExtension(new FileInfo(mp3).Name);
                BinaryFormatter fmt = new();

                if (File.Exists(samplesFile) && File.Exists(infoFile) && enableCache)
                {
                    MelonLogger.Msg($"Reading from cache for {filename}");
                    using FileStream fs = new(samplesFile, FileMode.Open);
                    float[] fsSamples = (float[])fmt.Deserialize(fs);
                    JObject mpegInfo = JObject.Parse(File.ReadAllText(infoFile));
                    int channels = mpegInfo["channels"]?.Value<int>() ?? 0;
                    int sampleRate = mpegInfo["sampleRate"]?.Value<int>() ?? 0;
                    AudioClip fsClip = AudioClip.Create(filename, fsSamples.Length / 2, channels, sampleRate, false);
                    fsClip.SetData(fsSamples, 1);
                    clips.Add(fsClip);
                    return;
                }

                MelonLogger.Msg($"Reading directly for {filename}");
                using MpegFile mpegFile = new(mp3);
                float[] samples = new float[mpegFile.Length / mpegFile.Channels / 2];
                mpegFile.ReadSamples(samples, 0, samples.Length);
                AudioClip clip = AudioClip.Create(filename, samples.Length / 2, mpegFile.Channels, mpegFile.SampleRate, false);
                clip.SetData(samples, 1);
                clips.Add(clip);

                if (enableCache)
                {
                    using FileStream samplesOut = new(samplesFile, FileMode.Create);
                    fmt.Serialize(samplesOut, samples);
                    JObject infoOut = new(new JProperty("channels", mpegFile.Channels), new JProperty("sampleRate", mpegFile.SampleRate));
                    File.WriteAllText(infoFile, infoOut.ToString());
                }
            }
            catch (Exception exception)
            {
                MelonLogger.Error(exception.Message);
            }
        }
    }
}

[HarmonyPatch(typeof(TitleScreen),"OnPlayButtonClicked")]
public static class TitleScreenOnPlayButtonClicked_Patch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        foreach (AudioClip clip in clips)
        {
            Game.instance.audioFactory.RegisterAudioClip(clip.name, clip);
            LocalizationManager.Instance.textTable.Add(clip.name, clip.name);
            LocalizationManager.Instance.defaultTable.Add(clip.name, clip.name);
            MusicItem musicItem = new()
            {
                name = clip.name,
                freeTrack = true,
                Clip = clip,
                hideFlags = 0,
                id = clip.name,
                locKey = clip.name
            };
            GameData.Instance.audioJukeBox.musicTrackData.Add(musicItem);
        }
    }
}
