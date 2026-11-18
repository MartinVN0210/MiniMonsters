using UnityEngine;

public static class AudioPreferences
{
    private static readonly System.Collections.Generic.Dictionary<AudioSource, float> baseVolumes =
        new System.Collections.Generic.Dictionary<AudioSource, float>();

    public static float MusicVolume => PlayerPrefs.GetFloat("VolumenMusica", 1f);
    public static float SfxVolume => PlayerPrefs.GetFloat("VolumenSFX", 1f);

    public static void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("VolumenMusica", Mathf.Clamp01(value));
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public static void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("VolumenSFX", Mathf.Clamp01(value));
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public static void ApplyVolumes()
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>();
        foreach (AudioSource source in sources)
        {
            if (source == null)
            {
                continue;
            }

            if (!baseVolumes.ContainsKey(source))
            {
                baseVolumes.Add(source, source.volume);
            }

            string sourceName = source.gameObject.name.ToLowerInvariant();
            bool isMusic = sourceName.Contains("music") || sourceName.Contains("musica");
            float setting = isMusic ? MusicVolume : SfxVolume;
            source.volume = baseVolumes[source] * setting;
        }
    }
}
