using UnityEngine;
using UnityEngine.SceneManagement;

// Crea los menus automaticamente al cargar cada escena, sin tener que tocar las escenas:
// - En MenuPrincipal: crea el menu principal nuevo (y apaga el viejo).
// - En la escena del juego: crea el menu de pausa (Esc) con guardar y cargar partida.
public static class ArranqueMenus
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Iniciar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
    }

    // Respaldo para la primera escena (al dar Play en el editor puede no llegar el aviso de sceneLoaded)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RevisarEscenaInicial()
    {
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (modo != LoadSceneMode.Single) return;

        if (escena.name == SistemaGuardado.EscenaMenu)
        {
            if (Object.FindFirstObjectByType<MenuPrincipalUI>() == null)
            {
                new GameObject("Menu Principal").AddComponent<MenuPrincipalUI>();
            }
            return;
        }

        bool esEscenaDeJuego = Object.FindFirstObjectByType<GeneradorDeCuadra>() != null
                               || GameObject.FindGameObjectWithTag("Player") != null;

        if (esEscenaDeJuego && Object.FindFirstObjectByType<MenuPausa>() == null)
        {
            new GameObject("Menu Pausa").AddComponent<MenuPausa>();
        }
    }
}
