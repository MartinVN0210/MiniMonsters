using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    public Sprite fondoCielo;
    public Sprite panelMenu;
    public Sprite panelAcerca;
    public Sprite botonAzul;
    public Sprite botonMorado;
    public Sprite botonRojo;
    public Sprite botonRosa;
    public Sprite botonVerde;
    public Sprite tarjetaSeleccion;
    public Sprite logoGame;
    public Sprite logoCompany;
    public Sprite chispaIdle;
    public Sprite rocaIdle;
    public Sprite sliderFondo;
    public Sprite sliderRelleno;
    public Sprite sliderPerilla;

    [Header("Audio")]
    [SerializeField] private AudioSource musicaAudioSource;
    [SerializeField] private AudioSource[] sfxAudioSources;

    private GameObject panelPrincipal;
    private GameObject panelModo;
    [SerializeField] private GameObject panelSeleccionNivel;
    private GameObject panelInstrucciones;
    private TMP_Text tituloInstrucciones;
    private TMP_Text controlesInstrucciones;
    private string escenaSeleccionada;
    private GameObject panelAjustes;
    private GameObject panelAcercaDe;
    private TMP_FontAsset tmpFont;

    private void Awake()
    {
        Time.timeScale = 1f;
        CrearInterfaz();
    }

    private void Start()
    {
        Mostrar(panelPrincipal);
        AplicarVolumenEnEscena(AudioPreferences.MusicVolume, AudioPreferences.SfxVolume);
    }

    private void CrearInterfaz()
    {
        Canvas canvas = CrearCanvas();
        if (fondoCielo != null)
        {
            Image fondo = MenuUIFactory.CreateImage(canvas.transform, "FondoCielo", fondoCielo,
                Vector2.zero, Vector2.zero);
            RectTransform backgroundRect = fondo.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            fondo.raycastTarget = false;
        }

        panelPrincipal = CrearPanel(canvas.transform, "Panel_MenuPrincipal", panelMenu, new Vector2(1500f, 900f));
        CrearContenidoPrincipal(panelPrincipal.transform);

        panelModo = CrearPanel(canvas.transform, "Panel_SeleccionModo", panelMenu, new Vector2(1500f, 900f));
        CrearSeleccionModo(panelModo.transform);

        panelSeleccionNivel = CrearPanel(canvas.transform, "Panel_SeleccionNivel", panelMenu, new Vector2(1500f, 900f));
        CrearSeleccionNivel(panelSeleccionNivel.transform);

        panelInstrucciones = CrearPanel(canvas.transform, "Panel_Instrucciones", panelMenu, new Vector2(1500f, 900f));
        CrearPanelInstrucciones(panelInstrucciones.transform);

        panelAjustes = CrearPanel(canvas.transform, "Panel_Ajustes", panelMenu, new Vector2(1150f, 800f));
        CrearAjustes(panelAjustes.transform, true);

        panelAcercaDe = CrearPanel(canvas.transform, "Panel_AcercaDe", panelAcerca != null ? panelAcerca : panelMenu,
            new Vector2(1150f, 800f));
        CrearAcercaDe(panelAcercaDe.transform);

        panelModo.SetActive(false);
        panelSeleccionNivel.SetActive(false);
        panelInstrucciones.SetActive(false);
        panelAjustes.SetActive(false);
        panelAcercaDe.SetActive(false);

        AudioPreferences.ApplyVolumes();
        AplicarVolumenEnEscena(AudioPreferences.MusicVolume, AudioPreferences.SfxVolume);
    }

    private Canvas CrearCanvas()
    {
        GameObject root = new GameObject("MainMenuCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        return canvas;
    }

    private GameObject CrearPanel(Transform parent, string name, Sprite sprite, Vector2 size)
    {
        Image panel = MenuUIFactory.CreateImage(parent, name, sprite, size, Vector2.zero, true);
        panel.raycastTarget = false;
        return panel.gameObject;
    }

    private void CrearContenidoPrincipal(Transform parent)
    {
        if (logoGame != null)
        {
            Image logo = MenuUIFactory.CreateImage(parent, "LogoGame", logoGame,
                new Vector2(700f, 370f), new Vector2(-350f, 150f));
            logo.preserveAspect = true;
            logo.raycastTarget = false;
        }
        else
        {
            MenuUIFactory.CreateText(parent, "LogoGameFallback", "MINI\nMONSTERS", 64, Color.white,
                new Vector2(650f, 300f), new Vector2(-350f, 150f));
        }

        if (logoCompany != null)
        {
            Image company = MenuUIFactory.CreateImage(parent, "LogoCompany", logoCompany,
                new Vector2(360f, 135f), new Vector2(-350f, -285f));
            company.preserveAspect = true;
            company.raycastTarget = false;
        }
        else
        {
            MenuUIFactory.CreateText(parent, "LogoCompanyFallback", "MINI MONSTERS STUDIO", 22, Color.white,
                new Vector2(380f, 70f), new Vector2(-350f, -300f));
        }

        MenuUIFactory.CreateButton(parent, "Jugar", "JUGAR", botonVerde,
            new Vector2(350f, 86f), new Vector2(330f, 250f), () => Mostrar(panelModo), 32);
        MenuUIFactory.CreateButton(parent, "Ajustes", "AJUSTES", botonAzul,
            new Vector2(350f, 86f), new Vector2(330f, 105f), () => Mostrar(panelAjustes), 32);
        MenuUIFactory.CreateButton(parent, "AcercaDe", "ACERCA DE", botonMorado,
            new Vector2(350f, 86f), new Vector2(330f, -40f), () => Mostrar(panelAcercaDe), 30);
        MenuUIFactory.CreateButton(parent, "Salir", "SALIR", botonRojo,
            new Vector2(350f, 86f), new Vector2(330f, -185f), Salir, 32);
    }

    private void CrearSeleccionModo(Transform parent)
    {
        MenuUIFactory.CreateText(parent, "Titulo", "ELIGE CÓMO JUGAR", 46, Color.white,
            new Vector2(1000f, 90f), new Vector2(0f, 340f));
        CrearBotonTMP(parent, "Btn_DosJugadores", "DOS JUGADORES (Local) - Recomendado",
            botonRosa, new Vector2(720f, 110f), new Vector2(0f, 90f), 25, IniciarCooperativo);
        CrearBotonTMP(parent, "Btn_UnJugador", "UN JUGADOR",
            botonVerde, new Vector2(720f, 110f), new Vector2(0f, -45f), 25, IniciarUnJugador);
        CrearBotonTMP(parent, "Btn_Regresar", "REGRESAR", botonRojo,
            new Vector2(340f, 90f), new Vector2(0f, -335f), 32, () => Mostrar(panelPrincipal));
    }

    private void CrearSeleccionNivel(Transform parent)
    {
        CrearTextoTMP(parent, "Titulo", "SELECCIONA UN NIVEL", 60,
            new Vector2(1250f, 110f), new Vector2(0f, 375f), 0.34f, new Vector4(0f, 0f, 0f, 0f));

        CrearBotonTMP(parent, "Btn_Nivel1", "NIVEL 1: EL PARQUE (Fácil)", botonVerde,
            new Vector2(930f, 125f), new Vector2(0f, 170f), 34, () => SeleccionarNivel("Nivel1_Parque"));
        CrearBotonTMP(parent, "Btn_Nivel2", "NIVEL 2: LA FÁBRICA ABANDONADA (Medio)", botonAzul,
            new Vector2(930f, 125f), new Vector2(0f, 15f), 34, () => SeleccionarNivel("Nivel2_Fabrica"));
        CrearBotonTMP(parent, "Btn_Nivel3", "NIVEL 3: LA GRAN AVERÍA (Difícil)", botonMorado,
            new Vector2(930f, 125f), new Vector2(0f, -140f), 34, () => SeleccionarNivel("Nivel3_Averia"));
        CrearBotonTMP(parent, "Btn_Regresar", "REGRESAR", botonRojo,
            new Vector2(480f, 110f), new Vector2(0f, -370f), 34,
            () => Mostrar(panelModo));
    }

    private void CrearPanelInstrucciones(Transform parent)
    {
        tituloInstrucciones = (TMP_Text)CrearTextoTMP(parent, "Titulo", "",
            40f, new Vector2(1300f, 90f), new Vector2(0f, 360f), 0.38f,
            new Vector4(0f, 0f, 0f, 0f));

        CrearTarjetaInstrucciones(parent, "TarjetaChispa", "CHISPA",
            "ACTIVA BOTONES Y PUENTES", chispaIdle, new Vector2(-405f, 125f));
        CrearTarjetaInstrucciones(parent, "TarjetaRoca", "ROCA",
            "EMPUJA Y MUEVE CAJAS", rocaIdle, new Vector2(405f, 125f));

        Image banner = MenuUIFactory.CreateImage(parent, "BannerControles", tarjetaSeleccion,
            new Vector2(1260f, 190f), new Vector2(0f, -125f), true);
        controlesInstrucciones = (TMP_Text)CrearTextoTMP(banner.transform, "Controles",
            "", 28f, new Vector2(1180f, 150f), Vector2.zero, 0.22f,
            new Vector4(16f, 0f, 16f, 0f));

        CrearBotonTMP(parent, "Btn_IniciarPartida", "¡INICIAR PARTIDA!", botonVerde,
            new Vector2(460f, 92f), new Vector2(280f, -355f), 30f, IniciarEscenaSeleccionada);
        CrearBotonTMP(parent, "Btn_RegresarInstrucciones", "REGRESAR", botonRojo,
            new Vector2(280f, 85f), new Vector2(-300f, -355f), 30f,
            () => Mostrar(panelSeleccionNivel));
    }

    private void CrearTarjetaInstrucciones(Transform parent, string name, string titulo,
        string descripcion, Sprite personaje, Vector2 position)
    {
        Image card = MenuUIFactory.CreateImage(parent, name, tarjetaSeleccion,
            new Vector2(650f, 250f), position, true);

        if (personaje != null)
        {
            Image character = MenuUIFactory.CreateImage(card.transform, "Personaje", personaje,
                new Vector2(150f, 150f), new Vector2(0f, 42f));
            character.preserveAspect = true;
            character.raycastTarget = false;
        }

        CrearTextoTMP(card.transform, "Titulo", titulo, 34f,
            new Vector2(600f, 55f), new Vector2(0f, -48f), 0.28f,
            new Vector4(10f, 0f, 10f, 0f));
        CrearTextoTMP(card.transform, "Descripcion", descripcion, 23f,
            new Vector2(620f, 50f), new Vector2(0f, -92f), 0.18f,
            new Vector4(8f, 0f, 8f, 0f));
    }

    private TMP_Text CrearTextoTMP(Transform parent, string name, string value, float fontSize,
        Vector2 size, Vector2 position, float outlineThickness, Vector4 margin)
    {
        RectTransform rect = MenuUIFactory.CreateRect(parent, name, size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = tmpFont;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(1f, 0.992f, 0.941f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.margin = margin;
        if (tmpFont != null && tmpFont.material != null)
        {
            text.outlineWidth = outlineThickness;
            text.outlineColor = Color.black;
        }

        return text;
    }

    private Button CrearBotonTMP(Transform parent, string name, string label, Sprite sprite,
        Vector2 size, Vector2 position, float fontSize, UnityEngine.Events.UnityAction action)
    {
        Image image = MenuUIFactory.CreateImage(parent, name, sprite, size, position, true);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        TMP_Text text = CrearTextoTMP(image.transform, "Label", label, fontSize,
            size - new Vector2(32f, 12f), Vector2.zero, 0.3f,
            new Vector4(24f, 0f, 95f, 0f));
        text.color = Color.white;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        return button;
    }

    private void CrearTarjeta(Transform parent, string name, string label, Sprite character,
        Vector2 position, UnityEngine.Events.UnityAction action)
    {
        Image card = MenuUIFactory.CreateImage(parent, name, tarjetaSeleccion,
            new Vector2(440f, 330f), position, true);
        Button button = card.gameObject.AddComponent<Button>();
        button.targetGraphic = card;
        button.onClick.AddListener(action);
        if (character != null)
        {
            Image characterImage = MenuUIFactory.CreateImage(card.transform, "Personaje", character,
                new Vector2(150f, 150f), new Vector2(0f, 88f));
            characterImage.preserveAspect = true;
        }

        Text description = MenuUIFactory.CreateText(card.transform, "Descripcion", label, 18, Color.white,
            new Vector2(380f, 64f), new Vector2(0f, -58f));
        description.alignment = TextAnchor.MiddleCenter;
    }

    private void CrearAjustes(Transform parent, bool showBack)
    {
        MenuUIFactory.CreateText(parent, "Titulo", "AJUSTES", 48, Color.white,
            new Vector2(800f, 90f), new Vector2(0f, 270f));

        float initialMusic = Mathf.Clamp01(AudioPreferences.MusicVolume);
        float initialSfx = Mathf.Clamp01(AudioPreferences.SfxVolume);

        MenuUIFactory.CreateSlider(parent, "SliderMusica", "MÚSICA", sliderFondo, sliderRelleno,
            sliderPerilla, initialMusic, OnMusicSliderChanged,
            new Vector2(0f, 95f));

        MenuUIFactory.CreateSlider(parent, "SliderSonidos", "SONIDO", sliderFondo, sliderRelleno,
            sliderPerilla, initialSfx, OnSfxSliderChanged,
            new Vector2(0f, -65f));

        AplicarVolumenEnEscena(initialMusic, initialSfx);

        if (showBack)
        {
            MenuUIFactory.CreateButton(parent, "Regresar", "REGRESAR", botonRojo,
                new Vector2(340f, 90f), new Vector2(0f, -270f), () => Mostrar(panelPrincipal), 32);
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        float clamped = Mathf.Clamp01(value);
        AudioPreferences.SetMusicVolume(clamped);
        AudioPreferences.ApplyVolumes();
        PlayerPrefs.Save();
        AplicarVolumenEnEscena(clamped, AudioPreferences.SfxVolume);
    }

    private void OnSfxSliderChanged(float value)
    {
        float clamped = Mathf.Clamp01(value);
        AudioPreferences.SetSfxVolume(clamped);
        AudioPreferences.ApplyVolumes();
        PlayerPrefs.Save();
        AplicarVolumenEnEscena(AudioPreferences.MusicVolume, clamped);
    }

    private void AplicarVolumenEnEscena(float musicVol, float sfxVol)
    {
        float clampedMusic = Mathf.Clamp01(musicVol);
        float clampedSfx = Mathf.Clamp01(sfxVol);

        if (musicaAudioSource != null)
        {
            musicaAudioSource.volume = clampedMusic;
        }

        if (sfxAudioSources != null)
        {
            for (int i = 0; i < sfxAudioSources.Length; i++)
            {
                if (sfxAudioSources[i] != null)
                {
                    sfxAudioSources[i].volume = clampedSfx;
                }
            }
        }
    }

    private void CrearAcercaDe(Transform parent)
    {
        if (logoCompany != null)
        {
            Image logo = MenuUIFactory.CreateImage(parent, "LogoCompany", logoCompany,
                new Vector2(420f, 170f), new Vector2(0f, 225f));
            logo.preserveAspect = true;
            logo.raycastTarget = false;
        }
        else
        {
            MenuUIFactory.CreateText(parent, "LogoCompanyFallback", "MINI MONSTERS STUDIO", 28, Color.white,
                new Vector2(700f, 100f), new Vector2(0f, 220f));
        }

        Text descripcion = MenuUIFactory.CreateText(parent, "Descripcion",
            "Somos una desarrolladora que genera experiencias únicas de juego cooperativo y aventuras llenas de creatividad.",
            26, Color.white, new Vector2(650f, 230f), new Vector2(0f, -15f));
        descripcion.lineSpacing = 1.1f;
        descripcion.alignment = TextAnchor.MiddleCenter;
        MenuUIFactory.CreateButton(parent, "Regresar", "REGRESAR", botonRojo,
            new Vector2(340f, 90f), new Vector2(0f, -270f), () => Mostrar(panelPrincipal), 32);
    }

    private void Mostrar(GameObject panel)
    {
        panelPrincipal.SetActive(panel == panelPrincipal);
        panelModo.SetActive(panel == panelModo);
        panelSeleccionNivel.SetActive(panel == panelSeleccionNivel);
        panelInstrucciones.SetActive(panel == panelInstrucciones);
        panelAjustes.SetActive(panel == panelAjustes);
        panelAcercaDe.SetActive(panel == panelAcercaDe);

        if (EventSystem.current != null && panel != null)
        {
            Selectable selectable = panel.GetComponentInChildren<Selectable>(true);
            EventSystem.current.SetSelectedGameObject(selectable == null ? panel : selectable.gameObject);
        }
    }

    private void IniciarUnJugador()
    {
        PlayerPrefs.SetInt("ModoJugadores", 1);
        PlayerPrefs.SetString("HeroeElegido", "Chispa");
        Mostrar(panelSeleccionNivel);
    }

    private void IniciarCooperativo()
    {
        PlayerPrefs.SetInt("ModoJugadores", 2);
        Mostrar(panelSeleccionNivel);
    }

    private void SeleccionarNivel(string nombreEscena)
    {
        escenaSeleccionada = nombreEscena;
        panelSeleccionNivel.SetActive(false);
        ActualizarInstrucciones();
        Mostrar(panelInstrucciones);
    }

    private void ActualizarInstrucciones()
    {
        int modo = PlayerPrefs.GetInt("ModoJugadores", 2);
        if (tituloInstrucciones != null)
        {
            tituloInstrucciones.text = modo == 1
                ? "CONTROLES (MODO 1 JUGADOR)"
                : "CONTROLES (MODO 2 JUGADORES)";
        }

        if (controlesInstrucciones != null)
        {
            controlesInstrucciones.text = modo == 1
                ? "[W A S D] MOVER Y SALTAR  |  [SHIFT] INTERACTUAR  |  [ALT] CAMBIAR PERSONAJE"
                : "CHISPA: [W A S D] + [SHIFT]   |   ROCA: [FLECHAS] + [ENTER / R-SHIFT]";
        }
    }

    private void IniciarEscenaSeleccionada()
    {
        if (string.IsNullOrEmpty(escenaSeleccionada))
        {
            return;
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene(escenaSeleccionada);
    }

    private void Salir()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}