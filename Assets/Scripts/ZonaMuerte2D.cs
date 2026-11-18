using UnityEngine;
using System.Collections.Generic;

public class ZonaMuerte2D : MonoBehaviour
{
    private bool procesada;
    private readonly HashSet<Collider2D> jugadoresDentro = new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || !jugadoresDentro.Add(collision))
        {
            return;
        }

        if (procesada)
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

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadoresDentro.Remove(collision);
            if (jugadoresDentro.Count == 0)
            {
                procesada = false;
            }
        }
    }
}