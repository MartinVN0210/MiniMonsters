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

        panelAjustes = CrearPanel(canvas.transform, "Panel_Ajustes", panelMenu, new Vector2(1150f, 800f));
        CrearAjustes(panelAjustes.transform, true);

        panelAcercaDe = CrearPanel(canvas.transform, "Panel_AcercaDe", panelAcerca != null ? panelAcerca : panelMenu,
            new Vector2(1150f, 800f));
        CrearAcercaDe(panelAcercaDe.transform);

        panelModo.SetActive(false);
        panelSeleccionNivel.SetActive(false);
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
        MenuUIFactory.CreateText(parent, "Subtitulo", "UN JUGADOR - Selecciona tu personaje", 20, Color.white,
            new Vector2(1050f, 32f), new Vector2(0f, 300f));
        CrearTarjeta(parent, "TarjetaChispa", "CHISPA\n(Roca = IA)", chispaIdle,
            new Vector2(-310f, 120f), () => IniciarUnJugador(0));
        CrearTarjeta(parent, "TarjetaRoca", "ROCA\n(Chispa = IA)", rocaIdle,
            new Vector2(310f, 120f), () => IniciarUnJugador(1));
        MenuUIFactory.CreateButton(parent, "DosJugadores", "DOS JUGADORES (Cooperativo Local)",
            botonRosa, new Vector2(450f, 86f), new Vector2(0f, -205f), IniciarCooperativo, 25);
        MenuUIFactory.CreateButton(parent, "Regresar", "REGRESAR", botonRojo,
            new Vector2(340f, 90f), new Vector2(0f, -335f), () => Mostrar(panelPrincipal), 32);
    }

    private void CrearSeleccionNivel(Transform parent)
    {
        CrearTextoTMP(parent, "Titulo", "SELECCIONA UN NIVEL", 60,
            new Vector2(1250f, 110f), new Vector2(0f, 375f), 0.34f, new Vector4(0f, 0f, 0f, 0f));

        CrearBotonTMP(parent, "Btn_Nivel1", "NIVEL 1: EL PARQUE (Fácil)", botonVerde,
            new Vector2(930f, 125f), new Vector2(0f, 170f), 34, CargarNivel1);
        CrearBotonTMP(parent, "Btn_Nivel2", "NIVEL 2: LA FÁBRICA ABANDONADA (Medio)", botonAzul,
            new Vector2(930f, 125f), new Vector2(0f, 15f), 34, CargarNivel2);
        CrearBotonTMP(parent, "Btn_Nivel3", "NIVEL 3: LA GRAN AVERÍA (Difícil)", botonMorado,
            new Vector2(930f, 125f), new Vector2(0f, -140f), 34, CargarNivel3);
        CrearBotonTMP(parent, "Btn_Regresar", "REGRESAR", botonRojo,
            new Vector2(480f, 110f), new Vector2(0f, -370f), 34,
            () => Mostrar(panelModo));
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
        text.textWrappingMode = TextWrappingModes.Normal;
        text.margin = margin;
        if (tmpFont != null && tmpFont.material != null)
        {
            text.outlineWidth = outlineThickness;
            text.outlineColor = Color.black;
        }
        else
        {
            Text fallback = MenuUIFactory.CreateText(rect, "LegacyFallback", value,
                Mathf.RoundToInt(fontSize), text.color, size, Vector2.zero);
            fallback.fontStyle = FontStyle.Bold;
            fallback.alignment = TextAnchor.MiddleCenter;
            Outline outline = fallback.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(1f, -2f);
            }
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
        CrearTextoTMP(image.transform, "Label", label, fontSize,
            size - new Vector2(32f, 12f), Vector2.zero, 0.3f,
            new Vector4(24f, 0f, 92f, 0f)).color = Color.white;
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
        panelAjustes.SetActive(panel == panelAjustes);
        panelAcercaDe.SetActive(panel == panelAcercaDe);

        if (EventSystem.current != null && panel != null)
        {
            Selectable selectable = panel.GetComponentInChildren<Selectable>(true);
            EventSystem.current.SetSelectedGameObject(selectable == null ? panel : selectable.gameObject);
        }
    }

    private void IniciarUnJugador(int heroe)
    {
        PlayerPrefs.SetInt("ModoJugadores", 1);
        PlayerPrefs.SetInt("HeroeElegido", heroe);
        Mostrar(panelSeleccionNivel);
    }

    private void IniciarCooperativo()
    {
        PlayerPrefs.SetInt("ModoJugadores", 2);
        Mostrar(panelSeleccionNivel);
    }

    private void CargarNivel1()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("Nivel1_Parque");
    }

    private void CargarNivel2()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("Nivel2_Fabrica");
    }

    private void CargarNivel3()
    {
        PlayerPrefs.Save();
        SceneManager.LoadScene("Nivel3_Averia");
    }

    private void Salir()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}