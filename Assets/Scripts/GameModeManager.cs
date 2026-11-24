using UnityEngine;

[DefaultExecutionOrder(-50)]
public class GameModeManager : MonoBehaviour
{
    private void Start()
    {
        MovimientoJugador chispa = BuscarJugador(MovimientoJugador.Protagonista.Chispa);
        MovimientoJugador roca = BuscarJugador(MovimientoJugador.Protagonista.Roca);
        if (chispa == null || roca == null)
        {
            Debug.LogError("GameModeManager requiere a Chispa y Roca en la escena.", this);
            return;
        }

        int modo = PlayerPrefs.GetInt("ModoJugadores", 2);
        if (modo == 1)
        {
            int heroeElegido = Mathf.Clamp(PlayerPrefs.GetInt("HeroeElegido", 0), 0, 1);
            MovimientoJugador humano = heroeElegido == 0 ? chispa : roca;
            MovimientoJugador companero = heroeElegido == 0 ? roca : chispa;

            AsegurarRastreador(humano);
            ConfigurarHumano(humano);
            ConfigurarCompanero(companero, humano);
            return;
        }

        ConfigurarHumano(chispa);
        ConfigurarHumano(roca);
        DeshabilitarIA(chispa);
        DeshabilitarIA(roca);
    }

    private void AsegurarRastreador(MovimientoJugador jugador)
    {
        if (jugador.GetComponent<PlayerPathTracker>() == null)
        {
            jugador.gameObject.AddComponent<PlayerPathTracker>();
        }
    }

    private void ConfigurarHumano(MovimientoJugador jugador)
    {
        jugador.enabled = true;
        jugador.ConfigurarControlHumano(true);
        DeshabilitarIA(jugador);
    }

    private void ConfigurarCompanero(MovimientoJugador companero, MovimientoJugador humano)
    {
        AICompanion ai = companero.GetComponent<AICompanion>();
        if (ai == null)
        {
            ai = companero.gameObject.AddComponent<AICompanion>();
        }

        companero.ConfigurarControlHumano(false);
        // AICompanion copia parámetros y referencias antes de detener el controlador.
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
        MovimientoJugador[] jugadores = FindObjectsByType<MovimientoJugador>();
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