using UnityEngine;

public class GeneradorDeCiudad : MonoBehaviour
{
    public GameObject edificioVacio;
    public GameObject edificioConParkour;

    public int cantidadCuadrasX = 3;
    public int cantidadCuadrasZ = 3;
    public float espacioEntreCuadras = 60f; // ancho de calle

    void Start()
    {
        GenerarCiudad();
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
        }
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
}
