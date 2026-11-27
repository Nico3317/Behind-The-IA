using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Base comun del menu principal y del menu de pausa:
// paneles con boton "Volver", panel de partidas (guardar/cargar), panel de opciones,
// ventana de confirmacion, mensajes y fundido a negro.
public abstract class MenuUIBase : MonoBehaviour
{
    protected Canvas canvas;
    protected RectTransform raiz;

    private readonly List<GameObject> paneles = new List<GameObject>();
    private readonly Dictionary<GameObject, Action> accionesVolver = new Dictionary<GameObject, Action>();
    private readonly Dictionary<GameObject, Func<Selectable>> primerElemento = new Dictionary<GameObject, Func<Selectable>>();
    protected GameObject panelActual;

    // Panel de partidas
    protected GameObject panelPartidas;
    private TextMeshProUGUI tituloPartidas;
    private readonly List<Button> botonesRanuras = new List<Button>();
    private bool partidasModoGuardar;

    // Panel de opciones
    protected GameObject panelOpciones;
    private readonly List<Action> refrescarOpciones = new List<Action>();

    // Confirmacion
    private GameObject ventanaConfirmacion;
    private TextMeshProUGUI textoConfirmacion;
    private Button botonSi;
    private Button botonNo;
    private Action accionConfirmar;
    private GameObject seleccionAntesDeConfirmar;

    // Mensajes y fundido
    private TextMeshProUGUI textoMensaje;
    private float tiempoMensaje;
    private CanvasGroup fundido;

    // ---------- Construccion ----------

    protected void CrearCanvasBase(string nombre, int orden)
    {
        AsegurarEventSystem();
        canvas = UIEstilo.CrearCanvas(nombre, orden);
        canvas.transform.SetParent(transform, false);
        raiz = (RectTransform)canvas.transform;
    }

    protected void AsegurarEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    protected GameObject CrearPanel(string nombre, float ancho, Action alVolver)
    {
        RectTransform columna = UIEstilo.CrearColumna(nombre, raiz, ancho, 14f);
        GameObject panel = columna.gameObject;
        paneles.Add(panel);
        if (alVolver != null) accionesVolver[panel] = alVolver;
        panel.SetActive(false);
        return panel;
    }

    protected void DefinirPrimerElemento(GameObject panel, Func<Selectable> primero)
    {
        primerElemento[panel] = primero;
    }

    protected void MostrarPanel(GameObject panel)
    {
        foreach (GameObject p in paneles)
        {
            if (p != null) p.SetActive(p == panel);
        }
        panelActual = panel;

        if (panel == panelPartidas) RefrescarPartidas();
        if (panel == panelOpciones) RefrescarOpciones();

        StartCoroutine(SeleccionarAlSiguienteFrame(panel));
    }

    private IEnumerator SeleccionarAlSiguienteFrame(GameObject panel)
    {
        yield return null;
        if (panel != panelActual || EventSystem.current == null) yield break;

        Selectable elegido = null;
        Func<Selectable> primero;
        if (primerElemento.TryGetValue(panel, out primero)) elegido = primero();

        if (elegido == null || !elegido.interactable)
        {
            foreach (Selectable s in panel.GetComponentsInChildren<Selectable>())
            {
                if (s.interactable) { elegido = s; break; }
            }
        }

        if (elegido != null) EventSystem.current.SetSelectedGameObject(elegido.gameObject);
    }

    protected void OcultarPaneles()
    {
        foreach (GameObject p in paneles)
        {
            if (p != null) p.SetActive(false);
        }
        panelActual = null;
    }

    // Esc / boton atras: cierra la confirmacion o vuelve al panel anterior
    protected bool Volver()
    {
        if (ventanaConfirmacion != null && ventanaConfirmacion.activeSelf)
        {
            CerrarConfirmacion();
            return true;
        }

        Action accion;
        if (panelActual != null && accionesVolver.TryGetValue(panelActual, out accion))
        {
            accion();
            return true;
        }
        return false;
    }

    // ---------- Panel de partidas ----------

