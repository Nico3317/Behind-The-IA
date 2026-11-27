using UnityEngine;

public class ReaparecerJugador : MonoBehaviour
{
    [Header("Reaparicion")]
    [Tooltip("Punto donde reaparece el jugador. Si se deja vacio, reaparece donde empezo la partida.")]
    public Transform puntoDeReaparicion;

    [Tooltip("Segundos en los que los enemigos no pueden volver a atrapar al jugador despues de reaparecer.")]
    public float tiempoInvulnerable = 2f;

    [Tooltip("Si esta activado, al morir aparece el menu con 'Volver a jugar'. Si no, reaparece directamente.")]
    public bool mostrarMenuAlMorir = true;

    private CharacterController characterController;
    private Vector3 posicionInicial;
    private float timerInvulnerable;

    public bool EsInvulnerable
    {
        get { return timerInvulnerable > 0; }
    }

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (timerInvulnerable > 0) timerInvulnerable -= Time.deltaTime;
    }

    // Al guardar o cargar una partida, el jugador pasa a reaparecer en ese lugar
    public void FijarPuntoDeReaparicion(Vector3 posicion)
    {
        puntoDeReaparicion = null;
        posicionInicial = posicion;
    }

    public void DarInvulnerabilidad()
    {
        timerInvulnerable = tiempoInvulnerable;
    }

    public void Reaparecer()
    {
        if (EsInvulnerable || MenuMuerte.Activo) return;

        if (mostrarMenuAlMorir)
        {
            MenuMuerte.Mostrar(gameObject);
            return;
        }

        Vector3 destino = puntoDeReaparicion != null ? puntoDeReaparicion.position : posicionInicial;

        // Igual que en Tp.cs: hay que apagar el CharacterController para poder mover al jugador
        if (characterController != null) characterController.enabled = false;
        transform.position = destino;
        if (characterController != null) characterController.enabled = true;

        timerInvulnerable = tiempoInvulnerable;
        Debug.Log("Un enemigo atrapo al jugador. Reapareciendo...");
    }
}
