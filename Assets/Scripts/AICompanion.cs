using UnityEngine;

[RequireComponent(typeof(MovimientoJugador))]
public class AICompanion : MonoBehaviour
{
    private enum Estado
    {
        EsperarInicio,
        Seguir,
        BuscarBoton,
        IrAlBoton,
        MantenerBoton,
        ApoyarComoEscalon,
        IrARocaComoEscalon,
        EvaluarObstaculo,
        PosicionarseTrasCaja,
        EmpujarCaja,
        SubirCaja,
        EsperarAyuda
    }

    [Header("Seguimiento y sensores")]
    [SerializeField] private float distanciaInicio = 1.2f;
    [SerializeField] private float distanciaObjetivo = 0.95f;
    [SerializeField] private float radioBotones = 12f;
    [SerializeField] private float longitudSensorSuelo = 0.65f;
    [SerializeField] private float longitudSensorPared = 0.85f;
    [SerializeField] private float alcanceSaltoRoca = 3.2f;
    [SerializeField] private float alturaSaltoEfectiva = 2.2f;
    [SerializeField] private float radioBusquedaCaja = 10f;
    [SerializeField] private float tiempoAtasco = 0.3f;

    private MovimientoJugador movimiento;
    private MovimientoJugador humano;
    private PlayerPathTracker rutaJugador;
    private Rigidbody2D rb;
    private Collider2D colisionador;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Transform checadorSuelo;
    private LayerMask sueloMask;
    private LayerMask paredMask;
    private PhysicsMaterial2D materialMovimiento;

    private float velocidadMovimiento;
    private float fuerzaSalto;
    private float velocidadObjetivoX;
    private bool saltoPendiente;
    private float siguienteSalto;
    private float siguienteEmpuje;
    private float tiempoSinAvance;
    private float ultimoX;
    private float tiempoUltimoSaltoHumano;
    private int saltosHumanoCercaMuro;
    private float apoyoHasta;
    private float tiempoLejano;
    private float finEsperaBoton;
    private float direccionActual;
    private bool saltoCajaRealizado;
    private bool segundoSaltoCajaRealizado;

    private Vector2 posicionInicialJugador;
    private Vector2 destinoCaja;
    private Estado estado = Estado.EsperarInicio;
    private InterruptorElectrico botonObjetivo;
    private CajaPesada cajaObjetivo;

    private static readonly int SpeedParameter = Animator.StringToHash("Speed");
    private static readonly int IsGroundedParameter = Animator.StringToHash("IsGrounded");
    private static readonly int EnSueloParameter = Animator.StringToHash("enSuelo");

    public void Configurar(MovimientoJugador jugadorHumano)
    {
        DesuscribirHumano();
        humano = jugadorHumano;
        InicializarComponentes();

        rutaJugador = humano == null ? null : humano.GetComponent<PlayerPathTracker>();
        if (humano != null)
        {
            humano.SaltoEjecutado += RegistrarSaltoHumano;
            posicionInicialJugador = humano.transform.position;
        }

        estado = Estado.EsperarInicio;
        botonObjetivo = null;
        cajaObjetivo = null;
        tiempoLejano = 0f;
        tiempoSinAvance = 0f;
        apoyoHasta = 0f;
        saltosHumanoCercaMuro = 0;
        velocidadObjetivoX = 0f;
        saltoPendiente = false;
        ultimoX = transform.position.x;
    }

    public void LlamarCompanero()
    {
        if (!PuedeActuar())
        {
            return;
        }

        botonObjetivo = null;
        cajaObjetivo = null;
        apoyoHasta = 0f;
        tiempoSinAvance = 0f;

        if (Vector2.Distance(transform.position, humano.transform.position) > 6f && humano.EnElSuelo)
        {
            ReposicionarCercaDeHumano();
            return;
        }

        estado = Estado.Seguir;
    }

    private void Awake()
    {
        InicializarComponentes();
    }

    private void InicializarComponentes()
    {
        movimiento = GetComponent<MovimientoJugador>();
        rb = GetComponent<Rigidbody2D>();
        colisionador = GetComponent<Collider2D>();
        animator = movimiento.AnimatorReferencia;
        spriteRenderer = movimiento.SpriteRendererReferencia;
        velocidadMovimiento = movimiento.Velocidad;
        fuerzaSalto = movimiento.FuerzaSalto;
        sueloMask = LayerMask.GetMask("Ground", "Box", "Suelo");
        paredMask = LayerMask.GetMask("Ground", "Box");
        checadorSuelo = movimiento.ChecadorSuelo;
        materialMovimiento = movimiento.MaterialMovimiento;
        ConfigurarColliderFisico();
    }