    protected void CrearPanelPartidas(Action alVolver)
    {
        panelPartidas = CrearPanel("Panel Partidas", 720f, alVolver);
        tituloPartidas = UIEstilo.CrearTituloDePanel(panelPartidas.transform, "PARTIDAS");

        botonesRanuras.Clear();
        for (int i = 0; i < SistemaGuardado.CantidadRanuras; i++)
        {
            int ranura = i;
            Button boton = UIEstilo.CrearBoton(panelPartidas.transform, "", () => AlElegirRanura(ranura), 112f, 34f);
            botonesRanuras.Add(boton);
        }

        RectTransform espacio = UIEstilo.CrearObjeto("Espacio", panelPartidas.transform);
        UIEstilo.FijarAlto(espacio.gameObject, 10f);

        UIEstilo.CrearBoton(panelPartidas.transform, "VOLVER", () => Volver());
    }

    protected void AbrirPartidas(bool modoGuardar)
    {
        partidasModoGuardar = modoGuardar;
        MostrarPanel(panelPartidas);
    }

    private void RefrescarPartidas()
    {
        if (tituloPartidas != null) tituloPartidas.text = partidasModoGuardar ? "GUARDAR PARTIDA" : "CARGAR PARTIDA";

        for (int i = 0; i < botonesRanuras.Count; i++)
        {
            DatosPartida datos = SistemaGuardado.LeerRanura(i);
            TextMeshProUGUI texto = botonesRanuras[i].GetComponentInChildren<TextMeshProUGUI>();

            string detalle;
            if (datos != null)
            {
                detalle = datos.fecha + "   ·   Llaves " + datos.llaves.Count + "/3   ·   " + SistemaGuardado.FormatearTiempo(datos.tiempoJugado) + " jugado";
            }
            else
            {
                detalle = partidasModoGuardar ? "Vacia - guardar aca" : "Vacia";
            }

            texto.text = "<b>PARTIDA " + (i + 1) + "</b>\n<size=24><color=#8FB3CC>" + detalle + "</color></size>";

            // Para cargar, las ranuras vacias no se pueden elegir
            botonesRanuras[i].interactable = partidasModoGuardar || datos != null;
        }
    }

    private void AlElegirRanura(int ranura)
    {
        if (partidasModoGuardar)
        {
            if (SistemaGuardado.ExisteRanura(ranura))
            {
                Confirmar("¿Sobrescribir la Partida " + (ranura + 1) + "?", () => AlGuardarEnRanura(ranura));
            }
            else
            {
                AlGuardarEnRanura(ranura);
            }
        }
        else if (SistemaGuardado.ExisteRanura(ranura))
        {
            AlCargarRanura(ranura);
        }
    }

    protected virtual void AlGuardarEnRanura(int ranura) { }
    protected abstract void AlCargarRanura(int ranura);

    // ---------- Panel de opciones ----------

    protected void CrearPanelOpciones(Action alVolver)
    {
        panelOpciones = CrearPanel("Panel Opciones", 860f, () => { Opciones.GuardarEnDisco(); alVolver(); });
        UIEstilo.CrearTituloDePanel(panelOpciones.transform, "OPCIONES");
        refrescarOpciones.Clear();

        CrearSelector("VOLUMEN",
            () => Mathf.RoundToInt(Opciones.Volumen * 100f) + "%",
            () => Opciones.Volumen,
            () => CambiarOpcion(() => Opciones.Volumen = Mathf.Round((Opciones.Volumen - 0.1f) * 10f) / 10f),
            () => CambiarOpcion(() => Opciones.Volumen = Mathf.Round((Opciones.Volumen + 0.1f) * 10f) / 10f));

        CrearSelector("SENSIBILIDAD",
            () => Opciones.Sensibilidad.ToString("0.0") + "x",
            () => Mathf.InverseLerp(0.2f, 2f, Opciones.Sensibilidad),
            () => CambiarOpcion(() => Opciones.Sensibilidad = Mathf.Round((Opciones.Sensibilidad - 0.1f) * 10f) / 10f),
            () => CambiarOpcion(() => Opciones.Sensibilidad = Mathf.Round((Opciones.Sensibilidad + 0.1f) * 10f) / 10f));

        CrearSelector("CALIDAD",
            () => Opciones.NombreCalidad().ToUpper(),
            null,
            () => CambiarOpcion(() => Opciones.Calidad = Opciones.Calidad - 1),
            () => CambiarOpcion(() => Opciones.Calidad = Opciones.Calidad + 1));

        CrearSelector("PANTALLA COMPLETA",
            () => Opciones.PantallaCompleta ? "SI" : "NO",
            null,
            () => CambiarOpcion(() => Opciones.PantallaCompleta = !Opciones.PantallaCompleta),
            () => CambiarOpcion(() => Opciones.PantallaCompleta = !Opciones.PantallaCompleta));

        RectTransform espacio = UIEstilo.CrearObjeto("Espacio", panelOpciones.transform);
        UIEstilo.FijarAlto(espacio.gameObject, 10f);

        UIEstilo.CrearBoton(panelOpciones.transform, "RESTABLECER", () =>
        {
            Opciones.Volumen = 0.8f;
            Opciones.Sensibilidad = 1f;
            Opciones.Aplicar();
            RefrescarOpciones();
            MostrarMensaje("Opciones restablecidas");
        });
        UIEstilo.CrearBoton(panelOpciones.transform, "VOLVER", () => Volver());
    }

