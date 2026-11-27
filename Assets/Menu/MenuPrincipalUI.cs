using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu principal nuevo: Continuar, Nueva partida, Cargar partida, Opciones y Salir.
// Se crea solo al abrir la escena MenuPrincipal (ver ArranqueMenus) y reemplaza al menu viejo.
public class MenuPrincipalUI : MenuUIBase
{
    private GameObject panelPrincipal;
    private Button botonContinuar;
    private RawImage fondo;

    void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Opciones.OlvidarCamara();
        Opciones.Aplicar();

        OcultarMenuViejo();
        Construir();
        MostrarPanel(panelPrincipal);
        StartCoroutine(Fundir(1f, 0f, 0.6f));
    }

    // Apaga el Canvas del menu anterior que esta guardado en la escena
    private void OcultarMenuViejo()
    {
        foreach (GameObject objeto in gameObject.scene.GetRootGameObjects())
        {
            if (objeto != gameObject && objeto.GetComponent<Canvas>() != null)
            {
                objeto.SetActive(false);
            }
        }
    }

    private void Construir()
    {
        CrearCanvasBase("Canvas Menu Principal", 50);

        // Fondo
        RectTransform rtFondo = UIEstilo.CrearObjeto("Fondo", raiz);
        UIEstilo.Estirar(rtFondo);
        fondo = rtFondo.gameObject.AddComponent<RawImage>();
        fondo.raycastTarget = false;
        Texture2D textura = Resources.Load<Texture2D>("FondoMenu");
        if (textura != null) fondo.texture = textura;
        else fondo.color = new Color(0.02f, 0.06f, 0.16f, 1f);

        // Oscurecido a la izquierda para que se lean los botones
        RectTransform rtSombra = UIEstilo.CrearObjeto("Sombra", raiz);
        UIEstilo.Estirar(rtSombra);
        RawImage sombra = rtSombra.gameObject.AddComponent<RawImage>();
        sombra.texture = UIEstilo.CrearDegradado(new Color(0f, 0.01f, 0.05f, 0.88f), new Color(0f, 0.01f, 0.05f, 0f));
        sombra.raycastTarget = false;
        rtSombra.anchorMax = new Vector2(0.62f, 1f);

        // Titulo
        TextMeshProUGUI titulo = UIEstilo.CrearTexto("Titulo", raiz, "BEHIND THE <color=#00E5FF>AI</color>", 112, Color.white, TextAlignmentOptions.TopLeft);
        titulo.fontStyle = FontStyles.Bold;
        titulo.characterSpacing = 8;
        UIEstilo.AgregarBrillo(titulo, new Color(0f, 0.75f, 1f, 0.75f), 0.7f, 0.45f);
        UIEstilo.Ubicar(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(150f, -90f), new Vector2(1100f, 140f));

        TextMeshProUGUI subtitulo = UIEstilo.CrearTexto("Subtitulo", raiz, "RECUPERA LOS RECUERDOS DE LA HUMANIDAD", 26, UIEstilo.Cian, TextAlignmentOptions.TopLeft);
        subtitulo.characterSpacing = 14;
        UIEstilo.Ubicar(subtitulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -232f), new Vector2(1100f, 40f));

        Image linea = UIEstilo.CrearImagen("Linea", raiz, UIEstilo.Cian);
        UIEstilo.Ubicar(linea.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -284f), new Vector2(120f, 3f));
        Image linea2 = UIEstilo.CrearImagen("Linea2", raiz, UIEstilo.CianSuave);
        UIEstilo.Ubicar(linea2.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(286f, -285f), new Vector2(480f, 1f));

        // Pie de pagina
        TextMeshProUGUI version = UIEstilo.CrearTexto("Version", raiz, "PROTOTIPO v1.1", 22, UIEstilo.TextoApagado, TextAlignmentOptions.BottomLeft);
        version.characterSpacing = 6;
        UIEstilo.Ubicar(version.rectTransform, Vector2.zero, Vector2.zero, new Vector2(156f, 40f), new Vector2(600f, 40f));

        TextMeshProUGUI ayuda = UIEstilo.CrearTexto("Ayuda", raiz, "ENTER  ACEPTAR      ESC  VOLVER", 22, UIEstilo.TextoApagado, TextAlignmentOptions.BottomRight);
        ayuda.characterSpacing = 4;
        UIEstilo.Ubicar(ayuda.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-60f, 40f), new Vector2(700f, 40f));

        // Panel principal
        panelPrincipal = CrearPanel("Panel Principal", 560f, null);
        botonContinuar = UIEstilo.CrearBoton(panelPrincipal.transform, "CONTINUAR", Continuar);
        UIEstilo.CrearBoton(panelPrincipal.transform, "NUEVA PARTIDA", NuevaPartida);
        UIEstilo.CrearBoton(panelPrincipal.transform, "CARGAR PARTIDA", () => AbrirPartidas(false));
        UIEstilo.CrearBoton(panelPrincipal.transform, "OPCIONES", () => MostrarPanel(panelOpciones));
        UIEstilo.CrearBoton(panelPrincipal.transform, "SALIR", () => Confirmar("¿Salir del juego?", SalirDelJuego));

        // Continuar solo aparece si hay alguna partida guardada
        botonContinuar.gameObject.SetActive(SistemaGuardado.HayAlgunaPartida());

        CrearPanelPartidas(() => MostrarPanel(panelPrincipal));
        CrearPanelOpciones(() => MostrarPanel(panelPrincipal));
        CrearVentanaConfirmacion();
        CrearMensajesYFundido();
    }

    protected override void Update()
    {
        base.Update();

        // Movimiento muy suave del fondo
        if (fondo != null)
        {
            float t = Time.unscaledTime * 0.05f;
            fondo.uvRect = new Rect(0.015f + Mathf.Sin(t) * 0.012f, 0.015f + Mathf.Cos(t * 0.8f) * 0.012f, 0.97f, 0.97f);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Volver();
        }
    }

    private void NuevaPartida()
    {
        SistemaGuardado.NuevaPartida();
        FundirYHacer(() => SceneManager.LoadScene(SistemaGuardado.EscenaJuego));
    }

    private void Continuar()
    {
        int ranura = SistemaGuardado.RanuraMasReciente();
        if (ranura >= 0) AlCargarRanura(ranura);
    }

    protected override void AlCargarRanura(int ranura)
    {
        if (!SistemaGuardado.PrepararCarga(ranura))
        {
            MostrarMensaje("No se pudo cargar la partida");
            return;
        }
        FundirYHacer(() => SceneManager.LoadScene(SistemaGuardado.EscenaACargar()));
    }
}
