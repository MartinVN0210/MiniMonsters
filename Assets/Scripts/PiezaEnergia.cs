using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PiezaEnergia : MonoBehaviour
{
    private bool recogida;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger == null)
        {
            Debug.LogError("La pieza de energía necesita un Collider2D para poder recogerse.", this);
            enabled = false;
            return;
        }

        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (recogida || !other.CompareTag("Player"))
        {
            return;
        }

        if (GameManager.Instancia == null)
        {
            Debug.LogError("No hay un GameManager activo para completar el nivel.", this);
            return;
        }

        recogida = true;
        GameManager.Instancia.CompletarNivel();
        Destroy(gameObject);
    }
}
