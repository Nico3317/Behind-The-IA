using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu de pausa del juego (tecla Esc): Continuar, Guardar partida, Cargar partida,
// Opciones, Menu principal y Salir. Tambien aplica la partida cargada al empezar la escena.
public class MenuPausa : MenuUIBase
{
    private GameObject panelPausa;
    private GameObject contenidoPausa;
    private bool pausado;
    private float tiempoJugado;

    private readonly List<Behaviour> controlesDesactivados = new List<Behaviour>();

    void Start()
    {
        Opciones.OlvidarCamara();
        Opciones.Aplicar();

        Construir();
        contenidoPausa.SetActive(false);

        StartCoroutine(AplicarPartidaCargada());
    }

    private void Construir()
    {
        CrearCanvasBase("Canvas Menu Pausa", 200);

        // Todo lo visual de la pausa va en un contenedor que se prende y apaga
        RectTransform contenido = UIEstilo.CrearObjeto("Contenido Pausa", raiz);
        UIEstilo.Estirar(contenido);
        contenidoPausa = contenido.gameObject;

        Image oscuro = UIEstilo.CrearImagen("Fondo Pausa", contenido, new Color(0f, 0.01f, 0.05f, 0.78f), true);
        UIEstilo.Estirar(oscuro.rectTransform);

        RectTransform rtSombra = UIEstilo.CrearObjeto("Sombra", contenido);
        UIEstilo.Estirar(rtSombra);
        rtSombra.anchorMax = new Vector2(0.6f, 1f);
        RawImage sombra = rtSombra.gameObject.AddComponent<RawImage>();
        sombra.texture = UIEstilo.CrearDegradado(new Color(0f, 0.02f, 0.08f, 0.85f), new Color(0f, 0.02f, 0.08f, 0f));
        sombra.raycastTarget = false;

        TextMeshProUGUI titulo = UIEstilo.CrearTexto("Titulo", contenido, "PAUSA", 96, Color.white, TextAlignmentOptions.TopLeft);
        titulo.fontStyle = FontStyles.Bold;
        titulo.characterSpacing = 12;
        UIEstilo.AgregarBrillo(titulo, new Color(0f, 0.75f, 1f, 0.7f), 0.7f, 0.45f);
        UIEstilo.Ubicar(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(150f, -90f), new Vector2(900f, 130f));

        TextMeshProUGUI subtitulo = UIEstilo.CrearTexto("Subtitulo", contenido, "BEHIND THE AI", 26, UIEstilo.Cian, TextAlignmentOptions.TopLeft);
        subtitulo.characterSpacing = 14;
        UIEstilo.Ubicar(subtitulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(156f, -215f), new Vector2(900f, 40f));

        panelPausa = CrearPanel("Panel Pausa", 560f, Reanudar);
        UIEstilo.CrearBoton(panelPausa.transform, "CONTINUAR", Reanudar);
        UIEstilo.CrearBoton(panelPausa.transform, "GUARDAR PARTIDA", () => AbrirPartidas(true));
        UIEstilo.CrearBoton(panelPausa.transform, "CARGAR PARTIDA", () => AbrirPartidas(false));
        UIEstilo.CrearBoton(panelPausa.transform, "OPCIONES", () => MostrarPanel(panelOpciones));
        UIEstilo.CrearBoton(panelPausa.transform, "MENU PRINCIPAL", () =>
            Confirmar("¿Volver al menu principal?\n<size=28><color=#8FB3CC>Se pierde lo que no hayas guardado.</color></size>", IrAlMenu));
        UIEstilo.CrearBoton(panelPausa.transform, "SALIR DEL JUEGO", () =>
            Confirmar("¿Salir del juego?\n<size=28><color=#8FB3CC>Se pierde lo que no hayas guardado.</color></size>", SalirDelJuego));

        CrearPanelPartidas(() => MostrarPanel(panelPausa));
        CrearPanelOpciones(() => MostrarPanel(panelPausa));
        CrearVentanaConfirmacion();
        CrearMensajesYFundido();
    }

    protected override void Update()
    {
        base.Update();

        if (!pausado) tiempoJugado += Time.unscaledDeltaTime;

        if (Input.GetKeyDown(KeyCode.Escape) && !MenuMuerte.Activo)
        {
            if (!pausado) Pausar();
            else Volver();
        }
    }

    // ---------- Pausa ----------

    private void Pausar()
    {
        pausado = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Apago la camara y el control del jugador para que no se muevan mientras esta el menu
        controlesDesactivados.Clear();
        DesactivarControl(FindFirstObjectByType<Camara>());
        DesactivarControl(FindFirstObjectByType<Jugador2>());

        contenidoPausa.SetActive(true);
        MostrarPanel(panelPausa);
    }

