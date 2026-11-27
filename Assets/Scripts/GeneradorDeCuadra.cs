using System.Collections.Generic;
using UnityEngine;

public class GeneradorDeCuadra : MonoBehaviour
{
    public GameObject edificioVacio;
    public GameObject edificioConParkour;

    public int cantidadCuadrasX = 3;
    public int cantidadCuadrasZ = 3;
    public float espacioEntreCuadras = 60f; // ancho de calle

    [Header("Enemigos")]
    [Tooltip("Prefab del enemigo que persigue al jugador cuando entra en su area. Si se deja vacio se crea una capsula.")]
    public GameObject prefabPerseguidor;
    [Tooltip("Prefab del enemigo que patrulla las calles. Si se deja vacio se crea una capsula.")]
    public GameObject prefabPatrulla;
    [Tooltip("Prefab del enemigo del parkour. Si se deja vacio se crea una capsula.")]
    public GameObject prefabParkour;

    public int cantidadPerseguidores = 3;
    public int cantidadPatrulleros = 3;

    [Tooltip("Nombre de la plataforma dentro de EdificioConParkour donde va y viene el enemigo del parkour.")]
    public string plataformaEnemigoParkour = "Cube (134)";

    [Tooltip("Los perseguidores no aparecen a menos de esta distancia del jugador.")]
    public float distanciaMinimaAlJugador = 25f;

    private List<Bounds> cuadras = new List<Bounds>();
    private Transform contenedorEnemigos;

    void Start()
    {
        // Misma semilla para la misma partida: al cargar se regenera la misma ciudad
        Random.InitState(SistemaGuardado.ObtenerSemillaDeLaPartida());
        GenerarCiudad();
        GenerarEnemigos();
    }

    void GenerarCiudad()
    {
        // Usamos edificioVacio como referencia de tamaño
        Vector2 tamaño = ObtenerTamañoHorizontal(edificioVacio);
        float ancho = tamaño.x;
        float largo = tamaño.y;

        float pasoX = ancho + espacioEntreCuadras;
        float pasoZ = largo + espacioEntreCuadras;

        for (int x = 0; x < cantidadCuadrasX; x++)
        {
            for (int z = 0; z < cantidadCuadrasZ; z++)
            {
                Vector3 posicion = new Vector3(x * pasoX, 0, z * pasoZ);
                InstanciarEdificio(posicion);
            }
        }
    }

    void InstanciarEdificio(Vector3 posicion)
    {
        GameObject prefabElegido = Random.value > 0.5f ? edificioVacio : edificioConParkour;

        Vector3 posicionFinal = posicion;

        if (prefabElegido == edificioVacio)
        {
            // Edificio vacío: se genera directamente en Y = 0
            posicionFinal.y = 80;
        }

        GameObject instancia = Instantiate(prefabElegido, posicionFinal, Quaternion.identity);

        if (prefabElegido == edificioConParkour)
        {
            // Ajuste vertical para que toque el suelo
            Renderer[] renderers = instancia.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer r in renderers)
                {
                    bounds.Encapsulate(r.bounds);
                }

                float puntoMasBajo = bounds.min.y;
                instancia.transform.position -= new Vector3(0, puntoMasBajo, 0);
            }

            // ajusto los ejes X y Z para que quede centrado en la cuadra
            instancia.transform.position -= new Vector3(35, 0, 22f);

