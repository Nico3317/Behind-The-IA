using System.Collections.Generic;
using UnityEngine;

// Enemigo que recorre una ruta de puntos en loop (por las calles de la ciudad).
// Si ve al jugador lo sigue a distancia y le dispara. Si lo pierde de vista, vuelve a patrullar.
[RequireComponent(typeof(CharacterController))]
public class EnemigoPatrulla : Enemigo
{
    [Header("Ruta")]
    [Tooltip("Puntos de la ruta. Si se crea desde el GeneradorDeCuadra, la ruta se arma sola.")]
    public List<Transform> puntosDePatrulla = new List<Transform>();

    [Tooltip("Si esta activado va y vuelve por la ruta. Si no, da vueltas en circulo.")]
    public bool idaYVuelta = false;

    [Header("Movimiento")]
    public float velocidad = 4f;
    public float velocidadGiro = 8f;
    [Tooltip("Segundos que se queda quieto en cada punto.")]
    public float esperaEnPunto = 0.5f;
    [Tooltip("Si se traba este tiempo sin llegar al punto, pasa al siguiente.")]
    public float tiempoMaximoTrabado = 3f;
    private float gravedad = -20f;

    [Header("Vision y ataque")]
    [Tooltip("Distancia a la que ve al jugador (si no hay paredes en el medio).")]
    public float radioVision = 20f;
    [Tooltip("Si se aleja mas que esto de su ruta, deja de seguir al jugador.")]
    public float distanciaMaximaDeRuta = 30f;
    [Tooltip("Segundos sin ver al jugador para volver a patrullar.")]
    public float tiempoParaOlvidar = 2.5f;
    [Tooltip("No se acerca al jugador mas que esta distancia.")]
    public float distanciaDeAtaque = 8f;
    public float velocidadAtaque = 5f;
    public float tiempoEntreDisparos = 1.2f;
    public float velocidadDisparo = 14f;
    public int danioDisparo = 20;

    [Header("Color")]
    public Color color = new Color(0.6f, 0.1f, 0.9f, 1f);
    public Color colorAlerta = new Color(0.9f, 0.1f, 0.2f, 1f);

    private bool atacando;
    private bool volviendo;
    private Vector3 puntoDeSalida;
    private float timerOlvido;
    private float timerDisparo;

    private CharacterController control;
    private List<Vector3> ruta = new List<Vector3>();
    private int indiceActual;
    private int sentido = 1;
    private float timerEspera;
    private float timerTrabado;
    private float distanciaAnterior;
    private float velocidadVertical;

    void Awake()
    {
        control = GetComponent<CharacterController>();
    }

    protected override void Start()
    {
        base.Start();
        CambiarColor(color);

        // Si no la configuro el generador, se usa la ruta del Inspector
        if (ruta.Count == 0)
        {
            foreach (Transform punto in puntosDePatrulla)
            {
                if (punto != null) ruta.Add(punto.position);
            }
        }

        if (ruta.Count == 0)
        {
            Debug.LogWarning("El enemigo " + name + " no tiene puntos de patrulla.");
        }
        distanciaAnterior = Mathf.Infinity;
    }

    public void ConfigurarRuta(List<Vector3> puntos, int indiceInicial)
    {
        ruta = new List<Vector3>(puntos);
        indiceActual = ruta.Count > 0 ? Mathf.Clamp(indiceInicial, 0, ruta.Count - 1) : 0;
        distanciaAnterior = Mathf.Infinity;
    }

    void Update()
    {
        Vector3 movimiento = Vector3.zero;
        ActualizarVision();

        if (atacando)
        {
            movimiento = Atacar();
        }
        else if (volviendo)
        {
            // Vuelve al lugar donde dejo la ruta (si se traba, sigue con la ruta)
            Vector3 haciaRuta = puntoDeSalida - transform.position;
            haciaRuta.y = 0;
            if (haciaRuta.magnitude < 0.5f)
            {
                volviendo = false;
                timerTrabado = 0;
                distanciaAnterior = Mathf.Infinity;
            }
            else
            {
                movimiento = haciaRuta.normalized * velocidad;
                MirarHacia(haciaRuta, velocidadGiro);
                ControlarSiEstaTrabado(haciaRuta.magnitude);
            }
        }
        else if (ruta.Count > 0)
        {
            if (timerEspera > 0)
            {
                timerEspera -= Time.deltaTime;
            }
            else
            {
                Vector3 haciaPunto = ruta[indiceActual] - transform.position;
                haciaPunto.y = 0;
                float distancia = haciaPunto.magnitude;

                if (distancia < 0.5f)
                {
                    SiguientePunto();
                    timerEspera = esperaEnPunto;
                }
                else
                {
                    movimiento = haciaPunto.normalized * velocidad;
                    MirarHacia(haciaPunto, velocidadGiro);
                    ControlarSiEstaTrabado(distancia);
                }
            }
        }

        if (control.isGrounded && velocidadVertical < 0) velocidadVertical = -2f;
        velocidadVertical += gravedad * Time.deltaTime;
        movimiento.y = velocidadVertical;
        control.Move(movimiento * Time.deltaTime);

        if (TocaAlJugador())
        {
            AtraparJugador();
        }
    }

