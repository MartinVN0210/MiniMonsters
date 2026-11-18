using System.Collections.Generic;
using UnityEngine;

public readonly struct PlayerPathPoint
{
    public readonly Vector2 Posicion;
    public readonly bool SaltoAqui;

    public PlayerPathPoint(Vector2 posicion, bool saltoAqui)
    {
        Posicion = posicion;
        SaltoAqui = saltoAqui;
    }
}

[RequireComponent(typeof(MovimientoJugador))]
public class PlayerPathTracker : MonoBehaviour
{
    [SerializeField] private float intervaloRegistro = 0.2f;
    [SerializeField] private int capacidadMaxima = 80;

    private readonly Queue<PlayerPathPoint> puntos = new Queue<PlayerPathPoint>();
    private MovimientoJugador movimiento;
    private float siguienteRegistro;

    public int Count => puntos.Count;

    private void Awake()
    {
        movimiento = GetComponent<MovimientoJugador>();
    }

    private void OnEnable()
    {
        if (movimiento == null)
        {
            movimiento = GetComponent<MovimientoJugador>();
        }

        movimiento.SaltoEjecutado += RegistrarSalto;
    }

    private void OnDisable()
    {
        if (movimiento != null)
        {
            movimiento.SaltoEjecutado -= RegistrarSalto;
        }
    }

    private void Update()
    {
        if (!movimiento.ControlHumano || !movimiento.EnElSuelo || Time.time < siguienteRegistro)
        {
            return;
        }

        RegistrarPunto(new PlayerPathPoint(transform.position, false));
        siguienteRegistro = Time.time + intervaloRegistro;
    }

    public bool TryPeek(out PlayerPathPoint punto)
    {
        if (puntos.Count == 0)
        {
            punto = default;
            return false;
        }

        punto = puntos.Peek();
        return true;
    }

    public bool TryDequeue(out PlayerPathPoint punto)
    {
        if (puntos.Count == 0)
        {
            punto = default;
            return false;
        }

        punto = puntos.Dequeue();
        return true;
    }

    public void Limpiar()
    {
        puntos.Clear();
    }

    private void RegistrarSalto(MovimientoJugador jugador)
    {
        RegistrarPunto(new PlayerPathPoint(transform.position, true));
    }

    private void RegistrarPunto(PlayerPathPoint punto)
    {
        while (puntos.Count >= capacidadMaxima)
        {
            puntos.Dequeue();
        }

        puntos.Enqueue(punto);
    }
}
