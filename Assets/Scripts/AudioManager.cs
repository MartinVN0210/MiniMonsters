using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor de audio persistente: guarda/carga el volumen de música y sonidos en
/// PlayerPrefs y lo aplica a todos los AudioSource de la escena actual y de las
/// escenas que se carguen después.
/// </summary>
[DefaultExecutionOrder(-200)]
public class AudioManager : MonoBehaviour
{
    public const string ClaveVolumenMusica = "VolumenMusica";
    public const string ClaveVolumenSonidos = "VolumenSonidos";

    private const string ClaveVolumenSonidosAnterior = "VolumenSFX";

    private static AudioManager instancia;

    private readonly Dictionary<AudioSource, float> volumenesBase = new Dictionary<AudioSource, float>();
    private AudioSource fuenteSFX;
    private AudioSource fuenteMusica;

    [Header("Clips de interfaz")]
    [SerializeField] private AudioClip clipClickBoton;
    [SerializeField] private AudioClip clipIniciarPartida;

    public static AudioManager Instancia
    {
        get
        {
            if (instancia == null)
            {
                instancia = FindAnyObjectByType<AudioManager>();
            }

            if (instancia == null)
            {
                GameObject objeto = new GameObject("AudioManager");
                instancia = objeto.AddComponent<AudioManager>();
            }

            return instancia;
        }
    }

    public static float VolumenMusica => Mathf.Clamp01(PlayerPrefs.GetFloat(ClaveVolumenMusica, 1f));
    public static float VolumenSonidos => Mathf.Clamp01(PlayerPrefs.GetFloat(ClaveVolumenSonidos, 1f));

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CrearAutomaticamente()
    {
        instancia = null;
        AudioManager manager = Instancia;
        manager.AplicarVolumenes();
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        DontDestroyOnLoad(gameObject);

        MigrarClaveAnterior();
        CrearFuentesInternas();
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDestroy()
    {
        if (instancia == this)
        {
            SceneManager.sceneLoaded -= AlCargarEscena;
            instancia = null;
        }
    }

    private void CrearFuentesInternas()
    {
        fuenteSFX = gameObject.AddComponent<AudioSource>();
        fuenteSFX.playOnAwake = false;
        fuenteSFX.loop = false;
        fuenteSFX.spatialBlend = 0f;
        fuenteSFX.ignoreListenerPause = true;

        fuenteMusica = gameObject.AddComponent<AudioSource>();
        fuenteMusica.playOnAwake = false;
        fuenteMusica.loop = true;
        fuenteMusica.spatialBlend = 0f;
        fuenteMusica.ignoreListenerPause = true;
    }

    private static void MigrarClaveAnterior()
    {
        if (PlayerPrefs.HasKey(ClaveVolumenSonidosAnterior) && !PlayerPrefs.HasKey(ClaveVolumenSonidos))
        {
            PlayerPrefs.SetFloat(ClaveVolumenSonidos, PlayerPrefs.GetFloat(ClaveVolumenSonidosAnterior, 1f));
            PlayerPrefs.Save();
        }
    }

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        AplicarVolumenes();
    }

    public static void EstablecerVolumenMusica(float valor)
    {
        PlayerPrefs.SetFloat(ClaveVolumenMusica, Mathf.Clamp01(valor));
        PlayerPrefs.Save();
        Instancia.AplicarVolumenes();
    }

    public static void EstablecerVolumenSonidos(float valor)
    {
        PlayerPrefs.SetFloat(ClaveVolumenSonidos, Mathf.Clamp01(valor));
        PlayerPrefs.Save();
        Instancia.AplicarVolumenes();
    }

    public void AplicarVolumenes()
    {
        float musica = VolumenMusica;
        float sonidos = VolumenSonidos;

        AudioSource[] fuentes = FindObjectsByType<AudioSource>(FindObjectsInactive.Include);
        foreach (AudioSource fuente in fuentes)
        {
            if (fuente == null || fuente == fuenteSFX || fuente == fuenteMusica)
            {
                continue;
            }

            if (!volumenesBase.ContainsKey(fuente))
            {
                volumenesBase[fuente] = fuente.volume;
            }

            fuente.volume = volumenesBase[fuente] * (EsMusica(fuente) ? musica : sonidos);
        }

        if (fuenteSFX != null)
        {
            fuenteSFX.volume = sonidos;
        }

        if (fuenteMusica != null)
        {
            fuenteMusica.volume = musica;
        }
    }

    private static bool EsMusica(AudioSource fuente)
    {
        string nombreClip = fuente.clip != null ? fuente.clip.name.ToLowerInvariant() : string.Empty;
        string nombreObjeto = fuente.gameObject.name.ToLowerInvariant();

        if (nombreClip.Contains("title") || nombreClip.Contains("music") || nombreClip.Contains("musica"))
        {
            return true;
        }

        if (nombreClip.StartsWith("level ") || nombreClip.Contains("nivel"))
        {
            return true;
        }

        return nombreObjeto.Contains("music") || nombreObjeto.Contains("musica");
    }

    public void ReproducirSFX(AudioClip clip, float volumen = 1f)
    {
        if (clip == null || fuenteSFX == null)
        {
            return;
        }

        fuenteSFX.PlayOneShot(clip, Mathf.Clamp01(volumen));
    }

    public void RegistrarClipsUI(AudioClip click, AudioClip iniciarPartida)
    {
        if (click != null)
        {
            clipClickBoton = click;
        }

        if (iniciarPartida != null)
        {
            clipIniciarPartida = iniciarPartida;
        }
    }

    public void ReproducirClickBoton()
    {
        ReproducirSFX(clipClickBoton);
    }

    public void ReproducirBotonIniciarPartida()
    {
        ReproducirSFX(clipIniciarPartida);
    }

    public float DuracionBotonIniciarPartida()
    {
        return clipIniciarPartida != null ? clipIniciarPartida.length : 0f;
    }
}
