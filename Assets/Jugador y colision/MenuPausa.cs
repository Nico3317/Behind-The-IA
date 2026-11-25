using UnityEngine;

// Menu de pausa: se abre y se cierra con Escape.
public class MenuPausa : MenuEnJuego
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || MenuMuerte.Activo) return;

        if (pantalla == null) Abrir();
        else Seguir();
    }

    public void Abrir()
    {
        if (pantalla != null) return;
        Frenar();
        ArmarPantalla("Pausa", new Color(0f, 0f, 0.05f, 0.8f));
        CrearBoton("BotonJugar", "Jugar", 0f, new Color(0.15f, 0.6f, 0.25f, 1f), Seguir);
        CrearBoton("BotonSalir", "Salir", -150f, new Color(0.35f, 0.35f, 0.4f, 1f), SalirAlMenu);
    }

    public void Seguir()
    {
        if (pantalla == null) return;
        Destroy(pantalla);
        pantalla = null;
        Reanudar();
    }

    void OnDestroy()
    {
        if (pantalla != null) Time.timeScale = 1f;
    }
}