    private void DesactivarControl(Behaviour control)
    {
        if (control != null && control.enabled)
        {
            control.enabled = false;
            controlesDesactivados.Add(control);
        }
    }

    private void Reanudar()
    {
        pausado = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        foreach (Behaviour control in controlesDesactivados)
        {
            if (control != null) control.enabled = true;
        }
        controlesDesactivados.Clear();

        OcultarPaneles();
        contenidoPausa.SetActive(false);
    }

    private void IrAlMenu()
    {
        FundirYHacer(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SistemaGuardado.EscenaMenu);
        });
    }

    // ---------- Guardar y cargar ----------

    protected override void AlGuardarEnRanura(int ranura)
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            MostrarMensaje("No se encontro al jugador");
            return;
        }

        DatosPartida datos = new DatosPartida();
        datos.escena = SceneManager.GetActiveScene().name;
        Vector3 posicion = jugador.transform.position;
        datos.posX = posicion.x;
        datos.posY = posicion.y;
        datos.posZ = posicion.z;
        datos.rotacionY = jugador.transform.eulerAngles.y;
        datos.tiempoJugado = tiempoJugado;

        if (SistemaInventario.Instance != null)
        {
            foreach (InventoryItem item in SistemaInventario.Instance.inventario)
            {
                if (item.Data != null) datos.llaves.Add(item.Data.name);
            }
        }

        if (SistemaGuardado.Guardar(ranura, datos))
        {
            // Guardar funciona como punto de control: si te atrapan, reapareces aca
            ReaparecerJugador reaparecer = ObtenerReaparecer(jugador);
            reaparecer.FijarPuntoDeReaparicion(posicion);

            MostrarMensaje("Partida guardada en la Partida " + (ranura + 1));
            MostrarPanel(panelPausa);
        }
        else
        {
            MostrarMensaje("No se pudo guardar la partida");
        }
    }

    protected override void AlCargarRanura(int ranura)
    {
        if (!SistemaGuardado.PrepararCarga(ranura))
        {
            MostrarMensaje("No se pudo cargar la partida");
            return;
        }

        FundirYHacer(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SistemaGuardado.EscenaACargar());
        });
    }

    private IEnumerator AplicarPartidaCargada()
    {
        // Espero a que la ciudad, los enemigos y el inventario terminen de crearse
        yield return null;
        yield return null;

        DatosPartida datos = SistemaGuardado.TomarPartidaPendiente();
        if (datos == null) yield break;

        tiempoJugado = datos.tiempoJugado;

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            Vector3 posicion = new Vector3(datos.posX, datos.posY, datos.posZ);
            CharacterController control = jugador.GetComponent<CharacterController>();
            if (control != null) control.enabled = false;
            jugador.transform.position = posicion;
            jugador.transform.rotation = Quaternion.Euler(0f, datos.rotacionY, 0f);
            if (control != null) control.enabled = true;

            ReaparecerJugador reaparecer = ObtenerReaparecer(jugador);
            reaparecer.FijarPuntoDeReaparicion(posicion);
            reaparecer.DarInvulnerabilidad();
        }

        // Devuelvo las llaves al inventario y saco del mapa las que ya se habian juntado
        List<DataItemInventario> llaves = new List<DataItemInventario>();
        foreach (DataItemInventario data in Resources.FindObjectsOfTypeAll<DataItemInventario>())
        {
            if (datos.llaves.Contains(data.name) && !llaves.Contains(data)) llaves.Add(data);
        }

        if (SistemaInventario.Instance != null)
        {
            foreach (DataItemInventario llave in llaves)
            {
                if (SistemaInventario.Instance.Get(llave) == null) SistemaInventario.Instance.Add(llave);
            }
        }

        foreach (RecogerRecompensa recompensa in FindObjectsByType<RecogerRecompensa>(FindObjectsSortMode.None))
        {
            if (recompensa.ItemData != null && llaves.Contains(recompensa.ItemData))
            {
                Destroy(recompensa.gameObject);
            }
        }

        MostrarMensaje("Partida " + (SistemaGuardado.RanuraActual + 1) + " cargada");
    }

    private ReaparecerJugador ObtenerReaparecer(GameObject jugador)
    {
        ReaparecerJugador reaparecer = jugador.GetComponent<ReaparecerJugador>();
        if (reaparecer == null) reaparecer = jugador.AddComponent<ReaparecerJugador>();
        return reaparecer;
    }

    void OnDestroy()
    {
        // Por si se cambia de escena estando en pausa
        Time.timeScale = 1f;
    }
}
