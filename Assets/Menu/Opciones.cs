using UnityEngine;

// Opciones del juego guardadas en PlayerPrefs (se mantienen entre partidas)
public static class Opciones
{
    const string ClaveVolumen = "opciones_volumen";
    const string ClaveSensibilidad = "opciones_sensibilidad";
    const string ClaveCalidad = "opciones_calidad";
    const string ClavePantallaCompleta = "opciones_pantalla_completa";

    // 0 a 1
    public static float Volumen
    {
        get { return PlayerPrefs.GetFloat(ClaveVolumen, 0.8f); }
        set { PlayerPrefs.SetFloat(ClaveVolumen, Mathf.Clamp01(value)); }
    }

    // Multiplicador de la sensibilidad de la camara (0.2 a 2)
    public static float Sensibilidad
    {
        get { return PlayerPrefs.GetFloat(ClaveSensibilidad, 1f); }
        set { PlayerPrefs.SetFloat(ClaveSensibilidad, Mathf.Clamp(value, 0.2f, 2f)); }
    }

    public static int Calidad
    {
        get { return Mathf.Clamp(PlayerPrefs.GetInt(ClaveCalidad, QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1); }
        set { PlayerPrefs.SetInt(ClaveCalidad, Mathf.Clamp(value, 0, QualitySettings.names.Length - 1)); }
    }

    public static bool PantallaCompleta
    {
        get { return PlayerPrefs.GetInt(ClavePantallaCompleta, Screen.fullScreen ? 1 : 0) == 1; }
        set { PlayerPrefs.SetInt(ClavePantallaCompleta, value ? 1 : 0); }
    }

    public static string NombreCalidad()
    {
        string[] nombres = QualitySettings.names;
        if (nombres.Length == 0) return "-";
        return nombres[Calidad];
    }

    // Aplica todas las opciones guardadas
    public static void Aplicar()
    {
        AudioListener.volume = Volumen;

        if (QualitySettings.names.Length > 0 && QualitySettings.GetQualityLevel() != Calidad)
        {
            QualitySettings.SetQualityLevel(Calidad, true);
        }

        if (Screen.fullScreen != PantallaCompleta)
        {
            Screen.fullScreen = PantallaCompleta;
        }

        AplicarSensibilidad();
    }

    // La camara guarda su sensibilidad original y se le aplica el multiplicador
    private static Vector2 sensibilidadMouseBase = new Vector2(-1, -1);
    private static Vector2 sensibilidadJoystickBase;

    public static void AplicarSensibilidad()
    {
        Camara camara = Object.FindFirstObjectByType<Camara>();
        if (camara == null) return;

        if (sensibilidadMouseBase.x < 0)
        {
            sensibilidadMouseBase = camara.mouseSensitivity;
            sensibilidadJoystickBase = camara.joystickSensitivity;
        }

        camara.mouseSensitivity = sensibilidadMouseBase * Sensibilidad;
        camara.joystickSensitivity = sensibilidadJoystickBase * Sensibilidad;
    }

    // Se llama al cambiar de escena: la camara nueva tiene su propia sensibilidad base
    public static void OlvidarCamara()
    {
        sensibilidadMouseBase = new Vector2(-1, -1);
    }

    public static void GuardarEnDisco()
    {
        PlayerPrefs.Save();
    }
}
