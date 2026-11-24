using UnityEngine;

// Proyectil del dron: va derecho, le saca vida al jugador y desaparece al chocar con una pared.
public class DisparoEnemigo : MonoBehaviour
{
    private Vector3 direccion;
    private float velocidad;
    private int danio;
    private Transform jugador;
    private CharacterController jugadorCC;
    private float radio = 0.15f;

    public static void Crear(Vector3 origen, Vector3 direccion, float velocidad, int danio, Transform jugador, CharacterController jugadorCC)
    {
        GameObject bala = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bala.name = "DisparoEnemigo";
        DestroyImmediate(bala.GetComponent<Collider>());
        bala.transform.position = origen;
        bala.transform.localScale = Vector3.one * 0.3f;
        bala.GetComponent<Renderer>().material.color = new Color(1f, 0.1f, 0.3f, 1f);

        DisparoEnemigo disparo = bala.AddComponent<DisparoEnemigo>();
        disparo.direccion = direccion.normalized;
        disparo.velocidad = velocidad;
        disparo.danio = danio;
        disparo.jugador = jugador;
        disparo.jugadorCC = jugadorCC;
        Destroy(bala, 4f);
    }

    void Update()
    {
        float paso = velocidad * Time.deltaTime;
        if (paso <= 0f) return;

        // Distancia hasta la primera pared en este paso (sin contar enemigos ni jugador)
        float distanciaPared = Mathf.Infinity;
        foreach (RaycastHit hit in Physics.RaycastAll(transform.position, direccion, paso, ~0, QueryTriggerInteraction.Ignore))
        {
            if (jugador != null && hit.transform.IsChildOf(jugador)) continue;
            if (hit.transform.GetComponentInParent<Enemigo>() != null) continue;
            distanciaPared = Mathf.Min(distanciaPared, hit.distance);
        }

        if (jugador != null && jugadorCC != null && jugadorCC.enabled)
        {
            Bounds cuerpo = jugadorCC.bounds;
            cuerpo.Expand(radio * 2f);
            float distancia;
            bool toca = cuerpo.IntersectRay(new Ray(transform.position, direccion), out distancia);
            if (toca && distancia <= paso && distancia < distanciaPared)
            {
                VidaJugador vida = jugador.GetComponent<VidaJugador>();
                if (vida != null) vida.RecibirDanio(danio);
                Destroy(gameObject);
                return;
            }
        }

        if (distanciaPared < Mathf.Infinity)
        {
            Destroy(gameObject);
            return;
        }
        transform.position += direccion * paso;
    }
}
