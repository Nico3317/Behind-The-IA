using UnityEngine;
using UnityEngine.UI;

// HUD del juego: llaves conseguidas, a que distancia esta cada una, objetivo actual y aviso de la tecla E.
public class HudLlaves : MonoBehaviour
{
    [Header("Llaves (en el mismo orden que los colores)")]
    public DataItemInventario[] llaves;
    public Color[] colores;
    [Tooltip("Puerta del edificio final, para mostrar la distancia cuando ya estan las tres llaves.")]
    public Transform puertaFinal;

    private Transform jugador;
    private RecogerRecompensa[] objetos;
    private Image[] iconos;
    private Image[] marcos;
    private Text[] estados;
    private Text objetivo;
    private GameObject aviso;
    private Text textoAviso;
    private GameObject pantalla;

    void Start()
    {
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) jugador = obj.transform;

        // Busco en la escena el objeto de cada llave
        RecogerRecompensa[] todos = FindObjectsByType<RecogerRecompensa>(FindObjectsSortMode.None);
        objetos = new RecogerRecompensa[llaves.Length];
        for (int i = 0; i < llaves.Length; i++)
        {
            foreach (RecogerRecompensa r in todos)
            {
                if (r.ItemData == llaves[i]) objetos[i] = r;
            }
        }

        Armar();
    }

    void Update()
    {
        if (SistemaInventario.Instance == null || jugador == null) return;

        int conseguidas = 0;
        bool cerca = false;

        for (int i = 0; i < llaves.Length; i++)
        {
            bool tiene = SistemaInventario.Instance.Get(llaves[i]) != null;
            if (tiene) conseguidas++;

            iconos[i].color = tiene ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.8f);
            marcos[i].color = tiene ? colores[i] : new Color(0f, 0f, 0f, 0.7f);
            estados[i].color = colores[i];

            if (tiene) estados[i].text = "¡La tenés!";
            else if (objetos[i] != null)
            {
                estados[i].text = Mathf.RoundToInt(Vector3.Distance(jugador.position, Centro(objetos[i]))) + " m";
                if (objetos[i].JugadorCerca)
                {
                    cerca = true;
                    textoAviso.text = "Presioná  [ E ]  para agarrar la llave";
                }
            }
        }

        if (conseguidas < llaves.Length)
        {
            objetivo.text = "Llaves " + conseguidas + " / " + llaves.Length + "  -  Seguí las columnas de color para encontrarlas";
        }
        else
        {
            string distancia = puertaFinal != null ? " (" + Mathf.RoundToInt(Vector3.Distance(jugador.position, puertaFinal.position)) + " m)" : "";
            objetivo.text = "¡Tenés las 3 llaves!  Andá a la puerta del edificio final" + distancia;
        }

        // No se muestra con el juego frenado (pausa o muerte)
        aviso.SetActive(cerca && Time.timeScale > 0f);
    }

    private Vector3 Centro(RecogerRecompensa llave)
    {
        Renderer render = llave.GetComponentInChildren<Renderer>();
        return render != null ? render.bounds.center : llave.transform.position;
    }

    private void Armar()
    {
        pantalla = new GameObject("PantallaHud");
        pantalla.transform.SetParent(transform, false);
        Canvas canvas = pantalla.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        CanvasScaler escala = pantalla.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;

        Vector2 arriba = new Vector2(0.5f, 1f);
        iconos = new Image[llaves.Length];
        marcos = new Image[llaves.Length];
        estados = new Text[llaves.Length];

        for (int i = 0; i < llaves.Length; i++)
        {
            float x = (i - (llaves.Length - 1) * 0.5f) * 150f;
            marcos[i] = Imagen("Marco" + (i + 1), pantalla.transform, arriba, new Vector2(x, -75f), new Vector2(110f, 110f));
            iconos[i] = Imagen("Icono", marcos[i].transform, Vector2.one * 0.5f, Vector2.zero, new Vector2(96f, 96f));
            iconos[i].sprite = llaves[i].icon;
            estados[i] = Texto("Estado", pantalla.transform, arriba, new Vector2(x, -152f), new Vector2(150f, 40f), 28);
        }

        Image fondoObjetivo = Imagen("FondoObjetivo", pantalla.transform, arriba, new Vector2(0f, -200f), new Vector2(1000f, 46f));
        fondoObjetivo.color = new Color(0f, 0f, 0f, 0.55f);
        objetivo = Texto("Objetivo", fondoObjetivo.transform, Vector2.one * 0.5f, Vector2.zero, new Vector2(1000f, 46f), 28);

        Image fondoAviso = Imagen("AvisoTecla", pantalla.transform, new Vector2(0.5f, 0f), new Vector2(0f, 230f), new Vector2(760f, 80f));
        fondoAviso.color = new Color(0f, 0f, 0f, 0.75f);
        aviso = fondoAviso.gameObject;
        textoAviso = Texto("Texto", aviso.transform, Vector2.one * 0.5f, Vector2.zero, new Vector2(760f, 80f), 40);
        textoAviso.color = new Color(1f, 0.9f, 0.3f, 1f);
        aviso.SetActive(false);

        Text controles = Texto("Controles", pantalla.transform, new Vector2(1f, 0f), new Vector2(-260f, 60f), new Vector2(480f, 40f), 26);
        controles.text = "E: agarrar llave    Esc: pausa";
        controles.alignment = TextAnchor.MiddleRight;
    }

    private Image Imagen(string nombre, Transform padre, Vector2 ancla, Vector2 posicion, Vector2 medida)
    {
        Image imagen = new GameObject(nombre).AddComponent<Image>();
        imagen.transform.SetParent(padre, false);
        imagen.raycastTarget = false;
        Ubicar(imagen.rectTransform, ancla, posicion, medida);
        return imagen;
    }

    private Text Texto(string nombre, Transform padre, Vector2 ancla, Vector2 posicion, Vector2 medida, int tamanio)
    {
        Text texto = new GameObject(nombre).AddComponent<Text>();
        texto.transform.SetParent(padre, false);
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = tamanio;
        texto.fontStyle = FontStyle.Bold;
        texto.color = Color.white;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.raycastTarget = false;
        texto.gameObject.AddComponent<Outline>().effectColor = Color.black;
        Ubicar(texto.rectTransform, ancla, posicion, medida);
        return texto;
    }

    private void Ubicar(RectTransform rect, Vector2 ancla, Vector2 posicion, Vector2 medida)
    {
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = medida;
    }
}
