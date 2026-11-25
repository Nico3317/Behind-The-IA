using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Arma la escena MapaNuevo (ciudad, parkours, llaves, edificio final y enemigos).
// Se usa desde el menu Herramientas > Construir mapa nuevo. Pisa la escena MapaNuevo.
public static class ConstructorMapa
{
    const string EscenaBase = "Assets/Scenes/test_player.unity";
    const string EscenaNueva = "Assets/Scenes/MapaNuevo.unity";
    const string EscenaMenu = "Assets/Scenes/MenuPrincipal.unity";
    const string Kenney = "Assets/Mapa/Kenney/";
    const float Paso = 50f;   // distancia entre manzanas
    const float Lado = 34f;   // lado de cada manzana

    static Transform raiz;
    static Material asfalto, vereda, plataforma, fugaz, techo, faro, pasto;

    [MenuItem("Herramientas/Construir mapa nuevo")]
    public static void Construir()
    {
        CrearMateriales();

        EditorSceneManager.OpenScene(EscenaBase);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), EscenaNueva, true);
        EditorSceneManager.OpenScene(EscenaNueva);

        Object.DestroyImmediate(GameObject.Find("Ciudad"));
        raiz = new GameObject("Mapa").transform;
        GameObject.Find("Plane").GetComponent<Renderer>().sharedMaterial = asfalto;

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        jugador.transform.position = new Vector3(0f, 1.5f, 0f);
        GameObject.FindGameObjectWithTag("MainCamera").transform.position = new Vector3(0f, 5f, -8f);

        Manzanas();
        Parkour1(new Vector3(-2f * Paso, 0f, 0f));
        Parkour2(new Vector3(2f * Paso, 0f, 0f));
        Parkour3(new Vector3(0f, 0f, -2f * Paso));
        EdificioFinal(jugador);
        Enemigos();
        Limites();
        new GameObject("MenuPausa").AddComponent<MenuPausa>();
        Hud();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        ConfigurarMenuPrincipal();
        ConfigurarBuild();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(EscenaNueva);
        Debug.Log("Mapa nuevo construido");
    }

    // ---------------- CIUDAD ----------------

    static void Manzanas()
    {
        string[][] repartos =
        {
            new[] { "building-a", "building-f", "building-d", "building-h" },
            new[] { "building-n" },
            new[] { "building-skyscraper-b", "building-c" , "building-g", "building-b" },
            new[] { "building-j", "building-k" },
            new[] { "building-l", "building-i", "building-skyscraper-c", "building-d" },
            new[] { "building-skyscraper-e", "building-m", "building-a", "building-c" },
        };

        int n = 0;
        for (int x = -2; x <= 2; x++)
        {
            for (int z = -2; z <= 2; z++)
            {
                Vector3 centro = new Vector3(x * Paso, 0f, z * Paso);
                bool esPlaza = x == 0 && z == 0;
                bool esParkour = (Mathf.Abs(x) == 2 && z == 0) || (x == 0 && z == -2);
                // (0,1) y (0,2) quedan como parque para que se vea el edificio final
                bool esParque = x == 0 && z > 0;

                Caja("Vereda", raiz, centro + Vector3.up * 0.1f, new Vector3(Lado, 0.2f, Lado), esPlaza || esParque ? pasto : vereda);
                if (esParkour) continue;
                if (esPlaza || esParque)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 esquina = Quaternion.Euler(0f, 45f + 90f * i, 0f) * Vector3.forward * 17f;
                        Modelo(i % 2 == 0 ? "detail-parasol-a" : "detail-parasol-b", centro + esquina + Vector3.up * 0.2f, 0f, 8f);
                    }
                    continue;
                }

                string[] reparto = repartos[n % repartos.Length];
                n++;
                for (int i = 0; i < reparto.Length; i++)
                {
                    Vector3 lugar = Vector3.zero;
                    float escala = 8f;
                    if (reparto.Length == 1) escala = 12f;
                    else if (reparto.Length == 2) lugar = new Vector3(0f, 0f, i == 0 ? -8f : 8f);
                    else lugar = new Vector3(i % 2 == 0 ? -8.5f : 8.5f, 0f, i < 2 ? -8.5f : 8.5f);
                    Modelo(reparto[i], centro + lugar + Vector3.up * 0.2f, i < 2 ? 180f : 0f, escala);
                }
            }
        }
    }

    // ---------------- PARKOURS ----------------

    // 1: espiral alrededor de un rascacielos. La llave esta en la azotea.
    static void Parkour1(Vector3 c)
    {
        Transform p = Grupo("Parkour 1 - Espiral");
        Modelo("building-skyscraper-a", c + Vector3.up * 0.2f, 0f, 10f, p);

        Vector2[] vuelta =
        {
            new Vector2(9, -9), new Vector2(9, -3), new Vector2(9, 3), new Vector2(9, 9),
            new Vector2(3, 9), new Vector2(-3, 9), new Vector2(-9, 9), new Vector2(-9, 3),
            new Vector2(-9, -3), new Vector2(-9, -9), new Vector2(-3, -9), new Vector2(3, -9)
        };
        for (int k = 0; k < 14; k++)
        {
            Vector2 v = vuelta[k % 12];
            Plataforma("P1_" + k.ToString("00"), p, c, v.x, v.y, 1.5f + 2f * k, 3.5f, 3.5f, plataforma);
        }
        Plataforma("P1_14", p, c, 0f, 0f, 29.3f, 12f, 12f, techo);
        Llave("LlaveUnoCompleta", p, c + new Vector3(0f, 29.3f, -2f));
        Virus(p, c + new Vector3(-4.5f, 30.3f, 3f), c + new Vector3(4.5f, 30.3f, 3f));
        Faro(p, c, new Color(1f, 0.85f, 0.1f, 1f));
    }

    // 2: obra en construccion, columnas en zigzag con plataformas que desaparecen (rojas).
    static void Parkour2(Vector3 c)
    {
        Transform p = Grupo("Parkour 2 - Obra");
        int k = 0;
        for (int fila = 0; fila < 4; fila++)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = -14f + 7f * (fila % 2 == 0 ? i : 4 - i);
                float z = -12f + 7f * fila;
                float y = 1.5f + 1.2f * k;
                bool ultima = k == 19;
                bool desaparece = !ultima && k % 4 == 2;

                GameObject caja = Plataforma("P2_" + k.ToString("00"), p, c, x, z, y, ultima ? 5f : 4f, ultima ? 5f : 4f, desaparece ? fugaz : plataforma);
                if (desaparece)
                {
                    DesaparecerObjeto d = caja.AddComponent<DesaparecerObjeto>();
                    d.delayAntesDeDesaparecer = 1.5f;
                    d.tiempoDesaparecido = 2f;
                }
                else
                {
                    float alto = y - 0.5f;
                    Caja("Columna", p, c + new Vector3(x, alto * 0.5f, z), new Vector3(0.8f, alto, 0.8f), techo);
                }
                if (ultima) Llave("LlaveDosCompleta", p, c + new Vector3(x - 1f, y, z + 1f));
                k++;
            }
        }
        Faro(p, c, new Color(0.2f, 0.9f, 1f, 1f));
    }

    // 3: cuatro torres unidas por escalones, un puente angosto y plataformas que desaparecen.
    static void Parkour3(Vector3 c)
    {
        Transform p = Grupo("Parkour 3 - Torres");
        int k = 0;

        float[] subida = { 4.5f, 0.3f, -3.9f };
        float[] alturas = { 1.8f, 3.9f, 6f };
        for (int i = 0; i < 3; i++)
        {
            Plataforma("P3_" + (k++).ToString("00"), p, c, subida[i], -13f, alturas[i], 3f, 3f, plataforma);
            Caja("Columna", p, c + new Vector3(subida[i], (alturas[i] - 0.5f) * 0.5f, -13f), new Vector3(0.8f, alturas[i] - 0.5f, 0.8f), techo);
        }

        Torre("P3_" + (k++).ToString("00"), p, c, -11f, -11f, 8f, "building-a");
        Plataforma("P3_" + (k++).ToString("00"), p, c, -11f, -3.2f, 9.2f, 3f, 3f, plataforma);
        Plataforma("P3_" + (k++).ToString("00"), p, c, -11f, 2.8f, 10.5f, 3f, 3f, plataforma);
        Torre("P3_" + (k++).ToString("00"), p, c, -11f, 11f, 11.5f, "building-f");
        Plataforma("P3_" + (k++).ToString("00"), p, c, -3f, 11f, 11.5f, 8f, 1.6f, plataforma); // puente
        Plataforma("P3_" + (k++).ToString("00"), p, c, 4f, 11f, 13f, 3f, 3f, plataforma);
        Torre("P3_" + (k++).ToString("00"), p, c, 11f, 11f, 14.5f, "building-g");
        Virus(p, c + new Vector3(8f, 15.5f, 13.5f), c + new Vector3(14f, 15.5f, 13.5f));
        for (int i = 0; i < 2; i++)
        {
            GameObject caja = Plataforma("P3_" + (k++).ToString("00"), p, c, 11f, i == 0 ? 3.2f : -2.8f, i == 0 ? 15.7f : 17f, 3f, 3f, fugaz);
            DesaparecerObjeto d = caja.AddComponent<DesaparecerObjeto>();
            d.delayAntesDeDesaparecer = 1.5f;
            d.tiempoDesaparecido = 2f;
        }
        Torre("P3_" + k.ToString("00"), p, c, 11f, -11f, 18f, "building-d");
        Llave("LlaveTresCompleta", p, c + new Vector3(11f, 18f, -13f));
        Faro(p, c, new Color(1f, 0.3f, 0.8f, 1f));
    }

    static void Torre(string nombre, Transform p, Vector3 c, float x, float z, float alto, string modelo)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Kenney + modelo + ".fbx");
        Vector3 medida = prefab.GetComponentInChildren<Renderer>().bounds.size;
        GameObject torre = Modelo(modelo, c + new Vector3(x, 0.2f, z), 0f, 1f, p);
        float cuerpo = alto - 0.6f;
        torre.transform.localScale = new Vector3(8f / medida.x, cuerpo / medida.y, 8f / medida.z);
        Plataforma(nombre, p, c, x, z, alto, 8.4f, 8.4f, techo);
    }

    // y es la altura de la cara de arriba
    static GameObject Plataforma(string nombre, Transform p, Vector3 c, float x, float z, float y, float ancho, float largo, Material m)
    {
        return Caja(nombre, p, c + new Vector3(x, y - 0.25f, z), new Vector3(ancho, 0.5f, largo), m);
    }

    static void Llave(string nombre, Transform p, Vector3 piso)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Inventario y llaves/" + nombre + ".prefab");
        GameObject llave = (GameObject)PrefabUtility.InstantiatePrefab(prefab, p);

        // El dibujo de la llave esta corrido respecto del pivote, asi que la centro por sus bounds
        llave.transform.position = Vector3.zero;
        Renderer[] renders = llave.GetComponentsInChildren<Renderer>();
        Bounds b = renders[0].bounds;
        foreach (Renderer r in renders) b.Encapsulate(r.bounds);
        llave.transform.position = piso - new Vector3(b.center.x, b.min.y - 0.3f, b.center.z);

        // Area para agarrarla con E sin tener que pegarse a ella
        SphereCollider area = llave.AddComponent<SphereCollider>();
        area.isTrigger = true;
        area.center = llave.transform.InverseTransformPoint(piso + Vector3.up * 1.2f);
        area.radius = 1.8f;
    }

    static void Virus(Transform p, Vector3 a, Vector3 b)
    {
        GameObject go = new GameObject("EnemigoParkour");
        go.transform.SetParent(p, false);
        go.transform.position = a;
        CapsuleCollider capsula = go.AddComponent<CapsuleCollider>();
        capsula.height = 2f;
        capsula.radius = 0.5f;
        EnemigoParkour e = go.AddComponent<EnemigoParkour>();
        e.puntoA = Punto(p, "PuntoA", a);
        e.puntoB = Punto(p, "PuntoB", b);
        Vestir(go, TipoEnemigo.Parkour, e.color);
    }

    static void Faro(Transform p, Vector3 c, Color color)
    {
        GameObject f = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        f.name = "Faro";
        Object.DestroyImmediate(f.GetComponent<Collider>());
        f.transform.SetParent(p, false);
        f.transform.position = c + new Vector3(-15.5f, 35f, -15.5f);
        f.transform.localScale = new Vector3(0.6f, 35f, 0.6f);
        f.GetComponent<Renderer>().sharedMaterial = Mat("Faro " + p.name, color, true);
    }

    // ---------------- FINAL ----------------

    static void EdificioFinal(GameObject jugador)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs Edificios/EdificioConPortal.prefab");
        GameObject edificio = (GameObject)PrefabUtility.InstantiatePrefab(prefab, raiz);
        edificio.name = "Edificio Final";
        // Girado para que la puerta mire a la plaza
        edificio.transform.SetPositionAndRotation(new Vector3(4.5f, 80.05f, 155.7f), Quaternion.Euler(0f, 180f, 0f));

        foreach (Tp portal in edificio.GetComponentsInChildren<Tp>())
        {
            portal.ThePlayer = jugador;
            PrefabUtility.RecordPrefabInstancePropertyModifications(portal);

            // El trigger original es tan bajo que el jugador no lo toca: lo hago de 2 metros de alto
            BoxCollider area = portal.GetComponent<BoxCollider>();
            float alto = 2f / portal.transform.lossyScale.y;
            area.size = new Vector3(area.size.x, alto, area.size.z);
            area.center = new Vector3(area.center.x, alto * 0.5f, area.center.z);
            PrefabUtility.RecordPrefabInstancePropertyModifications(area);
        }
        Caja("Camino", raiz, new Vector3(0f, 0.03f, 133f), new Vector3(10f, 0.06f, 30f), vereda);
    }

    // ---------------- ENEMIGOS ----------------

    static void Enemigos()
    {
        Transform p = Grupo("Enemigos");

        Vector2[] rutas = { new Vector2(0, 1), new Vector2(1, 2), new Vector2(-1, 2) };
        for (int i = 0; i < rutas.Length; i++)
        {
            Vector3 c = new Vector3(rutas[i].x * Paso, 1.2f, rutas[i].y * Paso);
            float d = Paso * 0.5f;
            GameObject go = Caminante("EnemigoPatrulla " + (i + 1), p, c + new Vector3(d, 0f, d));
            EnemigoPatrulla e = go.AddComponent<EnemigoPatrulla>();
            e.puntosDePatrulla = new List<Transform>
            {
                Punto(p, "Ruta", c + new Vector3(d, 0f, -d)), Punto(p, "Ruta", c + new Vector3(-d, 0f, -d)),
                Punto(p, "Ruta", c + new Vector3(-d, 0f, d)), Punto(p, "Ruta", c + new Vector3(d, 0f, d))
            };
            Vestir(go, TipoEnemigo.Patrulla, e.color);
        }

        Vector3[] puestos = { new Vector3(-75f, 1.2f, -75f), new Vector3(75f, 1.2f, -75f), new Vector3(75f, 1.2f, 75f), new Vector3(-75f, 1.2f, 75f) };
        for (int i = 0; i < puestos.Length; i++)
        {
            GameObject go = Caminante("EnemigoPerseguidor " + (i + 1), p, puestos[i]);
            EnemigoPerseguidor e = go.AddComponent<EnemigoPerseguidor>();
            Vestir(go, TipoEnemigo.Perseguidor, e.colorTranquilo);
        }
    }

    static GameObject Caminante(string nombre, Transform p, Vector3 posicion)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(p, false);
        go.transform.position = posicion;
        go.AddComponent<CharacterController>();
        return go;
    }

    static void Vestir(GameObject go, TipoEnemigo tipo, Color color)
    {
        ModeloEnemigo m = go.AddComponent<ModeloEnemigo>();
        m.tipo = tipo;
        m.colorInicial = color;
    }

    // ---------------- HUD ----------------

    static void Hud()
    {
        // La barra original queda afuera de la pantalla en ventanas chicas; la reemplaza HudLlaves
        InventarioUI barra = Object.FindFirstObjectByType<InventarioUI>();
        if (barra != null) barra.gameObject.SetActive(false);

        HudLlaves hud = new GameObject("HudLlaves").AddComponent<HudLlaves>();
        hud.llaves = new DataItemInventario[3];
        string[] nombres = { "LlaveUnoCompleta", "LlaveDosCompleta", "LlaveTresCompleta" };
        for (int i = 0; i < 3; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Inventario y llaves/" + nombres[i] + ".prefab");
            hud.llaves[i] = prefab.GetComponent<RecogerRecompensa>().ItemData;
        }
        // Mismos colores que los faros de cada parkour
        hud.colores = new[] { new Color(1f, 0.85f, 0.1f, 1f), new Color(0.2f, 0.9f, 1f, 1f), new Color(1f, 0.3f, 0.8f, 1f) };
        hud.puertaFinal = Object.FindFirstObjectByType<Puerta>().transform;
    }

    // ---------------- MENU Y BUILD ----------------

    static void ConfigurarMenuPrincipal()
    {
        EditorSceneManager.OpenScene(EscenaMenu);
        MenuInicial menu = Object.FindFirstObjectByType<MenuInicial>();
        Transform principal = menu.transform.Find("MainMenu");
        Button salir = principal.Find("Boton Salir").GetComponent<Button>();
        while (salir.onClick.GetPersistentEventCount() > 0) UnityEventTools.RemovePersistentListener(salir.onClick, 0);
        UnityEventTools.AddPersistentListener(salir.onClick, menu.Salir);

        TMPro.TMP_Text texto = principal.Find("Boton Nueva Partida").GetComponentInChildren<TMPro.TMP_Text>(true);
        texto.text = "Jugar";
        EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
        EditorSceneManager.SaveScene(menu.gameObject.scene);
    }

    static void ConfigurarBuild()
    {
        List<EditorBuildSettingsScene> escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!escenas.Exists(e => e.path == EscenaNueva))
        {
            escenas.Add(new EditorBuildSettingsScene(EscenaNueva, true));
            EditorBuildSettings.scenes = escenas.ToArray();
        }
    }

    // ---------------- UTILES ----------------

    static void Limites()
    {
        Transform p = Grupo("Limites");
        Pared(p, new Vector3(-140f, 25f, 50f), new Vector3(1f, 50f, 400f));
        Pared(p, new Vector3(140f, 25f, 50f), new Vector3(1f, 50f, 400f));
        Pared(p, new Vector3(0f, 25f, -140f), new Vector3(281f, 50f, 1f));
        Pared(p, new Vector3(0f, 25f, 245f), new Vector3(281f, 50f, 1f));
    }

    static void Pared(Transform p, Vector3 posicion, Vector3 medida)
    {
        GameObject go = new GameObject("Limite");
        go.transform.SetParent(p, false);
        go.transform.position = posicion;
        go.AddComponent<BoxCollider>().size = medida;
    }

    static Transform Grupo(string nombre)
    {
        Transform t = new GameObject(nombre).transform;
        t.SetParent(raiz, false);
        return t;
    }

    static Transform Punto(Transform p, string nombre, Vector3 posicion)
    {
        Transform t = new GameObject(nombre).transform;
        t.SetParent(p, false);
        t.position = posicion;
        return t;
    }

    static GameObject Caja(string nombre, Transform p, Vector3 centro, Vector3 medida, Material m)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = nombre;
        go.transform.SetParent(p, false);
        go.transform.position = centro;
        go.transform.localScale = medida;
        go.GetComponent<Renderer>().sharedMaterial = m;
        return go;
    }

    static GameObject Modelo(string nombre, Vector3 posicion, float giro, float escala, Transform p = null)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Kenney + nombre + ".fbx");
        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, p != null ? p : raiz);
        go.transform.SetPositionAndRotation(posicion, Quaternion.Euler(0f, giro, 0f));
        go.transform.localScale = Vector3.one * escala;
        if (!nombre.StartsWith("detail"))
        {
            foreach (MeshFilter f in go.GetComponentsInChildren<MeshFilter>()) f.gameObject.AddComponent<MeshCollider>();
        }
        return go;
    }

    static void CrearMateriales()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Mapa/Materiales")) AssetDatabase.CreateFolder("Assets/Mapa", "Materiales");
        asfalto = Mat("Asfalto", new Color(0.2f, 0.21f, 0.24f, 1f), false);
        vereda = Mat("Vereda", new Color(0.6f, 0.6f, 0.62f, 1f), false);
        plataforma = Mat("Plataforma", new Color(0.2f, 0.55f, 0.9f, 1f), false);
        fugaz = Mat("PlataformaFugaz", new Color(0.9f, 0.25f, 0.2f, 1f), false);
        techo = Mat("Techo", new Color(0.35f, 0.37f, 0.42f, 1f), false);
        pasto = Mat("Pasto", new Color(0.3f, 0.6f, 0.3f, 1f), false);
    }

    static Material Mat(string nombre, Color color, bool sinLuz)
    {
        string ruta = "Assets/Mapa/Materiales/" + nombre + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (m == null)
        {
            m = new Material(Shader.Find(sinLuz ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(m, ruta);
        }
        m.color = color;
        if (!sinLuz) m.SetFloat("_Smoothness", 0.1f);
        EditorUtility.SetDirty(m);
        return m;
    }
}
