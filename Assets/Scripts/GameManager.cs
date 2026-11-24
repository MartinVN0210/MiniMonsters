using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    [SerializeField] private int estrellasNecesarias = 10;
    [SerializeField] private Text textoEstrellas;
    [SerializeField] private int vidasMaximas = 3;

    [Header("Audio SFX")]
    [SerializeField] private AudioSource audioSourceSFX;
    [SerializeField] private AudioClip sonidoEstrella;
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoInterruptor;

    private static GameManager instancia;
    private static int vidasCompartidas = -1;
    private static bool transicionMuerte;
    private static float proximaMuertePermitida;

    private Vector2 ultimoCheckpoint;
    private bool checkpointRegistrado;
    private Checkpoint checkpointActivo;
    private Transform chispa;
    private Transform roca;

    public static GameManager Instancia
    {
        get
        {
            if (instancia == null)
            {
                instancia = FindAnyObjectByType<GameManager>();
            }

            return instancia;
        }
    }

    public int Estrellas { get; private set; }
    public int EnergiaRecolectada { get; private set; }
    public int EstrellasNecesarias => estrellasNecesarias;
    public int VidasRestantes => Mathf.Max(0, vidasCompartidas);
    public int VidasMaximas => vidasMaximas;
    public bool NivelCompletado { get; private set; }
    public bool GameOver { get; private set; }
    public Vector2 UltimoCheckpoint => ultimoCheckpoint;

    public event Action EstadoActualizado;
    public event Action GameOverIniciado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarInstancia()
    {
        instancia = null;
        vidasCompartidas = -1;
        transicionMuerte = false;
        proximaMuertePermitida = 0f;
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;

        if (audioSourceSFX == null)
        {
            audioSourceSFX = GetComponent<AudioSource>();
        }

        vidasCompartidas = vidasMaximas;

        Estrellas = 0;
        EnergiaRecolectada = 0;
        NivelCompletado = false;
        GameOver = false;
        transicionMuerte = false;
        proximaMuertePermitida = 0f;
        Time.timeScale = 1f;
        ActualizarTextoEstrellas();
    }

    private void Start()
    {
        GameObject chispaObjeto = GameObject.Find("Chispa");
        GameObject rocaObjeto = GameObject.Find("Roca");
        chispa = chispaObjeto != null ? chispaObjeto.transform : null;
        roca = rocaObjeto != null ? rocaObjeto.transform : null;
        if (chispa != null && roca != null)
        {
            ultimoCheckpoint = ((Vector2)chispa.position + (Vector2)roca.position) * 0.5f;
        }
    }

    private void OnEnable()
    {
        if (instancia == null)
        {
            instancia = this;
        }
    }

    private void OnDestroy()
    {
        if (instancia == this)
        {
            instancia = null;
        }
    }

    public void ReproducirSFX(AudioClip clip, float volumen = 1f)
    {
        AudioManager.Instancia.ReproducirSFX(clip, volumen);
    }

    public void ReproducirSonidoSalto(AudioClip clipPersonalizado = null)
    {
        ReproducirSFX(clipPersonalizado != null ? clipPersonalizado : sonidoSalto);
    }

    public void ReproducirSonidoInterruptor(AudioClip clipPersonalizado = null)
    {
        ReproducirSFX(clipPersonalizado != null ? clipPersonalizado : sonidoInterruptor);
    }

    public void RecogerEstrella()
    {
        if (GameOver || Estrellas >= estrellasNecesarias)
        {
            return;
        }

        Estrellas++;
        ReproducirSFX(sonidoEstrella);
        ActualizarTextoEstrellas();
    }

    public void CompletarNivel()
    {
        if (NivelCompletado || GameOver)
        {
            return;
        }

        EnergiaRecolectada++;
        NivelCompletado = true;
        ActualizarTextoEstrellas();
    }

    public void PerderVida()
    {
        if (GameOver || transicionMuerte || Time.unscaledTime < proximaMuertePermitida)
        {
            return;
        }

        transicionMuerte = true;
        proximaMuertePermitida = Time.unscaledTime + 0.75f;
        vidasCompartidas = Mathf.Max(0, vidasCompartidas - 1);
        ActualizarTextoEstrellas();

        if (vidasCompartidas == 0)
        {
            GameOver = true;
            Time.timeScale = 0f;
            GameOverIniciado?.Invoke();
            return;
        }

        ReaparecerEnCheckpoint();
        transicionMuerte = false;
    }

    public void RegistrarCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null)
        {
            Debug.LogError("No se puede registrar un checkpoint nulo.", this);
            return;
        }

        if (checkpointActivo != null && checkpointActivo != checkpoint)
        {
            checkpointActivo.Desactivar();
        }

        checkpointActivo = checkpoint;
        ultimoCheckpoint = checkpoint.transform.position;
        checkpointRegistrado = true;
        EstadoActualizado?.Invoke();
    }

    public void ReaparecerEnCheckpoint()
    {
        if (chispa == null || roca == null)
        {
            GameObject chispaObjeto = GameObject.Find("Chispa");
            GameObject rocaObjeto = GameObject.Find("Roca");
            chispa = chispaObjeto != null ? chispaObjeto.transform : null;
            roca = rocaObjeto != null ? rocaObjeto.transform : null;
        }

        if (chispa == null || roca == null)
        {
            Debug.LogError("No se pudieron encontrar Chispa y Roca para reaparecer.", this);
            return;
        }

        if (!checkpointRegistrado && ultimoCheckpoint == Vector2.zero)
        {
            ultimoCheckpoint = ((Vector2)chispa.position + (Vector2)roca.position) * 0.5f;
        }

        ReubicarJugador(chispa, ultimoCheckpoint + Vector2.left * 0.5f);
        ReubicarJugador(roca, ultimoCheckpoint + Vector2.right * 0.5f);
    }

    private void ReubicarJugador(Transform jugador, Vector2 posicion)
    {
        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = posicion;
        }
        else
        {
            jugador.position = posicion;
        }

        MovimientoJugador movimiento = jugador.GetComponent<MovimientoJugador>();
        if (movimiento != null)
        {
            movimiento.ConfigurarControlHumano(
                movimiento.ControlHumano,
                movimiento.ControlHumano && PlayerPrefs.GetInt("ModoJugadores", 2) == 1);
        }
    }

    public void RecuperarVida()
    {
        if (GameOver || vidasCompartidas >= vidasMaximas)
        {
            return;
        }

        vidasCompartidas++;
        ActualizarTextoEstrellas();
    }

    public void ReiniciarNivel()
    {
        vidasCompartidas = vidasMaximas;
        transicionMuerte = false;
        proximaMuertePermitida = 0f;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ActualizarTextoEstrellas()
    {
        if (textoEstrellas != null)
        {
            textoEstrellas.text = "★ x" + Estrellas.ToString("00") + "/" + estrellasNecesarias.ToString("00");
        }
        
        EstadoActualizado?.Invoke();
    }
}