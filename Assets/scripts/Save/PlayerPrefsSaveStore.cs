using UnityEngine;

namespace Save
{
    public class PlayerPrefsSaveStore : ISaveStore
    {
        private readonly string key;

        public PlayerPrefsSaveStore(string key = "SaveSlot_1")
        {
            this.key = key;
        }

        public bool TryLoad(out string json)
        {
            json = PlayerPrefs.GetString(key, "");
            return !string.IsNullOrEmpty(json);
        }

        public void Save(string json)
        {
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }

        public void Wipe()
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}
