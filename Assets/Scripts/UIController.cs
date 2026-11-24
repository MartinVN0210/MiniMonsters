using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private float reinicioGameOverSegundos = 2f;
    [SerializeField] private float mapaMinX = 0f;
    [SerializeField] private float mapaMaxX = 110f;
    [SerializeField] private float mapaMinY = -2f;
    [SerializeField] private float mapaMaxY = 8f;

    private GameManager gameManager;
    private Text textoEstrellas;
    private Text textoEnergia;
    private Text textoNivelCompletado;
    private Text textoGameOver;
    private Image[] corazones;
    private Button botonPausa;
    private GameObject panelGameOver;
    private RectTransform contenidoMinimapa;
    private RectTransform marcadorChispa;
    private RectTransform marcadorRoca;
    private Transform chispa;
    private Transform roca;
    private Coroutine reinicioGameOver;

    private void OnEnable()
    {
        if (!ResolverReferencias())
        {
            enabled = false;
            return;
        }

        gameManager.EstadoActualizado += ActualizarHUD;
        gameManager.GameOverIniciado += MostrarGameOver;
        botonPausa.onClick.AddListener(Pausar);
        ActualizarHUD();
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.EstadoActualizado -= ActualizarHUD;
            gameManager.GameOverIniciado -= MostrarGameOver;
        }

        if (botonPausa != null)
        {
            botonPausa.onClick.RemoveListener(Pausar);
        }
    }

    private void Update()
    {
        ActualizarMarcador(marcadorChispa, chispa);
        ActualizarMarcador(marcadorRoca, roca);
    }

    private bool ResolverReferencias()
    {
        gameManager = GameManager.Instancia;
        textoEstrellas = Buscar<Text>("PanelColeccionables/ContenedorContadores/TextoEstrellas");
        textoEnergia = Buscar<Text>("PanelColeccionables/ContenedorContadores/TextoEnergia");
        textoNivelCompletado = Buscar<Text>("TextoNivelCompletado");
        textoGameOver = Buscar<Text>("PanelGameOver/TextoGameOver");
        panelGameOver = BuscarObjeto("PanelGameOver");
        botonPausa = Buscar<Button>("PanelBotones/BotonPausa");
        Transform contenedorCorazones = transform.Find("PanelVida/ContenedorCorazones");
        corazones = contenedorCorazones != null
            ? contenedorCorazones.GetComponentsInChildren<Image>(false)
            : null;

        contenidoMinimapa = Buscar<RectTransform>("Minimapa/Viewport/ContenidoMapa");
        marcadorChispa = Buscar<RectTransform>("Minimapa/Viewport/ContenidoMapa/MarcadorChispa");
        marcadorRoca = Buscar<RectTransform>("Minimapa/Viewport/ContenidoMapa/MarcadorRoca");
        GameObject objetoChispa = GameObject.Find("Chispa");
        GameObject objetoRoca = GameObject.Find("Roca");
        chispa = objetoChispa != null ? objetoChispa.transform : null;
        roca = objetoRoca != null ? objetoRoca.transform : null;

        if (gameManager == null || textoEstrellas == null || textoEnergia == null ||
            textoNivelCompletado == null || textoGameOver == null || panelGameOver == null ||
            botonPausa == null || corazones == null ||
            contenidoMinimapa == null || marcadorChispa == null || marcadorRoca == null ||
            chispa == null || roca == null)
        {
            Debug.LogError("Faltan referencias necesarias para inicializar el HUD cooperativo.", this);
            return false;
        }

        return true;
    }

    private T Buscar<T>(string ruta) where T : Component
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.GetComponent<T>() : null;
    }

    private GameObject BuscarObjeto(string ruta)
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.gameObject : null;
    }

    private void ActualizarHUD()
    {
        textoEstrellas.text = "x" + gameManager.Estrellas.ToString("00") + "/" +
                              gameManager.EstrellasNecesarias.ToString("00");
        textoEnergia.text = "x" + gameManager.EnergiaRecolectada.ToString("00");

        for (int i = 0; i < corazones.Length; i++)
        {
            corazones[i].gameObject.SetActive(i < gameManager.VidasRestantes);
        }

        textoNivelCompletado.gameObject.SetActive(gameManager.NivelCompletado);
    }

    private void ActualizarMarcador(RectTransform marcador, Transform jugador)
    {
        float x = Mathf.Clamp01(Mathf.InverseLerp(mapaMinX, mapaMaxX, jugador.position.x));
        float y = Mathf.Clamp01(Mathf.InverseLerp(mapaMinY, mapaMaxY, jugador.position.y));
        Rect rect = contenidoMinimapa.rect;
        marcador.anchoredPosition = new Vector2((x - 0.5f) * rect.width, (y - 0.5f) * rect.height);
    }

    private void Pausar()
    {
        if (!gameManager.GameOver)
        {
            Time.timeScale = 0f;
        }
    }

    private void MostrarGameOver()
    {
        panelGameOver.SetActive(true);
        textoGameOver.text = "GAME OVER\nReiniciando nivel...";
        if (reinicioGameOver != null)
        {
            StopCoroutine(reinicioGameOver);
        }

        reinicioGameOver = StartCoroutine(ReiniciarDespuesDeGameOver());
    }

    private IEnumerator ReiniciarDespuesDeGameOver()
    {
        yield return new WaitForSecondsRealtime(reinicioGameOverSegundos);
        gameManager.ReiniciarNivel();
    }
}