    private void CambiarOpcion(Action cambio)
    {
        cambio();
        Opciones.Aplicar();
        RefrescarOpciones();
    }

    private void RefrescarOpciones()
    {
        foreach (Action refrescar in refrescarOpciones) refrescar();
    }

    // Fila de opcion:  ETIQUETA ............ [<]  valor  [>]
    private void CrearSelector(string etiqueta, Func<string> valor, Func<float> relleno, Action menos, Action mas)
    {
        RectTransform fila = UIEstilo.CrearObjeto("Opcion " + etiqueta, panelOpciones.transform);
        UIEstilo.FijarAlto(fila.gameObject, 74f);
        Image fondo = fila.gameObject.AddComponent<Image>();
        fondo.color = new Color(0.02f, 0.08f, 0.2f, 0.55f);
        fondo.raycastTarget = false;

        TextMeshProUGUI textoEtiqueta = UIEstilo.CrearTexto("Etiqueta", fila, etiqueta, 30, UIEstilo.TextoClaro, TextAlignmentOptions.MidlineLeft);
        textoEtiqueta.characterSpacing = 3;
        RectTransform rtEtiqueta = textoEtiqueta.rectTransform;
        UIEstilo.Estirar(rtEtiqueta);
        rtEtiqueta.offsetMin = new Vector2(30f, 0f);
        rtEtiqueta.offsetMax = new Vector2(-420f, 0f);

        Button botonMenos = UIEstilo.CrearBotonChico(fila, "<", menos);
        UIEstilo.Ubicar((RectTransform)botonMenos.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-340f, 0f), new Vector2(60f, 56f));

        Button botonMas = UIEstilo.CrearBotonChico(fila, ">", mas);
        UIEstilo.Ubicar((RectTransform)botonMas.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(60f, 56f));

