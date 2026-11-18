using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SalidaCooperativa : MonoBehaviour
{
    [SerializeField] private string siguienteNivel;

    private readonly HashSet<MovimientoJugador> jugadoresDentro = new HashSet<MovimientoJugador>();
    private bool cargandoNivel;

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
            IntentarCompletar();
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

    private void IntentarCompletar()
    {
        if (cargandoNivel || jugadoresDentro.Count < 2)
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

        cargandoNivel = true;
        SceneManager.LoadScene(siguienteNivel);
    }
}
