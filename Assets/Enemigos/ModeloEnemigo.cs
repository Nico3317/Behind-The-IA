using UnityEngine;

public enum TipoEnemigo { Perseguidor, Patrulla, Parkour }

// Arma el modelo de cada enemigo con figuras basicas y lo anima.
// Las piezas que se llaman "Cuerpo" son las que cambian de color.
public class ModeloEnemigo : MonoBehaviour
{
    public TipoEnemigo tipo;

    private Transform modelo;
    private Transform piernaIzq;
    private Transform piernaDer;
    private Transform[] helices = new Transform[0];
    private Vector3 posicionAnterior;
    private float fase;

    public static GameObject Crear(TipoEnemigo tipo, string nombre, Color color, Vector3 posicion)
    {
        GameObject enemigo = new GameObject(nombre);
        enemigo.transform.position = posicion;

        CapsuleCollider capsula = enemigo.AddComponent<CapsuleCollider>();
        capsula.height = 2f;
        capsula.radius = 0.5f;

        ModeloEnemigo modelo = enemigo.AddComponent<ModeloEnemigo>();
        modelo.tipo = tipo;
        modelo.Armar(color);
        return enemigo;
    }

    private void Armar(Color color)
    {
        modelo = new GameObject("Modelo").transform;
        modelo.SetParent(transform, false);

        if (tipo == TipoEnemigo.Perseguidor) ArmarRobot(color);
        else if (tipo == TipoEnemigo.Patrulla) ArmarDron(color);
        else ArmarVirus(color);
    }

    // Robot con cuernos, que corre con los brazos hacia adelante
    private void ArmarRobot(Color color)
    {
        Color metal = new Color(0.2f, 0.2f, 0.22f, 1f);

        Pieza(PrimitiveType.Cube, "Cuerpo", modelo, new Vector3(0f, 0.1f, 0f), new Vector3(0.8f, 0.8f, 0.5f), color);
        Pieza(PrimitiveType.Cube, "Cuerpo", modelo, new Vector3(0f, 0.78f, 0f), new Vector3(0.55f, 0.45f, 0.5f), color);
        Pieza(PrimitiveType.Cube, "Boca", modelo, new Vector3(0f, 0.66f, 0.25f), new Vector3(0.35f, 0.08f, 0.04f), Color.black);

        for (int lado = -1; lado <= 1; lado += 2)
        {
            Pieza(PrimitiveType.Sphere, "Ojo", modelo, new Vector3(0.14f * lado, 0.85f, 0.25f), Vector3.one * 0.14f, Color.yellow);

            Transform cuerno = Pieza(PrimitiveType.Cylinder, "Cuerno", modelo, new Vector3(0.22f * lado, 1.1f, 0f), new Vector3(0.1f, 0.2f, 0.1f), metal);
            cuerno.localRotation = Quaternion.Euler(0f, 0f, -25f * lado);

            Pieza(PrimitiveType.Cube, "Brazo", modelo, new Vector3(0.5f * lado, 0.3f, 0.3f), new Vector3(0.18f, 0.18f, 0.8f), metal);
            Pieza(PrimitiveType.Sphere, "Mano", modelo, new Vector3(0.5f * lado, 0.3f, 0.75f), Vector3.one * 0.26f, Color.black);

            // La pierna cuelga de un pivote en la cadera para poder balancearla
            Transform cadera = new GameObject("Pierna").transform;
            cadera.SetParent(modelo, false);
            cadera.localPosition = new Vector3(0.22f * lado, -0.3f, 0f);
            Pieza(PrimitiveType.Cube, "Pierna", cadera, new Vector3(0f, -0.35f, 0f), new Vector3(0.22f, 0.7f, 0.25f), metal);
            if (lado < 0) piernaIzq = cadera;
            else piernaDer = cadera;
        }
    }

