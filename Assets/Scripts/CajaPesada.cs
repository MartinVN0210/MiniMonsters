using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CajaPesada : MonoBehaviour
{
    [SerializeField] private float fuerzaEmpuje = 24f;
    [SerializeField] private float duracionEmpuje = 0.3f;

    private readonly HashSet<MovimientoJugador> jugadoresEnContacto = new HashSet<MovimientoJugador>();
    private Rigidbody2D rb;
    private float tiempoEmpujeRestante;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        MovimientoJugador jugador = collision.collider.GetComponentInParent<MovimientoJugador>();
        if (jugador != null && jugadoresEnContacto.Add(jugador))
        {
            jugador.HabilidadPresionada += IntentarEmpujar;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        MovimientoJugador jugador = collision.collider.GetComponentInParent<MovimientoJugador>();
        if (jugador != null && jugadoresEnContacto.Remove(jugador))
        {
            jugador.HabilidadPresionada -= IntentarEmpujar;
        }
    }

    private void FixedUpdate()
    {
        tiempoEmpujeRestante = Mathf.Max(0f, tiempoEmpujeRestante - Time.fixedDeltaTime);
        if (tiempoEmpujeRestante <= 0f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX;
        }
    }

    private void OnDisable()
    {
        foreach (MovimientoJugador jugador in jugadoresEnContacto)
        {
            if (jugador != null)
            {
                jugador.HabilidadPresionada -= IntentarEmpujar;
            }
        }

        jugadoresEnContacto.Clear();
    }

    private void IntentarEmpujar(MovimientoJugador jugador)
    {
        if (jugador.Tipo != MovimientoJugador.Protagonista.Roca || jugador.EntradaHorizontal == 0f)
        {
            return;
        }

        float direccionHaciaCaja = Mathf.Sign(transform.position.x - jugador.transform.position.x);
        if (Mathf.Sign(jugador.EntradaHorizontal) != direccionHaciaCaja)
        {
            return;
        }

        tiempoEmpujeRestante = duracionEmpuje;
        rb.constraints &= ~RigidbodyConstraints2D.FreezePositionX;
        rb.AddForce(Vector2.right * direccionHaciaCaja * fuerzaEmpuje, ForceMode2D.Impulse);
    }
}
