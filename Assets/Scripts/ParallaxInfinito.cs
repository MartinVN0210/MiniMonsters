using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxInfinito : MonoBehaviour
{
    [SerializeField] private Camera camaraObjetivo;
    [Range(0f, 1f)]
    [SerializeField] private float factorParallax = 0.1f;
    [SerializeField] private SpriteRenderer[] segmentos;

    private float posicionInicialCamaraX;
    private float factorParallaxAplicado;
    private Vector3[] posicionesIniciales;

    private void Awake()
    {
        if (camaraObjetivo == null)
        {
            camaraObjetivo = Camera.main;
        }

        if (camaraObjetivo == null)
        {
            Debug.LogError("ParallaxInfinito requiere una cámara objetivo.", this);
            enabled = false;
            return;
        }

        if (!PrepararSegmentos())
        {
            enabled = false;
            return;
        }

        factorParallaxAplicado = Mathf.Lerp(0.02f, 0.05f, Mathf.Clamp01(factorParallax));
        posicionInicialCamaraX = camaraObjetivo.transform.position.x;
        posicionesIniciales = new Vector3[segmentos.Length];
        for (int i = 0; i < segmentos.Length; i++)
        {
            posicionesIniciales[i] = segmentos[i].transform.position;
        }
    }

    private bool PrepararSegmentos()
    {
        if (segmentos == null)
        {
            segmentos = new SpriteRenderer[0];
            return false;
        }

        bool contieneReferenciasInvalidas = false;
        for (int i = 0; i < segmentos.Length; i++)
        {
            if (segmentos[i] == null || segmentos[i].sprite == null)
            {
                contieneReferenciasInvalidas = true;
                break;
            }
        }

        if (contieneReferenciasInvalidas)
        {
            var segmentosValidos = new System.Collections.Generic.List<SpriteRenderer>();
            for (int i = 0; i < segmentos.Length; i++)
            {
                SpriteRenderer segmento = segmentos[i];
                if (segmento == null || segmento.sprite == null || segmentosValidos.Contains(segmento))
                {
                    continue;
                }

                segmentosValidos.Add(segmento);
            }

            segmentos = segmentosValidos.ToArray();
        }

        return segmentos.Length >= 2;
    }

    private void LateUpdate()
    {
        if (camaraObjetivo == null || segmentos == null || posicionesIniciales == null)
        {
            enabled = false;
            return;
        }

        float desplazamientoCamaraX = camaraObjetivo.transform.position.x - posicionInicialCamaraX;
        int cantidadSegmentos = Mathf.Min(segmentos.Length, posicionesIniciales.Length);
        for (int i = 0; i < cantidadSegmentos; i++)
        {
            SpriteRenderer segmento = segmentos[i];
            if (segmento == null || segmento.sprite == null)
            {
                continue;
            }

            Vector3 posicion = posicionesIniciales[i];
            posicion.x += desplazamientoCamaraX * factorParallaxAplicado;
            segmento.transform.position = posicion;
        }
    }
}