    private bool VeAlJugador()
    {
        if (jugador == null || MenuMuerte.Activo) return false;
        if (reaparecer != null && reaparecer.EsInvulnerable) return false;

        Vector3 ojo = OrigenDisparo();
        Vector3 haciaJugador = CentroJugador() - ojo;
        if (haciaJugador.magnitude > radioVision) return false;

        // Si hay una pared en el medio no lo ve
        foreach (RaycastHit hit in Physics.RaycastAll(ojo, haciaJugador.normalized, haciaJugador.magnitude, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.IsChildOf(jugador)) continue;
            if (hit.transform.GetComponentInParent<Enemigo>() != null) continue;
            return false;
        }
        return true;
    }

    private Vector3 OrigenDisparo()
    {
        return transform.position + Vector3.up * 0.2f;
    }

    private void ActualizarVision()
    {
        bool loVe = VeAlJugador();

        if (!atacando)
        {
            if (!loVe) return;
            if (volviendo && LejosDeLaRuta()) return;

            atacando = true;
            if (!volviendo) puntoDeSalida = transform.position;
            volviendo = false;
            timerOlvido = 0;
            timerDisparo = tiempoEntreDisparos * 0.5f; // un respiro antes del primer disparo
            CambiarColor(colorAlerta);
            return;
        }

        timerOlvido = loVe ? 0 : timerOlvido + Time.deltaTime;
        if (timerOlvido >= tiempoParaOlvidar || LejosDeLaRuta())
        {
            atacando = false;
            volviendo = true;
            timerTrabado = 0;
            distanciaAnterior = Mathf.Infinity;
            CambiarColor(color);
        }
    }

    private bool LejosDeLaRuta()
    {
        Vector3 diferencia = transform.position - puntoDeSalida;
        diferencia.y = 0;
        return diferencia.magnitude > distanciaMaximaDeRuta;
    }

    private Vector3 Atacar()
    {
        Vector3 haciaJugador = CentroJugador() - transform.position;
        haciaJugador.y = 0;
        MirarHacia(haciaJugador, velocidadGiro);

        // Solo dispara si en este momento lo esta viendo
        timerDisparo -= Time.deltaTime;
        if (timerDisparo <= 0 && timerOlvido <= 0)
        {
            timerDisparo = tiempoEntreDisparos;
            Vector3 origen = OrigenDisparo();
            DisparoEnemigo.Crear(origen, CentroJugador() - origen, velocidadDisparo, danioDisparo, jugador, jugadorCC);
        }

        if (haciaJugador.magnitude <= distanciaDeAtaque) return Vector3.zero;
        return haciaJugador.normalized * velocidadAtaque;
    }

    private void ControlarSiEstaTrabado(float distancia)
    {
        if (distancia < distanciaAnterior - 0.05f)
        {
            distanciaAnterior = distancia;
            timerTrabado = 0;
            return;
        }

        timerTrabado += Time.deltaTime;
        if (timerTrabado >= tiempoMaximoTrabado)
        {
            volviendo = false;
            SiguientePunto();
        }
    }

    private void SiguientePunto()
    {
        timerTrabado = 0;
        distanciaAnterior = Mathf.Infinity;

        if (ruta.Count <= 1) return;

        if (idaYVuelta)
        {
            if (indiceActual + sentido >= ruta.Count || indiceActual + sentido < 0) sentido = -sentido;
            indiceActual += sentido;
        }
        else
        {
            indiceActual = (indiceActual + 1) % ruta.Count;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        if (Application.isPlaying)
        {
            for (int i = 0; i < ruta.Count; i++)
            {
                Gizmos.DrawSphere(ruta[i], 0.4f);
                Gizmos.DrawLine(ruta[i], ruta[(i + 1) % ruta.Count]);
            }
        }
        else
        {
            for (int i = 0; i < puntosDePatrulla.Count; i++)
            {
                if (puntosDePatrulla[i] == null) continue;
                Gizmos.DrawSphere(puntosDePatrulla[i].position, 0.4f);
                Transform siguiente = puntosDePatrulla[(i + 1) % puntosDePatrulla.Count];
                if (siguiente != null) Gizmos.DrawLine(puntosDePatrulla[i].position, siguiente.position);
            }
        }
    }
}
