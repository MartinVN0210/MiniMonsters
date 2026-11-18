using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SalidaVictoria : MonoBehaviour
{
    [SerializeField] private GameObject panelVictoria;

    private readonly HashSet<MovimientoJugador> jugadoresDentro = new HashSet<MovimientoJugador>();
    private bool victoriaMostrada;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MovimientoJugador jugador = other.GetComponentInParent<MovimientoJugador>();
        if (jugador != null)
        {
            jugadoresDentro.Add(jugador);
            MostrarVictoriaSiCorresponde();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MovimientoJugador jugador = other.GetComponentInParent<MovimientoJugador>();
        if (jugador != null)
        {
            jugadoresDentro.Remove(jugador);
        }
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    private void MostrarVictoriaSiCorresponde()
    {
        if (victoriaMostrada || jugadoresDentro.Count < 2)
        {
            return;
        }

        bool chispaDentro = false;
        bool rocaDentro = false;
        foreach (MovimientoJugador jugador in jugadoresDentro)
        {
            chispaDentro |= jugador.Tipo == MovimientoJugador.Protagonista.Chispa;
            rocaDentro |= jugador.Tipo == MovimientoJugador.Protagonista.Roca;
        }

        if (!chispaDentro || !rocaDentro)
        {
            return;
        }

        victoriaMostrada = true;
        Time.timeScale = 0f;
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }
        else
        {
            Debug.LogError("La salida de victoria no tiene Panel_Victoria asignado.", this);
        }
    }
}
