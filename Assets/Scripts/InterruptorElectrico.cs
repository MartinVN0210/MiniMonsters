using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InterruptorElectrico : MonoBehaviour
{
    [SerializeField] private PuenteRoto puente;
    [SerializeField] private SpriteRenderer indicador;
    [SerializeField] private Color colorApagado = Color.red;
    [SerializeField] private Color colorEncendido = Color.green;

    private readonly HashSet<MovimientoJugador> jugadoresCercanos = new HashSet<MovimientoJugador>();
    private bool activado;

    public bool Activado => activado;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        ActualizarIndicador();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        MovimientoJugador jugador = other.GetComponentInParent<MovimientoJugador>();
        if (jugador != null && jugadoresCercanos.Add(jugador))
        {
            jugador.InteraccionPresionada += IntentarActivar;
            jugador.HabilidadPresionada += IntentarActivar;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        MovimientoJugador jugador = other.GetComponentInParent<MovimientoJugador>();
        if (jugador != null && jugadoresCercanos.Remove(jugador))
        {
            jugador.InteraccionPresionada -= IntentarActivar;
            jugador.HabilidadPresionada -= IntentarActivar;
        }
    }

    private void OnDisable()
    {
        foreach (MovimientoJugador jugador in jugadoresCercanos)
        {
            if (jugador != null)
            {
                jugador.InteraccionPresionada -= IntentarActivar;
                jugador.HabilidadPresionada -= IntentarActivar;
            }
        }

        jugadoresCercanos.Clear();
    }

    private void IntentarActivar(MovimientoJugador jugador)
    {
        ActivarDesdeJugador(jugador);
    }

    public bool ActivarDesdeJugador(MovimientoJugador jugador)
    {
        if (activado || jugador == null || jugador.Tipo != MovimientoJugador.Protagonista.Chispa)
        {
            return false;
        }

        if (puente == null)
        {
            Debug.LogError("El interruptor no tiene un PuenteRoto asignado.", this);
            return false;
        }

        activado = true;
        ActualizarIndicador();
        puente.Activar();

        GameManager manager = GameManager.Instancia;
        if (manager != null)
        {
            manager.ReproducirSonidoInterruptor();
        }

        return true;
    }

    private void ActualizarIndicador()
    {
        if (indicador != null)
        {
            indicador.color = activado ? colorEncendido : colorApagado;
        }
    }
}
