using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu que aparece cuando un enemigo atrapa al jugador. Se crea solo, no hace falta ponerlo en la escena.
public class MenuMuerte : MonoBehaviour
{
    private static MenuMuerte actual;

    public static bool Activo
    {
        get { return actual != null; }
    }

    public static void Mostrar(GameObject jugador)
    {
        if (Activo) return;

        actual = new GameObject("MenuMuerte").AddComponent<MenuMuerte>();
        actual.ArmarMenu();

        // Freno el juego y suelto el mouse para poder tocar el boton
        Time.timeScale = 0f;
        Camara camara = FindFirstObjectByType<Camara>();
        if (camara != null) camara.enabled = false;
        Jugador2 control = jugador.GetComponentInChildren<Jugador2>();
        if (control != null) control.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void VolverAJugar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        // Por si se sale de la escena sin tocar el boton
        Time.timeScale = 1f;
        if (actual == this) actual = null;
    }

    private void ArmarMenu()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler escala = gameObject.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        Image fondo = CrearImagen("Fondo", transform, new Color(0.05f, 0f, 0f, 0.8f));
        fondo.rectTransform.anchorMin = Vector2.zero;
        fondo.rectTransform.anchorMax = Vector2.one;
        fondo.rectTransform.offsetMin = Vector2.zero;
        fondo.rectTransform.offsetMax = Vector2.zero;

        CrearTexto("Titulo", transform, "¡Te atraparon!", 110, new Color(0.95f, 0.2f, 0.2f, 1f), new Vector2(0f, 140f), new Vector2(1400f, 180f));

        Image imagenBoton = CrearImagen("BotonVolver", transform, Color.white);
        imagenBoton.rectTransform.anchoredPosition = new Vector2(0f, -80f);
        imagenBoton.rectTransform.sizeDelta = new Vector2(520f, 120f);

        Button boton = imagenBoton.gameObject.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = new Color(0.8f, 0.15f, 0.15f, 1f);
        colores.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
        colores.selectedColor = colores.normalColor;
        colores.pressedColor = new Color(0.55f, 0.08f, 0.08f, 1f);
        boton.colors = colores;
        boton.onClick.AddListener(VolverAJugar);

        CrearTexto("Texto", boton.transform, "Volver a jugar", 54, Color.white, Vector2.zero, new Vector2(520f, 120f));

        // Asi tambien se puede elegir con Enter o joystick
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(boton.gameObject);
    }

    private Image CrearImagen(string nombre, Transform padre, Color color)
    {
        Image imagen = new GameObject(nombre).AddComponent<Image>();
        imagen.transform.SetParent(padre, false);
        imagen.color = color;
        return imagen;
    }

    private void CrearTexto(string nombre, Transform padre, string contenido, int tamanio, Color color, Vector2 posicion, Vector2 medida)
    {
        Text texto = new GameObject(nombre).AddComponent<Text>();
        texto.transform.SetParent(padre, false);
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.text = contenido;
        texto.fontSize = tamanio;
        texto.color = color;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.raycastTarget = false;
        texto.rectTransform.anchoredPosition = posicion;
        texto.rectTransform.sizeDelta = medida;
    }
}
