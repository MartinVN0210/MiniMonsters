using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class GameModeManager : MonoBehaviour
{
    private MovimientoJugador chispa;
    private MovimientoJugador roca;
    private MovimientoJugador humanoActual;
    private MovimientoJugador companeroActual;
    private bool esUnJugador;

    private void Start()
    {
        chispa = BuscarJugador(MovimientoJugador.Protagonista.Chispa);
        roca = BuscarJugador(MovimientoJugador.Protagonista.Roca);
        if (chispa == null || roca == null)
        {
            Debug.LogError("GameModeManager requiere a Chispa y Roca en la escena.", this);
            return;
        }

        int modo = PlayerPrefs.GetInt("ModoJugadores", 2);
        esUnJugador = (modo == 1);

        if (esUnJugador)
        {
            int heroeElegido = Mathf.Clamp(PlayerPrefs.GetInt("HeroeElegido", 0), 0, 1);
            MovimientoJugador humano = heroeElegido == 0 ? chispa : roca;
            MovimientoJugador companero = heroeElegido == 0 ? roca : chispa;
            AsignarRolesUnJugador(humano, companero);
            return;
        }

        ConfigurarHumano(chispa);
        ConfigurarHumano(roca);
        DeshabilitarIA(chispa);
        DeshabilitarIA(roca);
    }

    private void Update()
    {
        if (!esUnJugador || Time.timeScale == 0f || humanoActual == null || companeroActual == null)
        {
            return;
        }

        Keyboard teclado = Keyboard.current;
        if (teclado == null)
        {
            return;
        }

        // Tab permite alternar manualmente entre Chispa y Roca en cualquier momento.
        if (teclado.tabKey.wasPressedThisFrame)
        {
            AsignarRolesUnJugador(companeroActual, humanoActual);
            return;
        }

        // C llama al compañero autónomo hacia tu posición.
        if (teclado.cKey.wasPressedThisFrame)
        {
            AICompanion ai = companeroActual.GetComponent<AICompanion>();
            if (ai != null)
            {
                ai.LlamarCompanero();
            }
        }
    }

    private void AsignarRolesUnJugador(MovimientoJugador nuevoHumano, MovimientoJugador nuevoCompanero)
    {
        humanoActual = nuevoHumano;
        companeroActual = nuevoCompanero;

        AsegurarRastreador(humanoActual);
        ConfigurarHumano(humanoActual);
        ConfigurarCompanero(companeroActual, humanoActual);
    }

    private void AsegurarRastreador(MovimientoJugador jugador)
    {
        PlayerPathTracker tracker = jugador.GetComponent<PlayerPathTracker>();
        if (tracker == null)
        {
            tracker = jugador.gameObject.AddComponent<PlayerPathTracker>();
        }

        tracker.Limpiar();
    }

    private void ConfigurarHumano(MovimientoJugador jugador)
    {
        DeshabilitarIA(jugador);
        jugador.enabled = true;
        jugador.ConfigurarControlHumano(true);
    }

    private void ConfigurarCompanero(MovimientoJugador companero, MovimientoJugador humano)
    {
        AICompanion ai = companero.GetComponent<AICompanion>();
        if (ai == null)
        {
            ai = companero.gameObject.AddComponent<AICompanion>();
        }

        companero.ConfigurarControlHumano(false);
        ai.Configurar(humano);
        companero.enabled = false;
        ai.enabled = true;
    }

    private void DeshabilitarIA(MovimientoJugador jugador)
    {
        AICompanion ai = jugador.GetComponent<AICompanion>();
        if (ai != null)
        {
            ai.enabled = false;
        }
    }

    private MovimientoJugador BuscarJugador(MovimientoJugador.Protagonista tipo)
    {
        MovimientoJugador[] jugadores = FindObjectsByType<MovimientoJugador>(FindObjectsInactive.Exclude);
        foreach (MovimientoJugador jugador in jugadores)
        {
            if (jugador.Tipo == tipo)
            {
                return jugador;
            }
        }

        return null;
    }
}