using UnityEngine;

public class ZonaMuerte2D : MonoBehaviour
{
    private bool procesada;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (procesada || !collision.CompareTag("Player"))
        {
            return;
        }

        GameManager manager = GameManager.Instancia;
        if (manager == null)
        {
            Debug.LogError("No hay un GameManager activo para registrar la pérdida de vida.", this);
            return;
        }

        procesada = true;
        manager.PerderVida();
    }
}