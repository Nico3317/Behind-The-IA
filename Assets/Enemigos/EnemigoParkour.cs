using UnityEngine;

// Enemigo que va y viene sobre una plataforma del parkour.
// No usa gravedad: se mueve entre dos puntos. El jugador tiene que saltarlo o esquivarlo.
public class EnemigoParkour : Enemigo
{
    [Header("Recorrido")]
    [Tooltip("Punto A del recorrido. Si lo crea el GeneradorDeCuadra, se configura solo.")]
    public Transform puntoA;
    [Tooltip("Punto B del recorrido. Si lo crea el GeneradorDeCuadra, se configura solo.")]
    public Transform puntoB;

    [Header("Movimiento")]
    public float velocidad = 3f;
    public float velocidadGiro = 12f;
    [Tooltip("Segundos que se queda quieto en cada punta de la plataforma.")]
    public float pausaEnPuntas = 0.6f;

    [Header("Color")]
    public Color color = new Color(0.1f, 0.8f, 0.9f, 1f);

    private Vector3 posicionA;
    private Vector3 posicionB;
    private bool recorridoConfigurado;
    private bool yendoHaciaB = true;
    private float timerPausa;

    protected override void Start()
    {
        base.Start();
        CambiarColor(color);

        // El collider es trigger para que no empuje al jugador fuera de la plataforma: lo atrapa por contacto
        if (miCollider != null) miCollider.isTrigger = true;

        if (!recorridoConfigurado)
        {
            if (puntoA != null && puntoB != null)
            {
                posicionA = puntoA.position;
                posicionB = puntoB.position;
                recorridoConfigurado = true;
            }
            else
            {
                Debug.LogWarning("El enemigo " + name + " no tiene puntoA y puntoB asignados.");
            }
        }
    }

    public void ConfigurarRecorrido(Vector3 a, Vector3 b)
    {
        posicionA = a;
        posicionB = b;
        recorridoConfigurado = true;
        yendoHaciaB = true;
    }

    void Update()
    {
        if (recorridoConfigurado)
        {
            if (timerPausa > 0)
            {
                timerPausa -= Time.deltaTime;
            }
            else
            {
                Vector3 destino = yendoHaciaB ? posicionB : posicionA;
                MirarHacia(destino - transform.position, velocidadGiro);
                transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

                if (Vector3.Distance(transform.position, destino) < 0.05f)
                {
                    yendoHaciaB = !yendoHaciaB;
                    timerPausa = pausaEnPuntas;
                }
            }
        }

        if (TocaAlJugador())
        {
            AtraparJugador();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        if (Application.isPlaying && recorridoConfigurado)
        {
            Gizmos.DrawLine(posicionA, posicionB);
        }
        else if (puntoA != null && puntoB != null)
        {
            Gizmos.DrawLine(puntoA.position, puntoB.position);
        }
    }
}
