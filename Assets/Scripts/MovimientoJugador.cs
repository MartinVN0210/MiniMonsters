using UnityEngine;
using UnityEngine.InputSystem; // Namespace nativo de Unity 6

public class MovimientoJugador : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    [SerializeField] private float velocidad = 7f;
    [SerializeField] private float fuerzaSalto = 12f;

    private Rigidbody2D rb;
    private float entradaHorizontal;
    private bool enElSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Lectura directa del teclado usando el nuevo Input System de Unity 6
        if (Keyboard.current != null)
        {
            entradaHorizontal = 0f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) entradaHorizontal = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) entradaHorizontal = -1f;

            // Salto con Barra Espaciadora
            if (Keyboard.current.spaceKey.wasPressedThisFrame && enElSuelo)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
                enElSuelo = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Aplicar velocidad física
        rb.linearVelocity = new Vector2(entradaHorizontal * velocidad, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D colision)
    {
        enElSuelo = true;
    }

    private void OnCollisionExit2D(Collision2D colision)
    {
        enElSuelo = false;
    }
}