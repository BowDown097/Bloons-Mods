using Newtonsoft.Json;
using System.IO;

namespace MapEditor
{
    /// <summary>
    /// This class saves other classes to json file
    /// </summary>
    public static class Serializer
    {
        public static T LoadFromFile<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath)) return null;

            string json = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json)) return null;

            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveToFile<T>(T jsonObject, string savePath, bool overwriteExisting = true) where T : class
        {
            CreateDirIfNotFound(savePath);
            string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);

            bool keepOriginal = !overwriteExisting;
            StreamWriter serialize = new StreamWriter(savePath, keepOriginal);
            serialize.Write(json);
            serialize.Close();
        }

        private static void CreateDirIfNotFound(string dir)
        {
            FileInfo f = new FileInfo(dir);
            Directory.CreateDirectory(f.Directory.FullName);
        }
    }
}
