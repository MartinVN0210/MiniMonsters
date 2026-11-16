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

    private static GameManager instancia;
    private static int vidasCompartidas = -1;
    private static bool transicionMuerte;

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

    public event Action EstadoActualizado;
    public event Action GameOverIniciado;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarInstancia()
    {
        instancia = null;
        vidasCompartidas = -1;
        transicionMuerte = false;
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        if (vidasCompartidas < 0)
        {
            vidasCompartidas = vidasMaximas;
        }

        Estrellas = 0;
        EnergiaRecolectada = 0;
        NivelCompletado = false;
        GameOver = false;
        transicionMuerte = false;
        ActualizarTextoEstrellas();
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

    public void RecogerEstrella()
    {
        if (GameOver || Estrellas >= estrellasNecesarias)
        {
            return;
        }

        Estrellas++;
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
        if (GameOver || transicionMuerte)
        {
            return;
        }

        transicionMuerte = true;
        vidasCompartidas = Mathf.Max(0, vidasCompartidas - 1);
        ActualizarTextoEstrellas();

        if (vidasCompartidas == 0)
        {
            GameOver = true;
            Time.timeScale = 0f;
            GameOverIniciado?.Invoke();
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
