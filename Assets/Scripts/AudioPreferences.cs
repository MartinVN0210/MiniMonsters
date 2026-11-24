using UnityEngine;

/// <summary>
/// Fachada estática sobre <see cref="AudioManager"/> para que los paneles de
/// ajustes (menú y pausa) compartan el mismo volumen persistente.
/// </summary>
public static class AudioPreferences
{
    public static float MusicVolume => AudioManager.VolumenMusica;
    public static float SfxVolume => AudioManager.VolumenSonidos;

    public static void SetMusicVolume(float value)
    {
        AudioManager.EstablecerVolumenMusica(value);
    }

    public static void SetSfxVolume(float value)
    {
        AudioManager.EstablecerVolumenSonidos(value);
    }

    public static void ApplyVolumes()
    {
        AudioManager.Instancia.AplicarVolumenes();
    }
}