    private void OnDisable()
    {
        DesuscribirHumano();
        velocidadObjetivoX = 0f;
        saltoPendiente = false;
    }

    private void DesuscribirHumano()
    {
        if (humano != null)
        {
            humano.SaltoEjecutado -= RegistrarSaltoHumano;
        }
    }

    private void Update()
    {
        if (!PuedeActuar() || Time.timeScale == 0f)
        {
            velocidadObjetivoX = 0f;
            return;
        }

        if (VerificarYEjecutarRescate())
        {
            ActualizarAnimacion();
            return;
        }

        ActualizarAtasco();
        EjecutarEstado();
        ActualizarAnimacion();
    }

    private void FixedUpdate()
    {
        if (!PuedeActuar() || rb == null)
        {
            return;
        }

        if (saltoPendiente)
        {
            saltoPendiente = false;
            if (EstaEnElSuelo() && Time.time >= siguienteSalto)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
                siguienteSalto = Time.time + 0.42f;
            }
        }

        rb.linearVelocity = new Vector2(velocidadObjetivoX, rb.linearVelocity.y);
    }

    private void EjecutarEstado()
    {
        if (movimiento.Tipo == MovimientoJugador.Protagonista.Roca &&
            Time.time < apoyoHasta &&
            estado != Estado.EmpujarCaja)
        {
            estado = Estado.ApoyarComoEscalon;
        }

        switch (estado)
        {
            case Estado.EsperarInicio:
                AplicarMovimiento(0f, false);
                if (Vector2.Distance(humano.transform.position, posicionInicialJugador) >= distanciaInicio ||
                    ObtenerSalidaCercanaAlHumano() != null)
                {
                    estado = movimiento.Tipo == MovimientoJugador.Protagonista.Roca
                        ? Estado.EvaluarObstaculo
                        : Estado.BuscarBoton;
                }
                break;

            case Estado.Seguir:
                EjecutarSeguimiento();
                break;

            case Estado.BuscarBoton:
                if (!BuscarBoton())
                {
                    estado = Estado.Seguir;
                }
                break;

            case Estado.IrAlBoton:
                EjecutarIrAlBoton();
                break;

            case Estado.MantenerBoton:
                EjecutarMantenerBoton();
                break;

            case Estado.ApoyarComoEscalon:
                EjecutarApoyoComoEscalon();
                break;

            case Estado.IrARocaComoEscalon:
                EjecutarEscalonDeRoca();
                break;

            case Estado.EvaluarObstaculo:
                EvaluarObstaculoRoca();
                break;

            case Estado.PosicionarseTrasCaja:
                EjecutarPosicionTrasCaja();
                break;

            case Estado.EmpujarCaja:
                EjecutarEmpujeCaja();
                break;

            case Estado.SubirCaja:
                EjecutarSubirCaja();
                break;

            case Estado.EsperarAyuda:
                EjecutarEsperaAyuda();
                break;
        }
    }

    private void EjecutarSeguimiento()
    {
        Transform salidaCercana = ObtenerSalidaCercanaAlHumano();
        if (salidaCercana != null)
        {
            float difSalida = salidaCercana.position.x - transform.position.x;
            float dirSalida = Mathf.Abs(difSalida) > 0.25f ? Mathf.Sign(difSalida) : 0f;
            bool saltarSalida = DetectarObstaculo(dirSalida) || DetectarHueco(dirSalida);
            AplicarMovimiento(dirSalida, saltarSalida);
            return;
        }

        if (movimiento.Tipo == MovimientoJugador.Protagonista.Chispa && BuscarBoton())
        {
            return;
        }

        if (movimiento.Tipo == MovimientoJugador.Protagonista.Roca && ObstaculoBloqueante())
        {
            estado = Estado.EvaluarObstaculo;
            AplicarMovimiento(0f, false);
            return;
        }

        float diferencia = humano.transform.position.x - transform.position.x;
        float direccion = Mathf.Abs(diferencia) > distanciaObjetivo ? Mathf.Sign(diferencia) : 0f;

        PlayerPathPoint punto = default;
        bool tienePunto = rutaJugador != null && rutaJugador.TryPeek(out punto);
        bool saltoMarcado = tienePunto && punto.SaltoAqui &&
                            Mathf.Abs(punto.Posicion.x - transform.position.x) < 0.65f;

        bool saltar = saltoMarcado || DetectarObstaculo(direccion) || DetectarHueco(direccion);
        AplicarMovimiento(direccion, saltar);

        if (tienePunto && Vector2.Distance(transform.position, punto.Posicion) <= distanciaObjetivo + 0.4f)
        {
            rutaJugador.TryDequeue(out punto);
        }
    }

    private Transform ObtenerSalidaCercanaAlHumano()
    {
        SalidaCooperativa salidaCoop = FindAnyObjectByType<SalidaCooperativa>();
        if (salidaCoop != null && Vector2.Distance(humano.transform.position, salidaCoop.transform.position) <= 3.2f)
        {
            return salidaCoop.transform;
        }

        SalidaVictoria salidaVic = FindAnyObjectByType<SalidaVictoria>();
        if (salidaVic != null && Vector2.Distance(humano.transform.position, salidaVic.transform.position) <= 3.2f)
        {
            return salidaVic.transform;
        }

        return null;
    }

    private bool BuscarBoton()
    {
        if (botonObjetivo != null && botonObjetivo.Activado)
        {
            finEsperaBoton = Time.time + 0.4f;
            estado = Estado.MantenerBoton;
            return true;
        }

        InterruptorElectrico[] botones = FindObjectsByType<InterruptorElectrico>(FindObjectsInactive.Exclude);
        float mejorDistancia = float.MaxValue;
        InterruptorElectrico mejor = null;
        foreach (InterruptorElectrico boton in botones)
        {
            if (boton == null || boton.Activado)
            {
                continue;
            }

            float distancia = Vector2.Distance(humano.transform.position, boton.transform.position);
            if (distancia <= radioBotones && distancia < mejorDistancia)
            {
                mejor = boton;
                mejorDistancia = distancia;
            }
        }

        if (mejor == null)
        {
            return false;
        }

        botonObjetivo = mejor;
        estado = Estado.IrAlBoton;
        return true;
    }

    private void EjecutarIrAlBoton()
    {
        if (botonObjetivo == null || botonObjetivo.Activado)
        {
            botonObjetivo = null;
            estado = Estado.Seguir;
            return;
        }

        float altura = botonObjetivo.transform.position.y - transform.position.y;
        float saltoNormal = AlturaSaltoNormal();
        bool requiereEscalon = altura > saltoNormal &&
                               altura <= saltoNormal * 1.9f &&
                               Vector2.Distance(transform.position, humano.transform.position) <= 2.6f;

        if (requiereEscalon && movimiento.Tipo == MovimientoJugador.Protagonista.Chispa)
        {
            estado = Estado.IrARocaComoEscalon;
            return;
        }

        float direccion = DireccionHacia(botonObjetivo.transform.position.x);
        bool saltar = DetectarObstaculo(direccion) || DetectarHueco(direccion) || Atascado();
        AplicarMovimiento(direccion, saltar);

        Collider2D colliderBoton = botonObjetivo.GetComponent<Collider2D>();
        if (colliderBoton != null &&
            Vector2.Distance(colliderBoton.ClosestPoint(transform.position), transform.position) <= 0.9f)
        {
            AplicarMovimiento(0f, false);
            botonObjetivo.ActivarDesdeJugador(movimiento);
            finEsperaBoton = Time.time + 0.4f;
            estado = Estado.MantenerBoton;
        }
    }

    private void EjecutarMantenerBoton()
    {
        AplicarMovimiento(0f, false);
        if (botonObjetivo == null || Time.time >= finEsperaBoton)
        {
            botonObjetivo = null;
            estado = Estado.Seguir;
        }
    }

    private void EjecutarApoyoComoEscalon()
    {
        if (Time.time >= apoyoHasta)
        {
            estado = Estado.Seguir;
            return;
        }

        float distanciaX = humano.transform.position.x - transform.position.x;
        if (Mathf.Abs(distanciaX) > 0.55f)
        {
            float dir = Mathf.Sign(distanciaX);
            AplicarMovimiento(dir, DetectarObstaculo(dir) || DetectarHueco(dir));
        }
        else
        {
            AplicarMovimiento(0f, false);
        }
    }

    private void EjecutarEscalonDeRoca()
    {
        if (botonObjetivo == null || botonObjetivo.Activado)
        {
            estado = Estado.Seguir;
            return;
        }

        float diferencia = humano.transform.position.x - transform.position.x;
        bool sobreRoca = Mathf.Abs(diferencia) < 0.5f &&
                         transform.position.y > humano.transform.position.y + 0.25f;

        if (!sobreRoca)
        {
            float dir = DireccionHacia(humano.transform.position.x);
            bool saltarSobreRoca = Mathf.Abs(diferencia) < 1.35f || Atascado();
            AplicarMovimiento(dir, saltarSobreRoca);
            return;
        }

        AplicarMovimiento(DireccionHacia(botonObjetivo.transform.position.x), true);
        if (transform.position.y > botonObjetivo.transform.position.y - 0.6f)
        {
            estado = Estado.IrAlBoton;
        }
    }

    private void EvaluarObstaculoRoca()
    {
        if (movimiento.Tipo != MovimientoJugador.Protagonista.Roca)
        {
            estado = Estado.Seguir;
            return;
        }

        float direccion = DireccionHacia(humano.transform.position.x);
        bool huecoAncho = HuecoExcedeAlcance(direccion);
        bool muroAlto = EsMuroAlto(direccion);
        if (!huecoAncho && !muroAlto)
        {
            estado = Estado.Seguir;
            return;
        }

        cajaObjetivo = BuscarCajaEnPlataforma(direccion);
        if (cajaObjetivo == null)
        {
            estado = Estado.EsperarAyuda;
            AplicarMovimiento(0f, false);
            return;
        }

        destinoCaja = cajaObjetivo.transform.position - Vector3.right * Mathf.Sign(direccion) * 0.95f;
        saltoCajaRealizado = false;
        segundoSaltoCajaRealizado = false;
        estado = Estado.PosicionarseTrasCaja;
    }

    private void EjecutarPosicionTrasCaja()
    {
        if (cajaObjetivo == null)
        {
            estado = Estado.EvaluarObstaculo;
            return;
        }

        float direccion = DireccionHacia(destinoCaja.x);
        bool ladoIncorrecto = Mathf.Sign(cajaObjetivo.transform.position.x - transform.position.x) !=
                              Mathf.Sign(humano.transform.position.x - transform.position.x);

        if (ladoIncorrecto && Mathf.Abs(cajaObjetivo.transform.position.x - transform.position.x) < 1.2f)
        {
            AplicarMovimiento(Mathf.Sign(humano.transform.position.x - transform.position.x), true);
            return;
        }

        AplicarMovimiento(direccion, Atascado());
        if (Mathf.Abs(transform.position.x - destinoCaja.x) < 0.25f)
        {
            estado = Estado.EmpujarCaja;
        }
    }

    private void EjecutarEmpujeCaja()
    {
        if (cajaObjetivo == null)
        {
            estado = Estado.EvaluarObstaculo;
            return;
        }

        float direccion = Mathf.Sign(humano.transform.position.x - transform.position.x);
        float distancia = Mathf.Abs(cajaObjetivo.transform.position.x - transform.position.x);
        if (distancia > 1.5f)
        {
            estado = Estado.PosicionarseTrasCaja;
            return;
        }

        movimiento.EstablecerEntradaIA(direccion, false);
        AplicarMovimiento(direccion, false);

        if (Time.time >= siguienteEmpuje)
        {
            movimiento.DispararHabilidad();
            siguienteEmpuje = Time.time + 0.22f;
        }

        bool cercaDeHumano = Mathf.Abs(cajaObjetivo.transform.position.x - humano.transform.position.x) <= 1.2f;
        if (CajaFlushContraObstaculo(direccion) || CajaEnBorde(direccion) || cercaDeHumano)
        {
            estado = Estado.SubirCaja;
            saltoCajaRealizado = false;
            segundoSaltoCajaRealizado = false;
        }
    }

    private void EjecutarSubirCaja()
    {
        if (cajaObjetivo == null)
        {
            estado = Estado.Seguir;
            return;
        }

        float direccion = Mathf.Sign(humano.transform.position.x - transform.position.x);
        if (!saltoCajaRealizado)
        {
            AplicarMovimiento(direccion, true);
            saltoCajaRealizado = true;
            return;
        }

        bool sobreCaja = transform.position.y > cajaObjetivo.transform.position.y + 0.4f;
        if (sobreCaja && !segundoSaltoCajaRealizado && EsMuroAlto(direccion))
        {
            AplicarMovimiento(direccion, true);
            segundoSaltoCajaRealizado = true;
            return;
        }

        if (segundoSaltoCajaRealizado || (sobreCaja && !EsMuroAlto(direccion)))
        {
            cajaObjetivo = null;
            estado = Estado.Seguir;
        }
        else
        {
            AplicarMovimiento(direccion, Atascado());
        }
    }

    private void EjecutarEsperaAyuda()
    {
        AplicarMovimiento(0f, false);
        float direccion = DireccionHacia(humano.transform.position.x);
        if (HaySueloDelante(direccion) && !HuecoExcedeAlcance(direccion))
        {
            estado = Estado.Seguir;
        }
    }

    private bool VerificarYEjecutarRescate()
    {
        float distancia = Vector2.Distance(transform.position, humano.transform.position);
        if (distancia > 14f || transform.position.y < humano.transform.position.y - 5f)
        {
            tiempoLejano += Time.deltaTime;
        }
        else
        {
            tiempoLejano = 0f;
        }

        if (tiempoLejano >= 2.2f)
        {
            AplicarMovimiento(0f, false);
            if (humano.EnElSuelo)
            {
                ReposicionarCercaDeHumano();
            }
            return true;
        }

        return false;
    }

    private void ReposicionarCercaDeHumano()
    {
        SpriteRenderer spriteHumano = humano.SpriteRendererReferencia;
        float mirando = spriteHumano != null && spriteHumano.flipX ? -1f : 1f;
        Vector2 destino = (Vector2)humano.transform.position - Vector2.right * mirando * 0.85f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = destino;
        }
        else
        {
            transform.position = destino;
        }

        if (rutaJugador != null)
        {
            rutaJugador.Limpiar();
        }

        tiempoLejano = 0f;
        tiempoSinAvance = 0f;
        botonObjetivo = null;
        cajaObjetivo = null;
        estado = Estado.Seguir;
    }

    private bool ObstaculoBloqueante()
    {
        float direccion = DireccionHacia(humano.transform.position.x);
        return direccion != 0f && (HuecoExcedeAlcance(direccion) || EsMuroAlto(direccion));
    }

    private bool HaySueloDelante(float direccion)
    {
        if (colisionador == null || Mathf.Abs(direccion) < 0.01f)
        {
            return true;
        }

        Bounds bounds = colisionador.bounds;
        float sentido = Mathf.Sign(direccion);
        Vector2 origen = new Vector2(bounds.center.x + sentido * (bounds.extents.x + 0.1f), bounds.min.y + 0.06f);
        if (HayPeligroEn(origen + Vector2.down * 0.25f))
        {
            return false;
        }

        return EsHitValido(Physics2D.Raycast(origen, Vector2.down, longitudSensorSuelo, paredMask));
    }

    private bool DetectarHueco(float direccion)
    {
        if (colisionador == null || Mathf.Abs(direccion) < 0.01f)
        {
            return false;
        }

        Bounds bounds = colisionador.bounds;
        float sentido = Mathf.Sign(direccion);
        Vector2 origen = new Vector2(bounds.center.x + sentido * (bounds.extents.x + 0.16f), bounds.min.y + 0.08f);
        if (HayPeligroEn(origen + Vector2.down * 0.25f))
        {
            return true;
        }

        return !EsHitValido(Physics2D.Raycast(origen, Vector2.down, 0.85f, paredMask));
    }

    private bool HayPeligroEn(Vector2 punto)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(punto, 0.3f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && (hits[i].GetComponent<ZonaMuerte2D>() != null || hits[i].CompareTag("Hazard")))
            {
                return true;
            }
        }

        return false;
    }

    private bool HuecoExcedeAlcance(float direccion)
    {
        if (!DetectarHueco(direccion))
        {
            return false;
        }

        Bounds bounds = colisionador.bounds;
        float sentido = Mathf.Sign(direccion);
        for (float distancia = 0.2f; distancia <= alcanceSaltoRoca; distancia += 0.2f)
        {
            Vector2 origen = new Vector2(bounds.center.x + sentido * (bounds.extents.x + distancia), bounds.min.y + 0.08f);
            if (!HayPeligroEn(origen + Vector2.down * 0.25f) &&
                EsHitValido(Physics2D.Raycast(origen, Vector2.down, longitudSensorSuelo, paredMask)))
            {
                return false;
            }
        }

        return true;
    }

    private bool DetectarObstaculo(float direccion)
    {
        if (colisionador == null || Mathf.Abs(direccion) < 0.01f)
        {
            return false;
        }

        Bounds bounds = colisionador.bounds;
        float sentido = Mathf.Sign(direccion);
        Vector2 tobillo = new Vector2(bounds.center.x + sentido * (bounds.extents.x + 0.04f), bounds.min.y + 0.16f);
        Vector2 pecho = new Vector2(bounds.center.x + sentido * (bounds.extents.x + 0.04f), bounds.center.y + bounds.extents.y * 0.35f);
        return EsHitValido(Physics2D.Raycast(tobillo, Vector2.right * sentido, longitudSensorPared, paredMask)) ||
               EsHitValido(Physics2D.Raycast(pecho, Vector2.right * sentido, longitudSensorPared, paredMask));
    }

    private bool EsMuroAlto(float direccion)
    {
        if (colisionador == null || Mathf.Abs(direccion) < 0.01f)
        {
            return false;
        }

        Bounds bounds = colisionador.bounds;
        float sentido = Mathf.Sign(direccion);
        Vector2 origen = new Vector2(bounds.center.x + sentido * (bounds.extents.x + 0.04f), bounds.center.y);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.right * sentido, longitudSensorPared, paredMask);
        if (!EsHitValido(hit))
        {
            return false;
        }

        return hit.collider.bounds.max.y - bounds.min.y > alturaSaltoEfectiva;
    }

    private CajaPesada BuscarCajaEnPlataforma(float direccion)
    {
        CajaPesada[] cajas = FindObjectsByType<CajaPesada>(FindObjectsInactive.Exclude);
        CajaPesada mejor = null;
        float mejorDistancia = float.MaxValue;
        foreach (CajaPesada caja in cajas)
        {
            if (caja == null || caja.GetComponent<Rigidbody2D>() == null)
            {
                continue;
            }

            float distanciaX = Mathf.Abs(caja.transform.position.x - transform.position.x);
            float distanciaY = Mathf.Abs(caja.transform.position.y - transform.position.y);
            if (distanciaX > radioBusquedaCaja || distanciaY > 3f)
            {
                continue;
            }

            if (Mathf.Sign(caja.transform.position.x - transform.position.x) != Mathf.Sign(direccion))
            {
                continue;
            }

            if (distanciaX < mejorDistancia)
            {
                mejor = caja;
                mejorDistancia = distanciaX;
            }
        }

        return mejor;
    }

    private bool CajaFlushContraObstaculo(float direccion)
    {
        Collider2D cajaCollider = cajaObjetivo.GetComponent<Collider2D>();
        if (cajaCollider == null)
        {
            return false;
        }

        float sentido = Mathf.Sign(direccion);
        Vector2 origen = new Vector2(cajaCollider.bounds.max.x + 0.02f, cajaCollider.bounds.center.y);
        if (sentido < 0f)
        {
            origen.x = cajaCollider.bounds.min.x - 0.02f;
        }

        return EsHitValido(Physics2D.Raycast(origen, Vector2.right * sentido, 0.25f, paredMask));
    }

    private bool CajaEnBorde(float direccion)
    {
        Collider2D cajaCollider = cajaObjetivo.GetComponent<Collider2D>();
        if (cajaCollider == null)
        {
            return false;
        }

        float sentido = Mathf.Sign(direccion);
        Vector2 origen = new Vector2(
            cajaCollider.bounds.center.x + sentido * (cajaCollider.bounds.extents.x + 0.1f),
            cajaCollider.bounds.min.y + 0.05f);
        return !EsHitValido(Physics2D.Raycast(origen, Vector2.down, longitudSensorSuelo, paredMask));
    }

    private float AlturaSaltoNormal()
    {
        return Mathf.Min(alturaSaltoEfectiva, fuerzaSalto * fuerzaSalto / (2f * Mathf.Abs(Physics2D.gravity.y)));
    }

    private float DireccionHacia(float x)
    {
        float diferencia = x - transform.position.x;
        return Mathf.Abs(diferencia) > 0.12f ? Mathf.Sign(diferencia) : 0f;
    }

    private void AplicarMovimiento(float dirX, bool saltar)
    {
        direccionActual = Mathf.Clamp(dirX, -1f, 1f);
        bool hueco = direccionActual != 0f && DetectarHueco(direccionActual);
        bool saltoPermitido = movimiento.Tipo == MovimientoJugador.Protagonista.Chispa ||
                              !HuecoExcedeAlcance(direccionActual);

        if (direccionActual != 0f && !HaySueloDelante(direccionActual) && (!hueco || !saltoPermitido))
        {
            direccionActual = 0f;
        }

        if (Atascado() && estado != Estado.EmpujarCaja && estado != Estado.ApoyarComoEscalon)
        {
            saltar = true;
        }

        if (saltar)
        {
            saltoPendiente = true;
        }

        velocidadObjetivoX = direccionActual * velocidadMovimiento;
        movimiento.EstablecerEntradaIA(direccionActual, false);
    }

    private void ActualizarAtasco()
    {
        if (estado == Estado.EmpujarCaja ||
            estado == Estado.ApoyarComoEscalon ||
            Mathf.Abs(direccionActual) < 0.01f ||
            !EstaEnElSuelo())
        {
            tiempoSinAvance = 0f;
            ultimoX = transform.position.x;
            return;
        }

        if (Mathf.Abs(transform.position.x - ultimoX) < 0.01f && Mathf.Abs(rb.linearVelocity.x) < 0.1f)
        {
            tiempoSinAvance += Time.deltaTime;
        }
        else
        {
            tiempoSinAvance = 0f;
        }

        ultimoX = transform.position.x;
    }

    private bool Atascado()
    {
        return tiempoSinAvance >= tiempoAtasco;
    }

    private void RegistrarSaltoHumano(MovimientoJugador jugador)
    {
        if (movimiento.Tipo != MovimientoJugador.Protagonista.Roca ||
            humano.Tipo != MovimientoJugador.Protagonista.Chispa ||
            !HumanoCercaDeMuro())
        {
            return;
        }

        if (Time.time - tiempoUltimoSaltoHumano > 2.5f)
        {
            saltosHumanoCercaMuro = 0;
        }

        tiempoUltimoSaltoHumano = Time.time;
        saltosHumanoCercaMuro++;
        if (saltosHumanoCercaMuro >= 2)
        {
            apoyoHasta = Time.time + 3.5f;
            saltosHumanoCercaMuro = 0;
        }
    }

    private bool HumanoCercaDeMuro()
    {
        SpriteRenderer sprite = humano.SpriteRendererReferencia;
        float sentido = sprite != null && sprite.flipX ? -1f : 1f;
        return EsHitValido(Physics2D.Raycast(
            humano.transform.position + Vector3.up * 0.4f,
            Vector2.right * sentido,
            1.4f,
            paredMask));
    }

    private void ConfigurarColliderFisico()
    {
        BoxCollider2D box = colisionador as BoxCollider2D;
        if (box != null)
        {
            box.edgeRadius = 0f;
            if (materialMovimiento != null)
            {
                box.sharedMaterial = materialMovimiento;
            }
        }
    }

    private bool EstaEnElSuelo()
    {
        if (checadorSuelo == null)
        {
            return false;
        }

        Vector2 centro = (Vector2)checadorSuelo.position + Vector2.down * 0.03f;
        Collider2D hit = Physics2D.OverlapBox(centro, new Vector2(0.45f, 0.18f), 0f, sueloMask);
        return EsHitValido(hit);
    }

    private bool EsHitValido(RaycastHit2D hit)
    {
        return hit.collider != null && !hit.collider.isTrigger && hit.collider.attachedRigidbody != rb;
    }

    private bool EsHitValido(Collider2D hit)
    {
        return hit != null && !hit.isTrigger && hit.attachedRigidbody != rb;
    }

    private void ActualizarAnimacion()
    {
        if (animator != null)
        {
            if (TieneParametro(SpeedParameter, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(SpeedParameter, Mathf.Abs(rb.linearVelocity.x));
            }

            bool grounded = EstaEnElSuelo();
            if (TieneParametro(IsGroundedParameter, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(IsGroundedParameter, grounded);
            }

            if (TieneParametro(EnSueloParameter, AnimatorControllerParameterType.Bool))
            {
                animator.SetBool(EnSueloParameter, grounded);
            }
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

    private bool TieneParametro(int hash, AnimatorControllerParameterType tipo)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.nameHash == hash && parametro.type == tipo)
            {
                return true;
            }
        }

        return false;
    }

    private bool PuedeActuar()
    {
        return enabled && movimiento != null && humano != null && rb != null &&
               humano.gameObject.activeInHierarchy && !movimiento.ControlHumano;
    }
}