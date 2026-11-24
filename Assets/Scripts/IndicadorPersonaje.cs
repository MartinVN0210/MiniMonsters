using TMPro;
using UnityEngine;

public class IndicadorPersonaje : MonoBehaviour
{
    [SerializeField] private string nombrePersonaje = "";
    [SerializeField] private Vector3 posicionLocal = new Vector3(0f, 1.4f, 0f);
    [SerializeField] private float tamanoFuente = 3f;

    private TextMeshPro etiqueta;

    private void Awake()
    {
        CrearEtiquetaSiHaceFalta();
    }

    private void OnEnable()
    {
        CrearEtiquetaSiHaceFalta();
    }

    public void Mostrar(bool visible)
    {
        CrearEtiquetaSiHaceFalta();
        if (etiqueta != null)
        {
            etiqueta.gameObject.SetActive(visible);
        }
    }

    public void ConfigurarNombre(string nombre)
    {
        nombrePersonaje = nombre;
        CrearEtiquetaSiHaceFalta();
        if (etiqueta != null)
        {
            etiqueta.text = nombrePersonaje;
        }
    }

    private void CrearEtiquetaSiHaceFalta()
    {
        if (etiqueta != null)
        {
            return;
        }

        Transform existente = transform.Find("IndicadorNombre");
        if (existente != null)
        {
            etiqueta = existente.GetComponent<TextMeshPro>();
            if (etiqueta == null)
            {
                DestroyImmediate(existente.gameObject);
                existente = null;
            }
        }

        if (existente == null)
        {
            GameObject objeto = new GameObject("IndicadorNombre");
            objeto.transform.SetParent(transform, false);
            objeto.transform.localPosition = posicionLocal;
            etiqueta = objeto.AddComponent<TextMeshPro>();
            etiqueta.text = nombrePersonaje;
            etiqueta.fontSize = tamanoFuente;
            etiqueta.alignment = TextAlignmentOptions.Center;
            etiqueta.color = Color.white;
            etiqueta.fontStyle = FontStyles.Bold;
            etiqueta.outlineWidth = 0.28f;
            etiqueta.outlineColor = Color.black;
            etiqueta.sortingOrder = 32767;
            etiqueta.renderer.sortingLayerName = "Default";
        }

        etiqueta.transform.localScale = Vector3.one;
        etiqueta.transform.localRotation = Quaternion.identity;
        etiqueta.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (etiqueta != null)
        {
            etiqueta.transform.localRotation = Quaternion.identity;

            Vector3 escalaPadre = etiqueta.transform.parent == null
                ? Vector3.one
                : etiqueta.transform.parent.lossyScale;
            float escalaPadreX = Mathf.Abs(escalaPadre.x);
            float escalaX = Mathf.Approximately(escalaPadreX, 0f)
                ? 1f
                : 1f / escalaPadreX;
            etiqueta.transform.localScale = new Vector3(escalaX, 1f, 1f);
        }
    }
}
