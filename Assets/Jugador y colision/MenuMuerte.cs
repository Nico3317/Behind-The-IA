using UnityEngine;
using UnityEngine.SceneManagement;

// Menu que aparece cuando el jugador muere. Se crea solo, no hace falta ponerlo en la escena.
public class MenuMuerte : MenuEnJuego
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
        actual.Frenar();
        actual.ArmarPantalla("¡Te atraparon!", new Color(0.1f, 0f, 0f, 0.85f));
        actual.CrearBoton("BotonVolver", "Volver a jugar", 0f, new Color(0.8f, 0.15f, 0.15f, 1f), actual.VolverAJugar);
        actual.CrearBoton("BotonSalir", "Salir", -150f, new Color(0.35f, 0.35f, 0.4f, 1f), actual.SalirAlMenu);
    }

    public void VolverAJugar()
    {
        Time.timeScale = 1f;
        // Si se guardo o cargo una partida, se vuelve a ese punto; si no, se empieza de nuevo
        if (SistemaGuardado.RanuraActual >= 0 && SistemaGuardado.PrepararCarga(SistemaGuardado.RanuraActual))
        {
            SceneManager.LoadScene(SistemaGuardado.EscenaACargar());
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        // Por si se sale de la escena sin tocar el boton
        Time.timeScale = 1f;
        if (actual == this) actual = null;
    }
}
