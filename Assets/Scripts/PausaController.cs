using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausaController : MonoBehaviour
{
    public Sprite panelPausaSprite;
    public Sprite panelGrandeSprite;
    public Sprite botonAzul;
    public Sprite botonVerde;
    public Sprite botonRojo;
    public Sprite sliderFondo;
    public Sprite sliderRelleno;
    public Sprite sliderPerilla;

    [Header("Audio de interfaz")]
    [SerializeField] private AudioClip clickBotonClip;

    private GameObject panelPausa;
    private GameObject panelAjustes;
    private Button botonHUDPausa;
    private bool construido;

    private void Start()
    {
        ConstruirPaneles();
        Button[] botones = GetComponentsInChildren<Button>(true);
        foreach (Button button in botones)
        {
            if (button.name == "BotonPausa")
            {
                botonHUDPausa = button;
                botonHUDPausa.onClick.AddListener(AbrirPausa);
            }
        }
    }

    private void Update()
    {
        if (KeyboardEscapePressed())
        {
            AlternarPausa();
        }
    }

    private void OnDestroy()
    {
        if (botonHUDPausa != null)
        {
            botonHUDPausa.onClick.RemoveListener(AbrirPausa);
        }

        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    public void AlternarPausa()
    {
        if (!construido)
        {
            ConstruirPaneles();
        }

        if (panelAjustes.activeSelf)
        {
            MostrarAjustes(false);
            return;
        }

        if (panelPausa.activeSelf)
        {
            Reanudar();
            return;
        }

        AbrirPausa();
    }

    public void AbrirPausa()
    {
        if (!construido)
        {
            ConstruirPaneles();
        }

        GameManager manager = GameManager.Instancia;
        if (manager != null && manager.GameOver)
        {
            return;
        }

        Time.timeScale = 0f;
        MostrarAjustes(false);
        MenuUIFactory.SetVisible(panelPausa, true);
    }

    public void Reanudar()
    {
        MostrarAjustes(false);
        MenuUIFactory.SetVisible(panelPausa, false);
        Time.timeScale = 1f;
    }

    private bool KeyboardEscapePressed()
    {
        return UnityEngine.InputSystem.Keyboard.current != null &&
               UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private void ConstruirPaneles()
    {
        if (construido)
        {
            return;
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("PausaController requiere un Canvas.", this);
            return;
        }

        panelPausa = CrearOverlay(canvas.transform, "Panel_Pausa");
        CrearMarco(panelPausa.transform, "MarcoPausa", panelPausaSprite, new Vector2(560f, 660f));
        MenuUIFactory.CreateText(panelPausa.transform, "Titulo", "PAUSA", 54, Color.white,
            new Vector2(500f, 90f), new Vector2(0f, 235f));
        MenuUIFactory.CreateButton(panelPausa.transform, "Reanudar", "REANUDAR", botonVerde,
            new Vector2(380f, 80f), new Vector2(0f, 105f), Reanudar, 30);
        MenuUIFactory.CreateButton(panelPausa.transform, "Ajustes", "AJUSTES", botonAzul,
            new Vector2(380f, 80f), new Vector2(0f, 0f), () => MostrarAjustes(true), 30);
        MenuUIFactory.CreateButton(panelPausa.transform, "MenuPrincipal", "MENÚ PRINCIPAL", botonRojo,
            new Vector2(380f, 80f), new Vector2(0f, -105f), VolverAlMenu, 28);

        panelAjustes = CrearOverlay(canvas.transform, "Panel_Ajustes_Pausa");
        CrearMarco(panelAjustes.transform, "MarcoAjustes", panelGrandeSprite, new Vector2(900f, 680f));
        MenuUIFactory.CreateText(panelAjustes.transform, "Titulo", "AJUSTES", 50, Color.white,
            new Vector2(800f, 90f), new Vector2(0f, 260f));
        MenuUIFactory.CreateSlider(panelAjustes.transform, "SliderMusica", "MÚSICA",
            sliderFondo, sliderRelleno, sliderPerilla, AudioPreferences.MusicVolume,
            AudioPreferences.SetMusicVolume, new Vector2(0f, 100f));
        MenuUIFactory.CreateSlider(panelAjustes.transform, "SliderSonidos", "SONIDOS",
            sliderFondo, sliderRelleno, sliderPerilla, AudioPreferences.SfxVolume,
            AudioPreferences.SetSfxVolume, new Vector2(0f, -40f));
        MenuUIFactory.CreateButton(panelAjustes.transform, "Regresar", "REGRESAR", botonRojo,
            new Vector2(340f, 90f), new Vector2(0f, -230f), () => MostrarAjustes(false), 32);

        RegistrarSonidosBotones(panelPausa);
        RegistrarSonidosBotones(panelAjustes);
        panelPausa.SetActive(false);
        panelAjustes.SetActive(false);
        construido = true;
    }

    private GameObject CrearOverlay(Transform parent, string name)
    {
        RectTransform rect = MenuUIFactory.CreateRect(parent, name, Vector2.zero, Vector2.zero);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image overlay = rect.gameObject.AddComponent<Image>();
        overlay.color = new Color(0f, 0f, 0f, 0.5f);
        overlay.raycastTarget = true;
        return rect.gameObject;
    }

    private void CrearMarco(Transform parent, string name, Sprite sprite, Vector2 size)
    {
        Image panel = MenuUIFactory.CreateImage(parent, name, sprite, size, Vector2.zero, true);
        panel.color = Color.white;
    }

    private void RegistrarSonidosBotones(GameObject panel)
    {
        if (clickBotonClip == null || panel == null)
        {
            return;
        }

        foreach (Button button in panel.GetComponentsInChildren<Button>(true))
        {
            button.onClick.AddListener(() => AudioManager.Instancia.ReproducirSFX(clickBotonClip));
        }
    }

    private void MostrarAjustes(bool visible)
    {
        MenuUIFactory.SetVisible(panelPausa, !visible);
        MenuUIFactory.SetVisible(panelAjustes, visible);
    }

    private void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
}
