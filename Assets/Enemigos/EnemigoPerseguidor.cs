using UnityEngine;

// Enemigo que espera en su zona y persigue al jugador cuando entra en el area.
// Si el jugador se aleja lo suficiente, deja de perseguirlo y vuelve a su lugar.
[RequireComponent(typeof(CharacterController))]
public class EnemigoPerseguidor : Enemigo
{
    [Header("Area de deteccion")]
    [Tooltip("Si el jugador entra en este radio (medido desde donde aparece el enemigo), lo empieza a perseguir.")]
    public float radioDeteccion = 15f;

    [Tooltip("Si el jugador sale de este radio, el enemigo deja de perseguirlo y vuelve.")]
    public float radioPerdida = 30f;

    [Tooltip("Diferencia de altura maxima para detectar al jugador (asi no lo persigue si esta arriba de un edificio).")]
    public float alturaMaximaDeteccion = 4f;

    [Header("Movimiento")]
    public float velocidadPersecucion = 5.5f;
    public float velocidadRegreso = 3f;
    public float velocidadGiro = 10f;
    private float gravedad = -20f;

    [Header("Colores")]
    public Color colorTranquilo = new Color(1f, 0.55f, 0f, 1f);
    public Color colorAlerta = Color.red;

    private enum Estado { Esperando, Persiguiendo, Volviendo }
    private Estado estado = Estado.Esperando;

    private CharacterController control;
    private Vector3 posicionInicial;
    private float velocidadVertical;

    void Awake()
    {
        control = GetComponent<CharacterController>();
    }

    protected override void Start()
    {
        base.Start();
        posicionInicial = transform.position;
        CambiarColor(colorTranquilo);
    }

    void Update()
    {
        if (jugador == null) return;

        switch (estado)
        {
            case Estado.Esperando:
                if (JugadorEnArea(radioDeteccion))
                {
                    estado = Estado.Persiguiendo;
                    CambiarColor(colorAlerta);
                }
                Mover(Vector3.zero, 0f);
                break;

            case Estado.Persiguiendo:
                if (!JugadorEnArea(radioPerdida))
                {
                    estado = Estado.Volviendo;
                    CambiarColor(colorTranquilo);
                    break;
                }
                Mover(CentroJugador() - transform.position, velocidadPersecucion);

                if (TocaAlJugador())
                {
                    AtraparJugador();
                }
                break;

            case Estado.Volviendo:
                if (JugadorEnArea(radioDeteccion))
                {
                    estado = Estado.Persiguiendo;
                    CambiarColor(colorAlerta);
                    break;
                }

                Vector3 haciaInicio = posicionInicial - transform.position;
                haciaInicio.y = 0;
                if (haciaInicio.magnitude < 0.5f)
                {
                    estado = Estado.Esperando;
                    Mover(Vector3.zero, 0f);
                }
                else
                {
                    Mover(haciaInicio, velocidadRegreso);
                }
                break;
        }
    }

    private bool JugadorEnArea(float radio)
    {
        Vector3 diferencia = CentroJugador() - posicionInicial;
        if (Mathf.Abs(diferencia.y) > alturaMaximaDeteccion + control.height) return false;
        diferencia.y = 0;
        return diferencia.magnitude <= radio;
    }

    private void Mover(Vector3 direccion, float velocidad)
    {
        direccion.y = 0;
        Vector3 movimiento = Vector3.zero;

        if (direccion.sqrMagnitude > 0.01f)
        {
            movimiento = direccion.normalized * velocidad;
            MirarHacia(direccion, velocidadGiro);
        }

        if (control.isGrounded && velocidadVertical < 0) velocidadVertical = -2f;
        velocidadVertical += gravedad * Time.deltaTime;
        movimiento.y = velocidadVertical;

        control.Move(movimiento * Time.deltaTime);
    }

    protected override void AlAtraparJugador()
    {
        estado = Estado.Volviendo;
        CambiarColor(colorTranquilo);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 centro = Application.isPlaying ? posicionInicial : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centro, radioDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centro, radioPerdida);
    }
}