            // el enemigo del parkour va sobre una de las plataformas
            CrearEnemigoParkour(instancia);
        }

        // guardo el espacio que ocupa la cuadra para despues ubicar a los enemigos en las calles
        cuadras.Add(ObtenerBounds(instancia));
    }



    Vector2 ObtenerTamañoHorizontal(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return Vector2.zero;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return new Vector2(bounds.size.x, bounds.size.z);
    }

    Bounds ObtenerBounds(GameObject objeto)
    {
        Renderer[] renderers = objeto.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(objeto.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }
        return bounds;
    }

    // ---------------- ENEMIGOS ----------------

    void GenerarEnemigos()
    {
        if (cuadras.Count == 0) return;

        List<int> ordenCuadras = new List<int>();
        for (int i = 0; i < cuadras.Count; i++) ordenCuadras.Add(i);
        Mezclar(ordenCuadras);

        // Patrulleros: cada uno da vueltas por la calle alrededor de una cuadra distinta
        int patrulleros = 0;
        for (int i = 0; i < ordenCuadras.Count * 2 && patrulleros < cantidadPatrulleros; i++)
        {
            int indiceCuadra = ordenCuadras[i % ordenCuadras.Count];
            List<Vector3> ruta = ObtenerRutaAlrededor(indiceCuadra);
            if (ruta == null) continue;

            int puntoInicial = Random.Range(0, ruta.Count);
            GameObject enemigo = CrearEnemigo(prefabPatrulla, TipoEnemigo.Patrulla, "EnemigoPatrulla " + (patrulleros + 1), new Color(0.6f, 0.1f, 0.9f, 1f), ruta[puntoInicial]);
            EnemigoPatrulla patrulla = AgregarComponente<EnemigoPatrulla>(enemigo);
            patrulla.idaYVuelta = false;
            patrulla.ConfigurarRuta(ruta, (puntoInicial + 1) % ruta.Count);
            patrulleros++;
        }

        // Perseguidores: esperan en la mitad de una calle y persiguen al jugador si entra en su area
        Vector3 posicionJugador = ObtenerPosicionJugador();
        int creados = 0;
        int intentos = 0;
        while (creados < cantidadPerseguidores && intentos < 100)
        {
            intentos++;
            int indiceCuadra = Random.Range(0, cuadras.Count);
            Vector3? lugar = ObtenerLugarEnLaCalle(indiceCuadra);
            if (lugar == null) continue;

            Vector3 diferencia = lugar.Value - posicionJugador;
            diferencia.y = 0;
            if (diferencia.magnitude < distanciaMinimaAlJugador) continue;

            GameObject enemigo = CrearEnemigo(prefabPerseguidor, TipoEnemigo.Perseguidor, "EnemigoPerseguidor " + (creados + 1), new Color(1f, 0.55f, 0f, 1f), lugar.Value);
            AgregarComponente<EnemigoPerseguidor>(enemigo);
            creados++;
        }
    }

    void CrearEnemigoParkour(GameObject edificio)
    {
        Transform plataforma = null;
        foreach (Transform hijo in edificio.GetComponentsInChildren<Transform>())
        {
            if (hijo.name == plataformaEnemigoParkour)
            {
                plataforma = hijo;
                break;
            }
        }

        if (plataforma == null)
        {
            Debug.LogWarning("No se encontro la plataforma '" + plataformaEnemigoParkour + "' en " + edificio.name + ". No se crea el enemigo del parkour.");
            return;
        }

        Renderer renderPlataforma = plataforma.GetComponent<Renderer>();
        Bounds b = renderPlataforma != null ? renderPlataforma.bounds : new Bounds(plataforma.position, Vector3.one);

        // Recorre la plataforma a lo largo de su lado mas largo, parado encima
        float alturaEnemigo = 1f; // mitad de la capsula
        float margen = 0.8f;
        Vector3 a;
        Vector3 c;
        if (b.size.x >= b.size.z)
        {
            a = new Vector3(b.min.x + margen, b.max.y + alturaEnemigo, b.center.z);
            c = new Vector3(b.max.x - margen, b.max.y + alturaEnemigo, b.center.z);
        }
        else
        {
            a = new Vector3(b.center.x, b.max.y + alturaEnemigo, b.min.z + margen);
            c = new Vector3(b.center.x, b.max.y + alturaEnemigo, b.max.z - margen);
        }

        GameObject enemigo = CrearEnemigo(prefabParkour, TipoEnemigo.Parkour, "EnemigoParkour", new Color(0.1f, 0.8f, 0.9f, 1f), Vector3.Lerp(a, c, 0.5f));
        EnemigoParkour parkour = AgregarComponente<EnemigoParkour>(enemigo);
        parkour.ConfigurarRecorrido(a, c);
    }

    GameObject CrearEnemigo(GameObject prefab, TipoEnemigo tipo, string nombre, Color color, Vector3 posicion)
    {
        if (contenedorEnemigos == null)
        {
            contenedorEnemigos = new GameObject("Enemigos").transform;
        }

        GameObject enemigo;
        if (prefab != null)
        {
            enemigo = Instantiate(prefab, posicion, Quaternion.identity);
            enemigo.name = nombre;
        }
        else
        {
            enemigo = ModeloEnemigo.Crear(tipo, nombre, color, posicion);
        }

        enemigo.transform.SetParent(contenedorEnemigos, true);
        return enemigo;
    }

    T AgregarComponente<T>(GameObject enemigo) where T : Enemigo
    {
        T componente = enemigo.GetComponent<T>();
        if (componente != null) return componente;

        // Los enemigos que caminan usan CharacterController, asi que saco el collider de la capsula
        bool usaCharacterController = typeof(T) != typeof(EnemigoParkour);
        if (usaCharacterController && enemigo.GetComponent<CharacterController>() == null)
        {
            Collider colliderViejo = enemigo.GetComponent<Collider>();
            if (colliderViejo != null) DestroyImmediate(colliderViejo);
            enemigo.AddComponent<CharacterController>();
        }

        return enemigo.AddComponent<T>();
    }

    // Las 4 esquinas de la calle que rodea una cuadra (por la mitad de la calle si hay lugar)
    List<Vector3> ObtenerRutaAlrededor(int indiceCuadra)
    {
        Bounds b = cuadras[indiceCuadra];
        float[] margenes = { espacioEntreCuadras * 0.5f, espacioEntreCuadras * 0.35f, espacioEntreCuadras * 0.2f, espacioEntreCuadras * 0.1f };

        foreach (float m in margenes)
        {
            List<Vector3> ruta = new List<Vector3>
            {
                new Vector3(b.min.x - m, 1.2f, b.min.z - m),
                new Vector3(b.max.x + m, 1.2f, b.min.z - m),
                new Vector3(b.max.x + m, 1.2f, b.max.z + m),
                new Vector3(b.min.x - m, 1.2f, b.max.z + m)
            };

            if (RutaLibre(ruta)) return ruta;
        }

        return null;
    }

    Vector3? ObtenerLugarEnLaCalle(int indiceCuadra)
    {
        Bounds b = cuadras[indiceCuadra];
        float m = espacioEntreCuadras * 0.5f;
        int lado = Random.Range(0, 4);

        Vector3 lugar;
        if (lado == 0) lugar = new Vector3(b.center.x, 1.2f, b.min.z - m);
        else if (lado == 1) lugar = new Vector3(b.center.x, 1.2f, b.max.z + m);
        else if (lado == 2) lugar = new Vector3(b.min.x - m, 1.2f, b.center.z);
        else lugar = new Vector3(b.max.x + m, 1.2f, b.center.z);

        if (!PuntoLibre(lugar)) return null;
        return lugar;
    }

    bool RutaLibre(List<Vector3> ruta)
    {
        for (int i = 0; i < ruta.Count; i++)
        {
            Vector3 desde = ruta[i];
            Vector3 hasta = ruta[(i + 1) % ruta.Count];
            int pasos = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(desde, hasta) / 3f));
            for (int p = 0; p <= pasos; p++)
            {
                if (!PuntoLibre(Vector3.Lerp(desde, hasta, (float)p / pasos))) return false;
            }
        }
        return true;
    }

    // Un punto esta libre si no cae adentro de ninguna cuadra que llegue al piso (mirando solo X y Z)
    bool PuntoLibre(Vector3 punto)
    {
        for (int i = 0; i < cuadras.Count; i++)
        {
            Bounds b = cuadras[i];
            if (b.min.y > 2.5f) continue; // el edificio esta en el aire, abajo se puede caminar

            float borde = 1.5f;
            bool adentro = punto.x > b.min.x - borde && punto.x < b.max.x + borde &&
                           punto.z > b.min.z - borde && punto.z < b.max.z + borde;
            if (adentro) return false;
        }
        return true;
    }

    Vector3 ObtenerPosicionJugador()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null) return new Vector3(99999f, 0, 99999f);
        return jugador.transform.position;
    }

    void Mezclar(List<int> lista)
    {
        for (int i = lista.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = lista[i];
            lista[i] = lista[j];
            lista[j] = temp;
        }
    }
}
