using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoJugador : MonoBehaviour
{
    [Header("Parámetros de Movimiento")]
    [SerializeField] private float velocidad = 7f;
    [SerializeField] private float fuerzaSalto = 12f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform checadorSuelo;
    [SerializeField] private float radioDeteccion = 0.2f;
    [SerializeField] private LayerMask capaSuelo;

    private Rigidbody2D rb;
    private float entradaHorizontal;
    private bool enElSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            // Lectura de teclado A/D o Flechas
            entradaHorizontal = 0f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) entradaHorizontal = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) entradaHorizontal = -1f;

            // Detección precisa de suelo en los pies
            if (checadorSuelo != null)
            {
                enElSuelo = Physics2D.OverlapCircle(checadorSuelo.position, radioDeteccion, capaSuelo);
            }

            // Salto
            if (Keyboard.current.spaceKey.wasPressedThisFrame && enElSuelo)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(entradaHorizontal * velocidad, rb.linearVelocity.y);

        // Voltear el sprite según la dirección en que camina
        if (entradaHorizontal > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (entradaHorizontal < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnDrawGizmosSelected()
    {
        if (checadorSuelo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(checadorSuelo.position, radioDeteccion);
        }
    }
}