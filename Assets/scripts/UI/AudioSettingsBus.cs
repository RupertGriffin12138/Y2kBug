// AudioSettingsBus.cs

using System;

namespace UI
{
    public static class AudioSettingsBus
    {
        public static event Action OnAudioSettingsChanged;

        public static void Broadcast()
        {
            OnAudioSettingsChanged?.Invoke();
        }
    }
}
