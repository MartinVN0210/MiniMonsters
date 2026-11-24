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
    [SerializeField] private AudioClip clickBotonClip;
    [SerializeField] private AudioClip iniciarPartidaClip;

    private GameObject panelPrincipal;
    private GameObject panelModo;
    [SerializeField] private GameObject panelSeleccionNivel;
    private GameObject panelInstrucciones;
    private TMP_Text tituloInstrucciones;
    private GameObject bannerControlesUnificados;
    private GameObject tarjetaChispa;
    private GameObject tarjetaRoca;
    private TMP_Text descripcionChispa;
    private TMP_Text descripcionRoca;
    private TMP_Text tituloTarjetaChispa;
    private TMP_Text tituloTarjetaRoca;
    private GameObject controlesChispa;
    private GameObject controlesRoca;
    private string escenaSeleccionada;
    private GameObject panelAjustes;
    private GameObject panelAcercaDe;
    private TMP_FontAsset tmpFont;

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioManager.Instancia.RegistrarClipsUI(clickBotonClip, iniciarPartidaClip);
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
        RegistrarSonidosBotones(canvas.transform);
    }

    private void RegistrarSonidosBotones(Transform raiz)
    {
        Button[] botones = raiz.GetComponentsInChildren<Button>(true);
        foreach (Button boton in botones)
        {
            if (boton == null || boton.name == "Btn_IniciarPartida")
            {
                continue;
            }

            boton.onClick.AddListener(AudioManager.Instancia.ReproducirClickBoton);
        }
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
        CrearBotonTMP(parent, "Btn_SalirJuego", "SALIR DEL JUEGO", botonRojo,
            new Vector2(350f, 86f), new Vector2(330f, -185f), 28f, SalirDelJuego, 85f, 24f);
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
            44f, new Vector2(1300f, 90f), new Vector2(0f, 360f), 0.32f,
            new Vector4(0f, 0f, 0f, 0f));
        tituloInstrucciones.color = new Color(1f, 253f / 255f, 240f / 255f, 1f);
        tituloInstrucciones.fontStyle = FontStyles.Bold;
        tituloInstrucciones.outlineWidth = 0.32f;
        tituloInstrucciones.outlineColor = Color.black;
        AplicarSombraTMP(tituloInstrucciones);

        CrearTarjetaInstrucciones(parent, "Tarjeta_Chispa", "JUGADOR 1: CHISPA",
            "ACTIVA BOTONES Y PUENTES", chispaIdle, new Vector2(-405f, 125f),
            out tarjetaChispa, out tituloTarjetaChispa, out descripcionChispa, out controlesChispa);
        CrearTarjetaInstrucciones(parent, "Tarjeta_Roca", "JUGADOR 2: ROCA",
            "EMPUJA Y MUEVE CAJAS", rocaIdle, new Vector2(405f, 125f),
            out tarjetaRoca, out tituloTarjetaRoca, out descripcionRoca, out controlesRoca);

        CrearControlesUnificados(parent);

        CrearBotonTMP(parent, "Btn_IniciarPartida", "¡INICIAR PARTIDA!", botonVerde,
            new Vector2(460f, 92f), new Vector2(210f, -360f), 30f, IniciarEscenaSeleccionada, 95f, 30f);
        CrearBotonTMP(parent, "Btn_RegresarInstrucciones", "REGRESAR", botonRojo,
            new Vector2(340f, 92f), new Vector2(-250f, -360f), 30f,
            () => Mostrar(panelSeleccionNivel), 85f, 30f);
    }

    private void CrearTarjetaInstrucciones(Transform parent, string name, string titulo,
        string descripcion, Sprite personaje, Vector2 position,
        out GameObject cardObject, out TMP_Text titleText, out TMP_Text descriptionText,
        out GameObject controlsObject)
    {
        Image card = MenuUIFactory.CreateImage(parent, name, tarjetaSeleccion,
            new Vector2(470f, 295f), position, true);
        cardObject = card.gameObject;

        if (personaje != null)
        {
            Image character = MenuUIFactory.CreateImage(card.transform, "Personaje", personaje,
                new Vector2(150f, 115f), new Vector2(0f, 78f));
            character.preserveAspect = true;
            character.raycastTarget = false;
        }

        titleText = (TMP_Text)CrearTextoTMP(card.transform, "Titulo", titulo, 30f,
            new Vector2(430f, 48f), new Vector2(0f, -12f), 0f,
            new Vector4(0f, 0f, 0f, 0f));
        titleText.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        titleText.fontStyle = FontStyles.Bold;
        titleText.outlineWidth = 0f;

        descriptionText = (TMP_Text)CrearTextoTMP(card.transform, "Descripcion", descripcion, 22f,
            new Vector2(420f, 55f), new Vector2(0f, -62f), 0f,
            new Vector4(0f, 0f, 0f, 0f));
        descriptionText.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        descriptionText.fontStyle = FontStyles.Bold;
        descriptionText.outlineWidth = 0f;

        controlsObject = new GameObject("ControlesDosJugadores", typeof(RectTransform));
        controlsObject.transform.SetParent(card.transform, false);
        RectTransform controlsRect = controlsObject.GetComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0.5f, 0.5f);
        controlsRect.anchorMax = new Vector2(0.5f, 0.5f);
        controlsRect.pivot = new Vector2(0.5f, 0.5f);
        controlsRect.sizeDelta = new Vector2(430f, 145f);
        controlsRect.anchoredPosition = new Vector2(0f, -58f);
    }

    private void CrearControlesUnificados(Transform parent)
    {
        Image banner = MenuUIFactory.CreateImage(parent, "Banner_ControlesUnificados", tarjetaSeleccion,
            new Vector2(980f, 145f), new Vector2(0f, -125f), true);
        bannerControlesUnificados = banner.gameObject;

        CrearGrupoControles(banner.transform, "Movimiento",
            new string[] { "W", "A", "S", "D" },
            "MOVER Y\nSALTAR", new Vector2(-395f, 0f), 150f,
            Vector2.zero, new Vector2(120f, 0f), new Vector2(140f, 70f),
            true);
        CrearGrupoControles(banner.transform, "Interactuar",
            new string[] { "SHIFT" },
            "INTERACTUAR", new Vector2(-115f, 0f), 92f,
            Vector2.zero, new Vector2(140f, 0f), new Vector2(175f, 70f),
            false);
        CrearGrupoControles(banner.transform, "Cambiar",
            new string[] { "ALT" },
            "CAMBIAR\nPERSONAJE", new Vector2(165f, 0f), 92f,
            Vector2.zero, new Vector2(125f, 0f), new Vector2(155f, 70f),
            false);
    }

    private void CrearGrupoControles(Transform parent, string name, string[] teclas,
        string descripcion, Vector2 position, float teclasWidth,
        Vector2 keysPosition, Vector2 descriptionPosition, Vector2 descriptionSize,
        bool wasd)
    {
        RectTransform group = MenuUIFactory.CreateRect(parent, name,
            new Vector2(300f, 125f), position);

        RectTransform keysRoot = MenuUIFactory.CreateRect(group, "Teclas",
            new Vector2(teclas.Length == 4 ? 150f : teclasWidth, teclas.Length == 4 ? 92f : 44f),
            keysPosition);

        if (wasd)
        {
            keysRoot.localScale = new Vector3(0.85f, 0.85f, 1f);
        }

        if (teclas.Length == 4)
        {
            EstilizarTecla(keysRoot, "W", "W", 48f, new Vector2(0f, 22f));
            EstilizarTecla(keysRoot, "A", "A", 48f, new Vector2(-52f, -26f));
            EstilizarTecla(keysRoot, "S", "S", 48f, new Vector2(0f, -26f));
            EstilizarTecla(keysRoot, "D", "D", 48f, new Vector2(52f, -26f));
        }
        else
        {
            EstilizarTecla(keysRoot, "Tecla", teclas[0], teclasWidth, Vector2.zero, 44f);
        }

        TMP_Text descripcionTMP = CrearTextoTMP(group, "Descripcion", descripcion,
            20f, descriptionSize, descriptionPosition,
            0f, new Vector4(0f, 0f, 0f, 0f));
        descripcionTMP.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        descripcionTMP.fontStyle = FontStyles.Bold;
        descripcionTMP.outlineWidth = 0f;
        descripcionTMP.alignment = TextAlignmentOptions.Left;
        descripcionTMP.textWrappingMode = TextWrappingModes.Normal;
    }

    private void EstilizarTecla(Transform parent, string name, string label,
        float width, Vector2 position, float height = 48f)
    {
        RectTransform keyRect = MenuUIFactory.CreateRect(parent, name,
            new Vector2(width, height), position);
        Image keyImage = keyRect.gameObject.AddComponent<Image>();
        keyImage.color = new Color(216f / 255f, 195f / 255f, 158f / 255f, 1f);

        Outline outline = keyRect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(92f / 255f, 58f / 255f, 33f / 255f, 1f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = keyRect.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(59f / 255f, 35f / 255f, 20f / 255f, 0.9f);
        shadow.effectDistance = new Vector2(0f, -3f);

        TMP_Text keyText = (TMP_Text)CrearTextoTMP(keyRect, "Texto", label,
            label.Length > 5 ? 16f : 22f, keyRect.sizeDelta, Vector2.zero, 0f,
            new Vector4(0f, 0f, 0f, 0f));
        keyText.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        keyText.fontStyle = FontStyles.Bold;
        keyText.outlineWidth = 0f;
    }

    private void AplicarSombraTMP(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        Material material = text.fontMaterial;
        if (material != null)
        {
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor", Color.black);
            material.SetFloat("_UnderlayOffsetX", 0.35f);
            material.SetFloat("_UnderlayOffsetY", -0.35f);
            material.SetFloat("_UnderlayDilate", 0.45f);
            material.SetFloat("_UnderlaySoftness", 0f);
        }

        Shadow shadow = text.gameObject.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = text.gameObject.AddComponent<Shadow>();
        }
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(3f, -4f);
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
        Vector2 size, Vector2 position, float fontSize, UnityEngine.Events.UnityAction action,
        float rightMargin = 24f, float leftMargin = 24f)
    {
        Image image = MenuUIFactory.CreateImage(parent, name, sprite, size, position, true);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        TMP_Text text = CrearTextoTMP(image.transform, "Label", label, fontSize,
            size - new Vector2(32f, 12f), Vector2.zero, 0.3f,
            new Vector4(leftMargin, 0f, rightMargin, 0f));
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

        MenuUIFactory.CreateSlider(parent, "SliderSonidos", "SONIDOS", sliderFondo, sliderRelleno,
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
        bool dosJugadores = modo == 2;
        ConfigurarTarjetasPorModo(dosJugadores);
        if (tituloInstrucciones != null)
        {
            tituloInstrucciones.text = dosJugadores == false
                ? "CONTROLES (MODO 1 JUGADOR)"
                : "CONTROLES (MODO 2 JUGADORES)";
        }

        if (tituloTarjetaChispa != null)
        {
            tituloTarjetaChispa.text = dosJugadores ? "JUGADOR 1: CHISPA" : "CHISPA";
        }
        if (tituloTarjetaRoca != null)
        {
            tituloTarjetaRoca.text = dosJugadores ? "JUGADOR 2: ROCA" : "ROCA";
        }
        if (descripcionChispa != null)
        {
            descripcionChispa.gameObject.SetActive(!dosJugadores);
            descripcionChispa.text = "ACTIVA BOTONES Y PUENTES";
        }
        if (descripcionRoca != null)
        {
            descripcionRoca.gameObject.SetActive(!dosJugadores);
            descripcionRoca.text = "EMPUJA Y MUEVE CAJAS";
        }
        if (controlesChispa != null)
        {
            controlesChispa.SetActive(dosJugadores);
            ConfigurarTarjetaDosJugadores(controlesChispa.transform,
                new[] { "W", "A", "S", "D" }, "MOVER Y SALTAR",
                "SHIFT IZQ", "ACTIVAR BOTONES\nY PUENTES", false);
        }
        if (controlesRoca != null)
        {
            controlesRoca.SetActive(dosJugadores);
            ConfigurarTarjetaDosJugadores(controlesRoca.transform,
                new[] { "▲", "◄", "▼", "►" }, "MOVER Y SALTAR",
                "SHIFT DER", "EMPUJAR Y\nMOVER CAJAS", true);
        }
        if (bannerControlesUnificados != null)
        {
            bannerControlesUnificados.SetActive(!dosJugadores);
        }
    }

    private void ConfigurarTarjetasPorModo(bool dosJugadores)
    {
        ConfigurarTarjetaLayout(tarjetaChispa, tituloTarjetaChispa, dosJugadores,
            new Vector2(-285f, 10f));
        ConfigurarTarjetaLayout(tarjetaRoca, tituloTarjetaRoca, dosJugadores,
            new Vector2(285f, 10f));
    }

    private void ConfigurarTarjetaLayout(GameObject tarjeta, TMP_Text titulo, bool dosJugadores,
        Vector2 dosJugadoresPosition)
    {
        if (tarjeta == null)
        {
            return;
        }

        RectTransform cardRect = tarjeta.GetComponent<RectTransform>();
        cardRect.sizeDelta = dosJugadores
            ? new Vector2(500f, 440f)
            : new Vector2(470f, 295f);
        cardRect.anchoredPosition = dosJugadores
            ? dosJugadoresPosition
            : new Vector2(dosJugadoresPosition.x < 0f ? -405f : 405f, 105f);

        Transform character = tarjeta.transform.Find("Personaje");
        if (character != null)
        {
            RectTransform characterRect = character.GetComponent<RectTransform>();
            characterRect.sizeDelta = dosJugadores
                ? new Vector2(150f, 105f)
                : new Vector2(150f, 115f);
            characterRect.anchoredPosition = dosJugadores
                ? new Vector2(0f, 135f)
                : new Vector2(0f, 78f);
        }

        if (titulo != null)
        {
            RectTransform titleRect = titulo.rectTransform;
            titleRect.sizeDelta = dosJugadores
                ? new Vector2(470f, 45f)
                : new Vector2(430f, 48f);
            titleRect.anchoredPosition = dosJugadores
                ? new Vector2(0f, 48f)
                : new Vector2(0f, -12f);
            titulo.fontSize = dosJugadores ? 26f : 30f;
        }

        Transform controls = tarjeta.transform.Find("ControlesDosJugadores");
        if (controls != null)
        {
            RectTransform controlsRect = controls.GetComponent<RectTransform>();
            controlsRect.sizeDelta = new Vector2(430f, 260f);
            controlsRect.anchoredPosition = Vector2.zero;
        }
    }

    private void ConfigurarTarjetaDosJugadores(Transform root, string[] teclas,
        string descripcionMovimiento, string teclaAccion, string descripcionAccion,
        bool flechas)
    {
        foreach (Transform child in root)
        {
            Destroy(child.gameObject);
        }

        Vector2[] keyPositions =
        {
            new Vector2(-115f, -18f),
            new Vector2(-149f, -52f),
            new Vector2(-115f, -52f),
            new Vector2(-81f, -52f)
        };
        for (int i = 0; i < teclas.Length; i++)
        {
            EstilizarTecla(root, "Tecla" + i, teclas[i], 34f, keyPositions[i], 34f);
        }

        TMP_Text movimiento = CrearTextoTMP(root, "Movimiento", descripcionMovimiento, 20f,
            new Vector2(190f, 55f), new Vector2(75f, -35f), 0f,
            new Vector4(0f, 0f, 0f, 0f));
        movimiento.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        movimiento.alignment = TextAlignmentOptions.Left;
        movimiento.outlineWidth = 0f;

        RectTransform actionKey = MenuUIFactory.CreateRect(root, "TeclaAccion",
            new Vector2(130f, 42f), new Vector2(-115f, -125f));
        Image actionImage = actionKey.gameObject.AddComponent<Image>();
        actionImage.color = new Color(216f / 255f, 195f / 255f, 158f / 255f, 1f);
        Outline actionOutline = actionKey.gameObject.AddComponent<Outline>();
        actionOutline.effectColor = new Color(92f / 255f, 58f / 255f, 33f / 255f, 1f);
        actionOutline.effectDistance = new Vector2(2f, -2f);
        Shadow actionShadow = actionKey.gameObject.AddComponent<Shadow>();
        actionShadow.effectColor = new Color(59f / 255f, 35f / 255f, 20f / 255f, 0.9f);
        actionShadow.effectDistance = new Vector2(0f, -3f);
        TMP_Text actionText = (TMP_Text)CrearTextoTMP(actionKey, "Texto", teclaAccion, 16f,
            actionKey.sizeDelta, Vector2.zero, 0f,
            new Vector4(0f, 0f, 0f, 0f));
        actionText.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        actionText.fontStyle = FontStyles.Bold;
        actionText.outlineWidth = 0f;

        TMP_Text accion = CrearTextoTMP(root, "Accion", descripcionAccion, 19f,
            new Vector2(220f, 65f), new Vector2(75f, -125f), 0f,
            new Vector4(0f, 0f, 0f, 0f));
        accion.color = new Color(59f / 255f, 35f / 255f, 20f / 255f, 1f);
        accion.alignment = TextAlignmentOptions.Left;
        accion.fontStyle = FontStyles.Bold;
        accion.outlineWidth = 0f;
        accion.textWrappingMode = TextWrappingModes.Normal;
    }

    private void IniciarEscenaSeleccionada()
    {
        if (string.IsNullOrEmpty(escenaSeleccionada))
        {
            return;
        }

        PlayerPrefs.Save();
        StartCoroutine(CargarEscenaConSonido(escenaSeleccionada));
    }

    private System.Collections.IEnumerator CargarEscenaConSonido(string nombreEscena)
    {
        AudioManager manager = AudioManager.Instancia;
        manager.ReproducirBotonIniciarPartida();

        float espera = manager.DuracionBotonIniciarPartida();
        if (espera > 0f)
        {
            yield return new WaitForSecondsRealtime(espera);
        }

        SceneManager.LoadScene(nombreEscena);
    }

    private void SalirDelJuego()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}