    // Dron vigilante con un ojo y cuatro helices
    private void ArmarDron(Color color)
    {
        Color metal = new Color(0.25f, 0.25f, 0.3f, 1f);

        Pieza(PrimitiveType.Sphere, "Cuerpo", modelo, new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.7f, 0.9f), color);
        Pieza(PrimitiveType.Sphere, "Ojo", modelo, new Vector3(0f, 0.2f, 0.36f), Vector3.one * 0.36f, Color.white);
        Pieza(PrimitiveType.Sphere, "Pupila", modelo, new Vector3(0f, 0.2f, 0.5f), Vector3.one * 0.18f, Color.red);
        Pieza(PrimitiveType.Cylinder, "Antena", modelo, new Vector3(0f, 0.7f, 0f), new Vector3(0.05f, 0.2f, 0.05f), metal);
        Pieza(PrimitiveType.Sphere, "Luz", modelo, new Vector3(0f, 0.92f, 0f), Vector3.one * 0.14f, Color.red);

        helices = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            Vector3 direccion = Quaternion.Euler(0f, 45f + 90f * i, 0f) * Vector3.forward;

            Transform brazo = Pieza(PrimitiveType.Cube, "Brazo", modelo, direccion * 0.5f + Vector3.up * 0.4f, new Vector3(0.08f, 0.08f, 0.6f), metal);
            brazo.localRotation = Quaternion.LookRotation(direccion);

            helices[i] = Pieza(PrimitiveType.Cube, "Helice", modelo, direccion * 0.8f + Vector3.up * 0.5f, new Vector3(0.6f, 0.03f, 0.08f), Color.black);
        }
    }

    // Virus: esfera con puas que gira
    private void ArmarVirus(Color color)
    {
        Color puas = new Color(0.9f, 0.1f, 0.4f, 1f);
        Pieza(PrimitiveType.Sphere, "Cuerpo", modelo, Vector3.zero, Vector3.one * 1.1f, color);

        Vector3[] direcciones =
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back,
            new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(1, -1, 1), new Vector3(1, -1, -1),
            new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1)
        };

        foreach (Vector3 d in direcciones)
        {
            Vector3 direccion = d.normalized;
            Transform pua = Pieza(PrimitiveType.Cylinder, "Pua", modelo, direccion * 0.65f, new Vector3(0.12f, 0.2f, 0.12f), puas);
            pua.localRotation = Quaternion.FromToRotation(Vector3.up, direccion);
            Pieza(PrimitiveType.Sphere, "Punta", modelo, direccion * 0.85f, Vector3.one * 0.24f, puas);
        }
    }

    private Transform Pieza(PrimitiveType forma, string nombre, Transform padre, Vector3 posicion, Vector3 escala, Color color)
    {
        GameObject pieza = GameObject.CreatePrimitive(forma);
        pieza.name = nombre;
        DestroyImmediate(pieza.GetComponent<Collider>()); // el choque lo maneja el collider del enemigo
        pieza.transform.SetParent(padre, false);
        pieza.transform.localPosition = posicion;
        pieza.transform.localScale = escala;
        pieza.GetComponent<Renderer>().material.color = color;
        return pieza.transform;
    }

    void Start()
    {
        posicionAnterior = transform.position;
    }

    void Update()
    {
        if (modelo == null || Time.deltaTime <= 0f) return;

        Vector3 avance = transform.position - posicionAnterior;
        avance.y = 0;
        posicionAnterior = transform.position;

        if (tipo == TipoEnemigo.Perseguidor)
        {
            // Las piernas se mueven segun lo que camina
            fase += avance.magnitude * 3f;
            float angulo = avance.sqrMagnitude > 0.000001f ? Mathf.Sin(fase) * 40f : 0f;
            piernaIzq.localRotation = Quaternion.Euler(angulo, 0f, 0f);
            piernaDer.localRotation = Quaternion.Euler(-angulo, 0f, 0f);
        }
        else if (tipo == TipoEnemigo.Patrulla)
        {
            fase += Time.deltaTime * 3f;
            modelo.localPosition = Vector3.up * Mathf.Sin(fase) * 0.12f;
            foreach (Transform helice in helices) helice.Rotate(0f, 1200f * Time.deltaTime, 0f);
        }
        else
        {
            modelo.Rotate(200f * Time.deltaTime, 60f * Time.deltaTime, 0f);
        }
    }
}
