using Assets.Scripts.Unity.Localization;
using Newtonsoft.Json.Linq;
using NLayer;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace BetterJukebox
{
    class AudioUtils
    {
        /// <summary>
        /// Creates a new <see cref="AudioClip"/> from a given MP3 file.
        /// </summary>
        /// <param name="path">The path to an MP3 file.</param>
        /// <param name="clipName">The name that you want to give the resulting <see cref="AudioClip"/>.</param>
        /// <param name="useSerialization">Will serialize the resulting <see cref="AudioClip"/> if true for a large increase in loading time on subsequent runs at the cost of disk space.</param>
        /// <returns>Will return a new <see cref="AudioClip"/> instance if the file exists and is valid; otherwise, returns null.</returns>
        public static AudioClip AudioClipFromMP3(string path, string clipName, bool useSerialization)
        {
            string samplesFile = path.Replace("mp3", "samples");
            string infoFile = path.Replace("mp3", "mpeginfo");
            // if serialized data exists, deserialize instead of constructing the AudioClip
            if (File.Exists(samplesFile) && File.Exists(infoFile) && useSerialization)
            {
                using (FileStream samplesIn = new FileStream(samplesFile, FileMode.Open))
                {
                    BinaryFormatter formatterIn = new BinaryFormatter();
                    float[] samples = (float[])formatterIn.Deserialize(samplesIn);
                    JObject infoFromFile = JObject.Parse(File.ReadAllText(path.Replace("mp3", "mpeginfo")));
                    AudioClip clipOut = AudioClip.Create(clipName, samples.Length / 2, infoFromFile["channels"].Value<int>(), infoFromFile["sampleRate"].Value<int>(), false);
                    clipOut.SetData(samples, 0);
                    return clipOut;
                }
            }

            // since there was nothing to deserialize...
            // construct an MpegFile from mp3 file, read samples into float array
            NKHook6.Logger.Log("Parsing and serializing " + Path.GetFileName(path), (int)ConsoleColor.Cyan, NKHook6.Logger.Level.Normal);
            using (MpegFile mpegFile = new MpegFile(path))
            {
                float[] samples = new float[mpegFile.Length / sizeof(float)];
                mpegFile.ReadSamples(samples, 0, samples.Length);

                // construct an AudioClip from the MpegFile
                AudioClip clipOut = AudioClip.Create(clipName, samples.Length / 2, mpegFile.Channels, mpegFile.SampleRate, false);
                clipOut.SetData(samples, 0);

                if (useSerialization)
                {
                    // serialize AudioClip and write to 2 new files, [audioname].mpeginfo and [audioname].samples
                    using (FileStream samplesOut = new FileStream(samplesFile, FileMode.Create))
                    {
                        BinaryFormatter formatterOut = new BinaryFormatter();
                        formatterOut.Serialize(samplesOut, samples);
                    }
                    JObject infoObj = new JObject(new JProperty("channels", mpegFile.Channels), new JProperty("sampleRate", mpegFile.SampleRate));
                    File.WriteAllText(path.Replace("mp3", "mpeginfo"), infoObj.ToString());
                }

                return clipOut;
            }
        }

        public static void AddAudioClipToJukebox(AudioJukeBox jukeBoxTracks, AudioClip clip, string name)
        {
            jukeBoxTracks.audioTracks.Add(clip);
            jukeBoxTracks.purchasedTracks.Add(true);

            LocalizationManager.instance.textTable.Add("JukeboxMusicCustom" + clip.name, name);
            LocalizationManager.instance.defaultTable.Add("JukeboxMusicCustom" + clip.name, name);
        }
    }
}
