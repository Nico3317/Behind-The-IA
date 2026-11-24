using UnityEngine;
using UnityEngine.UI;

// Vida del jugador con su barra en pantalla. Los enemigos se la agregan solos al Player.
public class VidaJugador : MonoBehaviour
{
    public int vidaMaxima = 100;

    private int vida;
    private Image relleno;
    private Text texto;
    private GameObject barra;

    public int Vida
    {
        get { return vida; }
    }

    void Awake()
    {
        vida = vidaMaxima;
        ArmarBarra();
        ActualizarBarra();
    }

    public void RecibirDanio(int cantidad)
    {
        ReaparecerJugador reaparecer = GetComponent<ReaparecerJugador>();
        if (MenuMuerte.Activo || vida <= 0) return;
        if (reaparecer != null && reaparecer.EsInvulnerable) return;

        vida = Mathf.Max(0, vida - cantidad);

        if (vida == 0 && reaparecer != null)
        {
            reaparecer.Reaparecer();
            // Si no hay menu de muerte, reaparece con la vida llena
            if (!MenuMuerte.Activo) vida = vidaMaxima;
        }
        ActualizarBarra();
    }

    void OnDestroy()
    {
        if (barra != null) Destroy(barra);
    }

    private void ActualizarBarra()
    {
        float porcentaje = (float)vida / vidaMaxima;
        relleno.rectTransform.anchorMax = new Vector2(porcentaje, 1f);
        relleno.color = Color.Lerp(new Color(0.85f, 0.15f, 0.15f, 1f), new Color(0.2f, 0.8f, 0.3f, 1f), porcentaje);
        texto.text = "Vida " + vida;
    }

    private void ArmarBarra()
    {
        barra = new GameObject("BarraVida");
        Canvas canvas = barra.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler escala = barra.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);

        // Abajo a la izquierda
        Image fondo = new GameObject("Fondo").AddComponent<Image>();
        fondo.transform.SetParent(barra.transform, false);
        fondo.color = new Color(0f, 0f, 0f, 0.7f);
        fondo.raycastTarget = false;
        fondo.rectTransform.anchorMin = Vector2.zero;
        fondo.rectTransform.anchorMax = Vector2.zero;
        fondo.rectTransform.pivot = Vector2.zero;
        fondo.rectTransform.anchoredPosition = new Vector2(40f, 40f);
        fondo.rectTransform.sizeDelta = new Vector2(400f, 50f);

        relleno = new GameObject("Relleno").AddComponent<Image>();
        relleno.transform.SetParent(fondo.transform, false);
        relleno.raycastTarget = false;
        Estirar(relleno.rectTransform);

        texto = new GameObject("Texto").AddComponent<Text>();
        texto.transform.SetParent(fondo.transform, false);
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.fontSize = 30;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.color = Color.white;
        texto.raycastTarget = false;
        Estirar(texto.rectTransform);
    }

    private void Estirar(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
