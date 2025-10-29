using System.IO;
using UnityEngine;

namespace Save
{
    public class FileSaveStore : ISaveStore
    {
        private readonly string path;
        public FileSaveStore(string filename = "demo_save.json")
        {
            path = Path.Combine(Application.persistentDataPath, filename);
        }

        public bool TryLoad(out string json)
        {
            if (File.Exists(path))
            {
                json = File.ReadAllText(path);
                return !string.IsNullOrEmpty(json);
            }
            json = null;
            return false;
        }

        public void Save(string json)
        {
            // 简单写入；若要更安全可先写 .tmp 再替换
            File.WriteAllText(path, json);
        }

        public void Wipe()
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
