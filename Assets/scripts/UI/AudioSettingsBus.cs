// AudioSettingsBus.cs
using System;

public static class AudioSettingsBus
{
    public static event Action OnAudioSettingsChanged;

    public static void Broadcast()
    {
        OnAudioSettingsChanged?.Invoke();
    }
}