        // Barra de valor (solo para opciones numericas)
        RectTransform rtRelleno = null;
        if (relleno != null)
        {
            Image barraFondo = UIEstilo.CrearImagen("BarraFondo", fila, new Color(0f, 0.9f, 1f, 0.12f));
            UIEstilo.Ubicar(barraFondo.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-94f, -20f), new Vector2(226f, 6f));
            Image barraRelleno = UIEstilo.CrearImagen("BarraRelleno", barraFondo.transform, UIEstilo.Cian);
            rtRelleno = barraRelleno.rectTransform;
            rtRelleno.anchorMin = new Vector2(0f, 0f);
            rtRelleno.anchorMax = new Vector2(1f, 1f);
            rtRelleno.offsetMin = Vector2.zero;
            rtRelleno.offsetMax = Vector2.zero;
        }

        TextMeshProUGUI textoValor = UIEstilo.CrearTexto("Valor", fila, "", 30, UIEstilo.Cian, TextAlignmentOptions.Center);
        textoValor.fontStyle = FontStyles.Bold;
        UIEstilo.Ubicar(textoValor.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-94f, relleno != null ? 6f : 0f), new Vector2(226f, 50f));

        RectTransform rellenoFinal = rtRelleno;
        refrescarOpciones.Add(() =>
        {
            textoValor.text = valor();
            if (rellenoFinal != null) rellenoFinal.anchorMax = new Vector2(Mathf.Clamp01(relleno()), 1f);
        });
    }

    // ---------- Confirmacion ----------

    protected void CrearVentanaConfirmacion()
    {
        Image oscuro = UIEstilo.CrearImagen("Confirmacion", raiz, new Color(0f, 0.01f, 0.04f, 0.75f), true);
        UIEstilo.Estirar(oscuro.rectTransform);
        ventanaConfirmacion = oscuro.gameObject;

        Image caja = UIEstilo.CrearImagen("Caja", oscuro.transform, UIEstilo.FondoPanel, true);
        UIEstilo.Ubicar(caja.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(820f, 330f));

        Image borde = UIEstilo.CrearImagen("Borde", caja.transform, UIEstilo.Cian);
        RectTransform rtBorde = borde.rectTransform;
        rtBorde.anchorMin = new Vector2(0f, 1f);
        rtBorde.anchorMax = new Vector2(1f, 1f);
        rtBorde.pivot = new Vector2(0.5f, 1f);
        rtBorde.sizeDelta = new Vector2(0f, 4f);

        textoConfirmacion = UIEstilo.CrearTexto("Pregunta", caja.transform, "", 38, UIEstilo.TextoClaro, TextAlignmentOptions.Center);
        UIEstilo.Ubicar(textoConfirmacion.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(740f, 150f));

        botonSi = UIEstilo.CrearBoton(caja.transform, "SI", () =>
        {
            Action accion = accionConfirmar;
            CerrarConfirmacion();
            if (accion != null) accion();
        });
        UIEstilo.Ubicar((RectTransform)botonSi.transform, new Vector2(0.5f, 0f), new Vector2(1f, 0f), new Vector2(-15f, 36f), new Vector2(300f, 74f));

        botonNo = UIEstilo.CrearBoton(caja.transform, "NO", () => CerrarConfirmacion());
        UIEstilo.Ubicar((RectTransform)botonNo.transform, new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(15f, 36f), new Vector2(300f, 74f));

        ventanaConfirmacion.SetActive(false);
    }

    protected void Confirmar(string pregunta, Action alConfirmar)
    {
        accionConfirmar = alConfirmar;
        textoConfirmacion.text = pregunta;
        seleccionAntesDeConfirmar = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        ventanaConfirmacion.transform.SetAsLastSibling();
        ventanaConfirmacion.SetActive(true);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(botonNo.gameObject);
    }

    private void CerrarConfirmacion()
    {
        ventanaConfirmacion.SetActive(false);
        accionConfirmar = null;
        if (EventSystem.current != null && seleccionAntesDeConfirmar != null && seleccionAntesDeConfirmar.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(seleccionAntesDeConfirmar);
        }
    }

    // ---------- Mensajes y fundido ----------

    protected void CrearMensajesYFundido()
    {
        textoMensaje = UIEstilo.CrearTexto("Mensaje", raiz, "", 34, UIEstilo.Cian, TextAlignmentOptions.Center);
        UIEstilo.Ubicar(textoMensaje.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 70f), new Vector2(1200f, 60f));
        UIEstilo.AgregarBrillo(textoMensaje, new Color(0f, 0.6f, 1f, 0.5f));
        textoMensaje.alpha = 0f;

        Image negro = UIEstilo.CrearImagen("Fundido", raiz, Color.black);
        UIEstilo.Estirar(negro.rectTransform);
        fundido = negro.gameObject.AddComponent<CanvasGroup>();
        fundido.alpha = 0f;
        fundido.blocksRaycasts = false;
        fundido.interactable = false;
    }

    protected void MostrarMensaje(string mensaje)
    {
        if (textoMensaje == null) return;
        textoMensaje.text = mensaje;
        textoMensaje.transform.SetAsLastSibling();
        if (fundido != null) fundido.transform.SetAsLastSibling();
        tiempoMensaje = 2.5f;
    }

    protected virtual void Update()
    {
        if (textoMensaje != null)
        {
            if (tiempoMensaje > 0f) tiempoMensaje -= Time.unscaledDeltaTime;
            textoMensaje.alpha = Mathf.Clamp01(tiempoMensaje / 0.5f);
        }
    }

    protected IEnumerator Fundir(float desde, float hasta, float duracion)
    {
        if (fundido == null) yield break;
        fundido.transform.SetAsLastSibling();
        fundido.blocksRaycasts = true;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            fundido.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        fundido.alpha = hasta;
        fundido.blocksRaycasts = hasta > 0.01f;
    }

    protected void FundirYHacer(Action accion)
    {
        StartCoroutine(FundirYHacerRutina(accion));
    }

    private IEnumerator FundirYHacerRutina(Action accion)
    {
        yield return Fundir(0f, 1f, 0.35f);
        accion();
    }

    protected void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
