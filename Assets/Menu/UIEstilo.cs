using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Colores y "fabrica" de elementos de interfaz con el estilo del juego (neon cian sobre azul oscuro).
// Toda la interfaz de los menus se arma por codigo con estas funciones.
public static class UIEstilo
{
    public static readonly Color Cian = new Color(0f, 0.9f, 1f, 1f);
    public static readonly Color CianSuave = new Color(0f, 0.9f, 1f, 0.35f);
    public static readonly Color Violeta = new Color(0.55f, 0.35f, 1f, 1f);
    public static readonly Color FondoBoton = new Color(0.02f, 0.08f, 0.2f, 0.72f);
    public static readonly Color FondoBotonActivo = new Color(0f, 0.38f, 0.62f, 0.85f);
    public static readonly Color FondoPanel = new Color(0.01f, 0.04f, 0.12f, 0.9f);
    public static readonly Color TextoClaro = new Color(0.88f, 0.96f, 1f, 1f);
    public static readonly Color TextoApagado = new Color(0.55f, 0.68f, 0.8f, 1f);

    public static Canvas CrearCanvas(string nombre, int orden)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = orden;

        CanvasScaler escalador = go.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);
        escalador.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    public static RectTransform CrearObjeto(string nombre, Transform padre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return (RectTransform)go.transform;
    }

    public static void Estirar(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void Ubicar(RectTransform rt, Vector2 ancla, Vector2 pivote, Vector2 posicion, Vector2 tamano)
    {
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = pivote;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
    }

    public static Image CrearImagen(string nombre, Transform padre, Color color, bool bloqueaClicks = false)
    {
        RectTransform rt = CrearObjeto(nombre, padre);
        Image imagen = rt.gameObject.AddComponent<Image>();
        imagen.color = color;
        imagen.raycastTarget = bloqueaClicks;
        return imagen;
    }

    public static TextMeshProUGUI CrearTexto(string nombre, Transform padre, string texto, float tamano, Color color, TextAlignmentOptions alineacion)
    {
        RectTransform rt = CrearObjeto(nombre, padre);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        // Si el texto se crea dentro de un panel apagado, TextMesh Pro no carga la fuente solo
        if (tmp.font == null) tmp.font = TMP_Settings.defaultFontAsset;
        if (tmp.fontSharedMaterial == null && tmp.font != null) tmp.fontSharedMaterial = tmp.font.material;
        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.color = color;
        tmp.alignment = alineacion;
        tmp.raycastTarget = false;
        tmp.richText = true;
        return tmp;
    }

    // Brillo de neon alrededor del texto (usa el "underlay" del shader de TextMesh Pro)
    public static void AgregarBrillo(TextMeshProUGUI tmp, Color color, float suavidad = 0.6f, float grosor = 0.35f)
    {
        if (tmp.fontSharedMaterial == null) return;
        Material material = tmp.fontMaterial;
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, color);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, suavidad);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, grosor);
        tmp.fontMaterial = material;
        tmp.UpdateMeshPadding();
    }

    // Contenedor vertical que acomoda solos los botones uno debajo del otro
    public static RectTransform CrearColumna(string nombre, Transform padre, float ancho, float separacion)
    {
        RectTransform rt = CrearObjeto(nombre, padre);
        Ubicar(rt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(150f, -60f), new Vector2(ancho, 0f));

        VerticalLayoutGroup layout = rt.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = separacion;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter ajuste = rt.gameObject.AddComponent<ContentSizeFitter>();
        ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return rt;
    }

    public static void FijarAlto(GameObject go, float alto)
    {
        LayoutElement elemento = go.GetComponent<LayoutElement>();
        if (elemento == null) elemento = go.AddComponent<LayoutElement>();
        elemento.preferredHeight = alto;
        elemento.minHeight = alto;
    }

    public static TextMeshProUGUI CrearTituloDePanel(Transform columna, string texto)
    {
        TextMeshProUGUI titulo = CrearTexto("Titulo", columna, texto, 54, Cian, TextAlignmentOptions.MidlineLeft);
        titulo.fontStyle = FontStyles.Bold;
        titulo.characterSpacing = 6;
        AgregarBrillo(titulo, new Color(0f, 0.7f, 1f, 0.6f));
        FijarAlto(titulo.gameObject, 90);
        return titulo;
    }

    public static Button CrearBoton(Transform padre, string texto, Action alClick, float alto = 74f, float tamanoTexto = 34f)
    {
        RectTransform rt = CrearObjeto("Boton " + texto, padre);
        FijarAlto(rt.gameObject, alto);

        Image fondo = rt.gameObject.AddComponent<Image>();
        fondo.color = FondoBoton;

        Button boton = rt.gameObject.AddComponent<Button>();
        boton.transition = Selectable.Transition.None;
        boton.targetGraphic = fondo;
        if (alClick != null) boton.onClick.AddListener(() => alClick());

        // Barra de acento a la izquierda
        Image barra = CrearImagen("Barra", rt, Cian);
        RectTransform rtBarra = barra.rectTransform;
        rtBarra.anchorMin = new Vector2(0f, 0f);
        rtBarra.anchorMax = new Vector2(0f, 1f);
        rtBarra.pivot = new Vector2(0f, 0.5f);
        rtBarra.anchoredPosition = Vector2.zero;
        rtBarra.sizeDelta = new Vector2(4f, 0f);

        // Linea fina inferior
        Image linea = CrearImagen("Linea", rt, CianSuave);
        RectTransform rtLinea = linea.rectTransform;
        rtLinea.anchorMin = new Vector2(0f, 0f);
        rtLinea.anchorMax = new Vector2(1f, 0f);
        rtLinea.pivot = new Vector2(0.5f, 0f);
        rtLinea.anchoredPosition = Vector2.zero;
        rtLinea.sizeDelta = new Vector2(0f, 1.5f);

        TextMeshProUGUI tmp = CrearTexto("Texto", rt, texto, tamanoTexto, TextoClaro, TextAlignmentOptions.MidlineLeft);
        tmp.characterSpacing = 3;
        RectTransform rtTexto = tmp.rectTransform;
        Estirar(rtTexto);
        rtTexto.offsetMin = new Vector2(34f, 0f);
        rtTexto.offsetMax = new Vector2(-20f, 0f);

        BotonMenuEfecto efecto = rt.gameObject.AddComponent<BotonMenuEfecto>();
        efecto.Configurar(boton, fondo, rtBarra, rtTexto, tmp);

        return boton;
    }

    public static Button CrearBotonChico(Transform padre, string texto, Action alClick)
    {
        RectTransform rt = CrearObjeto("Boton " + texto, padre);
        Image fondo = rt.gameObject.AddComponent<Image>();
        fondo.color = FondoBoton;

        Button boton = rt.gameObject.AddComponent<Button>();
        boton.transition = Selectable.Transition.None;
        boton.targetGraphic = fondo;
        if (alClick != null) boton.onClick.AddListener(() => alClick());

        Image barra = CrearImagen("Barra", rt, Cian);
        RectTransform rtBarra = barra.rectTransform;
        rtBarra.anchorMin = new Vector2(0f, 0f);
        rtBarra.anchorMax = new Vector2(1f, 0f);
        rtBarra.pivot = new Vector2(0.5f, 0f);
        rtBarra.sizeDelta = new Vector2(0f, 2f);

        TextMeshProUGUI tmp = CrearTexto("Texto", rt, texto, 34, TextoClaro, TextAlignmentOptions.Center);
        tmp.fontStyle = FontStyles.Bold;
        Estirar(tmp.rectTransform);

        BotonMenuEfecto efecto = rt.gameObject.AddComponent<BotonMenuEfecto>();
        efecto.Configurar(boton, fondo, null, null, tmp);
        return boton;
    }

    // Textura de degradado horizontal (oscuro a la izquierda, transparente a la derecha)
    public static Texture2D CrearDegradado(Color izquierda, Color derecha)
    {
        Texture2D textura = new Texture2D(256, 1, TextureFormat.RGBA32, false);
        textura.wrapMode = TextureWrapMode.Clamp;
        for (int x = 0; x < 256; x++)
        {
            float t = x / 255f;
            textura.SetPixel(x, 0, Color.Lerp(izquierda, derecha, t * t * (3f - 2f * t)));
        }
        textura.Apply();
        return textura;
    }
}
