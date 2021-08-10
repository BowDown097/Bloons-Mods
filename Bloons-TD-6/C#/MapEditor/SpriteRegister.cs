using Assets.Scripts.Utils;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditor
{
    public static class SpriteRegister
    {
        public static Dictionary<string, Sprite> register = new Dictionary<string, Sprite>();

        public static Texture2D TextureFromPNG(string path)
        {
            Texture2D text = new Texture2D(2, 2);

            if (!ImageConversion.LoadImage(text, File.ReadAllBytes(path)))
            {
                throw new Exception("Could not acquire texture from file " + Path.GetFileName(path) + ".");
            }

            return text;
        }

        public static Texture2D TextureFromLink(string path, string url)
        {
            using (WebClient client = new WebClient())
            {
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    client.DownloadFile(url, path);
                }
            }
			
			return TextureFromPNG(path);
        }

        public static void RegisterSpriteFromImage(string path)
        {
            Texture2D pngTexture = TextureFromPNG(path);
            register.Add(path, Sprite.Create(pngTexture, new Rect(0.0f, 0.0f, pngTexture.width, pngTexture.height), default));
        }

        public static void RegisterSpriteFromURL(string path, string url)
        {
            Texture2D pngTexture = TextureFromLink(path, url);
            register.Add(path, Sprite.Create(pngTexture, new Rect(0.0f, 0.0f, pngTexture.width, pngTexture.height), default));
        }
    }

    [HarmonyPatch(typeof(ResourceLoader), "LoadSpriteFromSpriteReferenceAsync")]
    public class SpriteRegister_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(ref SpriteReference reference, ref Image image)
        {
            if (reference != null && image != null && SpriteRegister.register.Count > 0)
            {
                string guid = reference.GUID;
                KeyValuePair<string, Sprite>? entryAtRef = SpriteRegister.register.Where(e => e.Key == guid).Select(e => (KeyValuePair<string, Sprite>?)e).FirstOrDefault();
                if (entryAtRef.HasValue)
                {
                    Sprite sprite = entryAtRef.Value.Value;
                    if (sprite == null)
                    {
                        Texture2D pngTexture = SpriteRegister.TextureFromPNG(guid);
                        sprite = Sprite.Create(pngTexture, new Rect(0.0f, 0.0f, pngTexture.width, pngTexture.height), default);
                    }
                    image.canvasRenderer.SetTexture(sprite.texture);
                    image.sprite = sprite;
                }
            }
        }
    }
}