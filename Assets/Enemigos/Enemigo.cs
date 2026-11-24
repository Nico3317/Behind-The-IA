using UnityEngine;

// Base comun de todos los enemigos: busca al jugador y lo hace reaparecer cuando lo toca.
public abstract class Enemigo : MonoBehaviour
{
    [Header("Atrapar al jugador")]
    [Tooltip("Distancia extra a la que el enemigo atrapa al jugador (ademas de tocarlo).")]
    public float margenAtrapar = 0.3f;

    protected Transform jugador;
    protected CharacterController jugadorCC;
    protected ReaparecerJugador reaparecer;
    protected Collider miCollider;

    protected virtual void Start()
    {
        miCollider = GetComponent<Collider>();
        BuscarJugador();
    }

    protected void BuscarJugador()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj == null)
        {
            Debug.LogWarning("No se encontro ningun objeto con el tag Player. El enemigo " + name + " no va a hacer nada.");
            return;
        }

        jugadorCC = obj.GetComponent<CharacterController>();
        if (jugadorCC == null) jugadorCC = obj.GetComponentInParent<CharacterController>();
        if (jugadorCC == null) jugadorCC = obj.GetComponentInChildren<CharacterController>();

        jugador = jugadorCC != null ? jugadorCC.transform : obj.transform;

        reaparecer = jugador.GetComponent<ReaparecerJugador>();
        if (reaparecer == null)
        {
            reaparecer = jugador.gameObject.AddComponent<ReaparecerJugador>();
        }

        if (jugador.GetComponent<VidaJugador>() == null)
        {
            jugador.gameObject.AddComponent<VidaJugador>();
        }
    }

    protected Vector3 CentroJugador()
    {
        if (jugadorCC != null && jugadorCC.enabled) return jugadorCC.bounds.center;
        return jugador.position;
    }

    protected bool TocaAlJugador()
    {
        if (jugador == null || miCollider == null) return false;
        if (reaparecer != null && reaparecer.EsInvulnerable) return false;

        Bounds misBounds = miCollider.bounds;
        misBounds.Expand(margenAtrapar * 2f);

        if (jugadorCC != null && jugadorCC.enabled)
        {
            return misBounds.Intersects(jugadorCC.bounds);
        }
        return misBounds.Contains(jugador.position);
    }

    protected void AtraparJugador()
    {
        if (reaparecer != null) reaparecer.Reaparecer();
        AlAtraparJugador();
    }

    // Cada enemigo puede reaccionar distinto despues de atrapar al jugador
    protected virtual void AlAtraparJugador() { }

    protected void MirarHacia(Vector3 direccion, float velocidadGiro)
    {
        direccion.y = 0;
        if (direccion.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccion), Time.deltaTime * velocidadGiro);
    }

    protected void CambiarColor(Color color)
    {
        Renderer render = GetComponent<Renderer>();
        if (render != null) render.material.color = color;

        // Piezas del modelo armado por ModeloEnemigo
        foreach (Renderer pieza in GetComponentsInChildren<Renderer>())
        {
            if (pieza.name == "Cuerpo") pieza.material.color = color;
        }
    }
}
