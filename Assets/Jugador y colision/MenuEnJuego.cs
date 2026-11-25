using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Base de los menus que se abren durante el juego (muerte y pausa): arma la pantalla y frena el juego.
public abstract class MenuEnJuego : MonoBehaviour
{
    protected GameObject pantalla;
    private Camara camara;
    private Jugador2 control;

    protected void Frenar()
    {
        Time.timeScale = 0f;
        camara = FindFirstObjectByType<Camara>();
        control = FindFirstObjectByType<Jugador2>();
        if (camara != null) camara.enabled = false;
        if (control != null) control.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected void Reanudar()
    {
        Time.timeScale = 1f;
        if (camara != null) camara.enabled = true;
        if (control != null) control.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected void SalirAlMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MenuPrincipal");
    }

    protected void ArmarPantalla(string titulo, Color colorFondo)
    {
        pantalla = new GameObject("Pantalla");
        pantalla.transform.SetParent(transform, false);

        Canvas canvas = pantalla.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler escala = pantalla.AddComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920f, 1080f);
        escala.matchWidthOrHeight = 0.5f;
        pantalla.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        Image fondo = CrearImagen("Fondo", colorFondo);
        fondo.rectTransform.anchorMin = Vector2.zero;
        fondo.rectTransform.anchorMax = Vector2.one;
        fondo.rectTransform.offsetMin = Vector2.zero;
        fondo.rectTransform.offsetMax = Vector2.zero;

        CrearTexto("Titulo", pantalla.transform, titulo, 110, Color.white, new Vector2(0f, 200f), new Vector2(1400f, 180f));
    }

    protected Button CrearBoton(string nombre, string contenido, float y, Color color, UnityAction accion)
    {
        Image imagen = CrearImagen(nombre, Color.white);
        imagen.rectTransform.anchoredPosition = new Vector2(0f, y);
        imagen.rectTransform.sizeDelta = new Vector2(520f, 120f);

        Button boton = imagen.gameObject.AddComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = color;
        colores.selectedColor = color;
        colores.highlightedColor = Color.Lerp(color, Color.white, 0.3f);
        colores.pressedColor = Color.Lerp(color, Color.black, 0.3f);
        boton.colors = colores;
        boton.onClick.AddListener(accion);

        CrearTexto("Texto", boton.transform, contenido, 54, Color.white, Vector2.zero, new Vector2(520f, 120f));
        return boton;
    }

    private Image CrearImagen(string nombre, Color color)
    {
        Image imagen = new GameObject(nombre).AddComponent<Image>();
        imagen.transform.SetParent(pantalla.transform, false);
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
