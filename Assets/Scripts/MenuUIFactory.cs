using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class MenuUIFactory
{
    private static Font defaultFont;
    private static readonly Color TextColor = new Color(1f, 0.99f, 0.94f, 1f);

    public static RectTransform CreateRect(Transform parent, string name, Vector2 size, Vector2 position)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    public static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 position, bool sliced = false)
    {
        RectTransform rect = CreateRect(parent, name, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = !sliced;
        return image;
    }

    public static Text CreateText(Transform parent, string name, string value, int fontSize, Color color, Vector2 size, Vector2 position)
    {
        RectTransform rect = CreateRect(parent, name, size, position);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = GetFont();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = color == Color.white ? TextColor : color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(2.5f, -2.5f);
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.65f);
        shadow.effectDistance = new Vector2(0f, -1.5f);
        return text;
    }

    public static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Sprite sprite,
        Vector2 size,
        Vector2 position,
        UnityAction onClick,
        int fontSize = 30)
    {
        Image image = CreateImage(parent, name, sprite, size, position, true);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        CreateText(image.transform, "Label", label, fontSize, Color.white,
            size - new Vector2(24f, 12f), Vector2.zero);
        return button;
    }

    public static Slider CreateSlider(
        Transform parent,
        string name,
        string label,
        Sprite background,
        Sprite fill,
        Sprite handle,
        float value,
        UnityAction<float> onChanged,
        Vector2 position)
    {
        RectTransform root = CreateRect(parent, name, new Vector2(620f, 130f), position);
        CreateText(root, "Label", label, 24, Color.white,
            new Vector2(460f, 30f), new Vector2(0f, 43f));

        RectTransform sliderRect = CreateRect(root, "Slider", new Vector2(460f, 40f), new Vector2(0f, -7f));
        Image backgroundImage = sliderRect.gameObject.AddComponent<Image>();
        backgroundImage.sprite = background;
        backgroundImage.type = Image.Type.Sliced;

        CreateText(sliderRect, "IconoIzquierdo", "♪", 22, Color.white,
            new Vector2(34f, 34f), new Vector2(-253f, 0f));
        CreateText(sliderRect, "IconoDerecho", "♫", 22, Color.white,
            new Vector2(34f, 34f), new Vector2(253f, 0f));

        RectTransform fillArea = CreateRect(sliderRect, "Fill Area", Vector2.zero, Vector2.zero);
        fillArea.anchorMin = Vector2.zero;
        fillArea.anchorMax = Vector2.one;
        fillArea.offsetMin = new Vector2(18f, 4f);
        fillArea.offsetMax = new Vector2(-18f, -4f);

        RectTransform fillRect = CreateRect(fillArea, "Fill", Vector2.zero, Vector2.zero);
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImage = fillRect.gameObject.AddComponent<Image>();
        fillImage.sprite = fill;
        fillImage.type = Image.Type.Sliced;

        RectTransform handleSlideArea = CreateRect(sliderRect, "Handle Slide Area", Vector2.zero, Vector2.zero);
        handleSlideArea.anchorMin = Vector2.zero;
        handleSlideArea.anchorMax = Vector2.one;
        handleSlideArea.offsetMin = new Vector2(22f, 0f);
        handleSlideArea.offsetMax = new Vector2(-22f, 0f);

        RectTransform handleRect = CreateRect(handleSlideArea, "Handle", new Vector2(40f, 0f), Vector2.zero);
        Image handleImage = handleRect.gameObject.AddComponent<Image>();
        handleImage.sprite = handle;
        handleImage.preserveAspect = true;

        Slider slider = sliderRect.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.value = Mathf.Clamp01(value);
        slider.onValueChanged.AddListener(onChanged);
        return slider;
    }

    public static void SetVisible(GameObject panel, bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
        }
    }

    public static Font GetFont()
    {
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null)
            {
                defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 24);
            }
        }

        return defaultFont;
    }
}
