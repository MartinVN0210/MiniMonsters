using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public class MovimientoJugador : MonoBehaviour
{
    public enum Protagonista
    {
        Chispa,
        Roca
    }

    [Header("Parámetros de Movimiento")]
    [SerializeField] private Protagonista protagonista;
    [SerializeField] private float velocidad = 7f;
    [SerializeField] private float fuerzaSalto = 12f;

    [Header("Detección de Suelo")]
    [SerializeField] private Transform checadorSuelo;
    [SerializeField] private LayerMask capaSuelo;

    [Header("Detección de Pared")]
    [SerializeField] private float radioDeteccionPared = 0.12f;
    [SerializeField] private LayerMask capaPared;

    [Header("Acciones")]
    [SerializeField] private UnityEvent alInteractuar = new UnityEvent();
    [SerializeField] private UnityEvent alUsarHabilidad = new UnityEvent();

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float entradaHorizontal;
    private float entradaIA;
    private bool controlHumano = true;
    private bool saltoIASolicitado;
    private bool interaccionIASolicitada;
    private bool habilidadIASolicitada;
    private bool controlesUnificados;
    private bool enElSuelo;
    private Collider2D colisionador;
    private ContactFilter2D filtroSuelo;
    private readonly Collider2D[] resultadosDeteccionSuelo = new Collider2D[8];
    private readonly RaycastHit2D[] resultadosDeteccionPared = new RaycastHit2D[4];
    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParameter = Animator.StringToHash("IsGrounded");

    public Protagonista Tipo => protagonista;
    public float EntradaHorizontal => entradaHorizontal;
    public bool EnElSuelo => enElSuelo;
    public bool ControlHumano => controlHumano;
    public float Velocidad => velocidad;
    public float FuerzaSalto => fuerzaSalto;
    public Transform ChecadorSuelo => checadorSuelo;
    public LayerMask CapaSuelo => capaSuelo;
    public LayerMask CapaPared => capaPared;
    public PhysicsMaterial2D MaterialMovimiento => colisionador == null ? null : colisionador.sharedMaterial;
    public Animator AnimatorReferencia => animator;
    public SpriteRenderer SpriteRendererReferencia => spriteRenderer;

    public event Action<MovimientoJugador> InteraccionPresionada;
    public event Action<MovimientoJugador> HabilidadPresionada;
    public event Action<MovimientoJugador> SaltoEjecutado;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        colisionador = GetComponent<Collider2D>();
        capaSuelo = capaSuelo.value | LayerMask.GetMask("Ground", "Box", "Player", "Suelo");
        capaPared = LayerMask.GetMask("Ground", "Box");
        filtroSuelo.SetLayerMask(capaSuelo);
        filtroSuelo.useTriggers = false;
    }

    private void Update()
    {
        bool solicitarSalto = false;
        if (controlHumano)
        {
            Keyboard teclado = Keyboard.current;
            if (teclado == null)
            {
                entradaHorizontal = 0f;
                enElSuelo = DetectarSuelo();
                ActualizarParametrosAnimator();
                return;
            }

            Key izquierda = controlesUnificados || protagonista == Protagonista.Chispa ? Key.A : Key.LeftArrow;
            Key derecha = controlesUnificados || protagonista == Protagonista.Chispa ? Key.D : Key.RightArrow;
            Key salto = controlesUnificados || protagonista == Protagonista.Chispa ? Key.W : Key.UpArrow;
            Key interactuar = controlesUnificados || protagonista == Protagonista.Chispa ? Key.E : Key.Enter;
            Key habilidad = controlesUnificados || protagonista == Protagonista.Chispa ? Key.LeftShift : Key.RightShift;

            entradaHorizontal = (teclado[derecha].isPressed ? 1f : 0f)
                - (teclado[izquierda].isPressed ? 1f : 0f);
            solicitarSalto = teclado[salto].wasPressedThisFrame;

            if (teclado[interactuar].wasPressedThisFrame)
            {
                DispararInteraccion();
            }

            if (teclado[habilidad].wasPressedThisFrame)
            {
                DispararHabilidad();
            }
        }
        else
        {
            entradaHorizontal = entradaIA;
            solicitarSalto = saltoIASolicitado;
            saltoIASolicitado = false;

            if (interaccionIASolicitada)
            {
                DispararInteraccion();
                interaccionIASolicitada = false;
            }

            if (habilidadIASolicitada)
            {
                DispararHabilidad();
                habilidadIASolicitada = false;
            }
        }

        enElSuelo = DetectarSuelo();

        if (solicitarSalto && enElSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);

            if (GameManager.Instancia != null)
            {
                GameManager.Instancia.ReproducirSonidoSalto();
            }

            SaltoEjecutado?.Invoke(this);
        }

        ActualizarParametrosAnimator();
    }

    public void ConfigurarControlHumano(bool habilitado, bool unificarControles = false)
    {
        controlHumano = habilitado;
        controlesUnificados = habilitado && unificarControles;
        entradaIA = 0f;
        entradaHorizontal = 0f;
        saltoIASolicitado = false;
        interaccionIASolicitada = false;
        habilidadIASolicitada = false;
    }

    public void EstablecerEntradaIA(float horizontal, bool saltar)
    {
        if (!controlHumano)
        {
            entradaIA = Mathf.Clamp(horizontal, -1f, 1f);
            entradaHorizontal = entradaIA;
            saltoIASolicitado |= saltar;
        }
    }

    public void SolicitarInteraccionIA()
    {
        if (!controlHumano)
        {
            interaccionIASolicitada = true;
        }
    }

    public void SolicitarHabilidadIA()
    {
        if (!controlHumano)
        {
            habilidadIASolicitada = true;
        }
    }

    public void DispararInteraccion()
    {
        alInteractuar.Invoke();
        InteraccionPresionada?.Invoke(this);
    }

    public void DispararHabilidad()
    {
        alUsarHabilidad.Invoke();
        HabilidadPresionada?.Invoke(this);
    }

    private void ActualizarParametrosAnimator()
    {
        if (animator == null)
        {
            return;
        }

        animator.SetFloat(SpeedParameter, rb == null ? Mathf.Abs(entradaHorizontal) : Mathf.Abs(rb.linearVelocity.x));
        animator.SetBool(IsGroundedParameter, enElSuelo);
    }

    private bool DetectarSuelo()
    {
        if (checadorSuelo == null)
        {
            return false;
        }

        float ancho = colisionador == null ? 0.05f : colisionador.bounds.size.x * 0.7f;
        Vector2 centro = (Vector2)checadorSuelo.position + Vector2.down * 0.03f;
        Vector2 tamano = new Vector2(Mathf.Max(0.05f, ancho), 0.18f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(centro, tamano, 0f, LayerMask.GetMask("Ground", "Box"));
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit != null && !hit.isTrigger && hit != colisionador && hit.attachedRigidbody != rb)
            {
                return true;
            }
        }

        return false;
    }

    private bool DetectarMuroEnDireccion(float direccion)
    {
        if (colisionador == null || direccion == 0f)
        {
            return false;
        }

        Bounds bounds = colisionador.bounds;
        Vector2 tamano = new Vector2(0.05f, bounds.size.y * 0.5f);
        float centroY = bounds.min.y + bounds.size.y * 0.6f;
        float semiancho = tamano.x * 0.5f;
        float sentido = Mathf.Sign(direccion);
        float centroX = sentido > 0f
            ? bounds.max.x + semiancho + 0.001f
            : bounds.min.x - semiancho - 0.001f;
        Vector2 origen = new Vector2(centroX, centroY);
        Vector2 direccionCast = new Vector2(sentido, 0f);
        ContactFilter2D filtroPared = new ContactFilter2D();
        filtroPared.SetLayerMask(capaPared);
        filtroPared.useTriggers = false;
        int cantidad = Physics2D.BoxCast(
            origen,
            tamano,
            0f,
            direccionCast,
            filtroPared,
            resultadosDeteccionPared,
            Mathf.Max(0.01f, radioDeteccionPared));

        return cantidad > 0;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        bool bloqueadoPorMuro = !enElSuelo
            && Mathf.Abs(entradaHorizontal) > 0.05f
            && DetectarMuroEnDireccion(entradaHorizontal);
        if (bloqueadoPorMuro)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(entradaHorizontal * velocidad, rb.linearVelocity.y);
        }

        if (spriteRenderer != null)
        {
            if (rb.linearVelocity.x < -0.05f)
            {
                spriteRenderer.flipX = true;
            }
            else if (rb.linearVelocity.x > 0.05f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (checadorSuelo != null)
        {
            Collider2D collider = GetComponent<Collider2D>();
            float ancho = collider == null ? 0.01f : collider.bounds.size.x * 0.7f;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(checadorSuelo.position, new Vector3(ancho, 0.06f, 0f));
        }
    }
}