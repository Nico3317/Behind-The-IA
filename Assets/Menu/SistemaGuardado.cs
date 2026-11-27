using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Datos que se guardan de cada partida (se escriben como JSON)
[Serializable]
public class DatosPartida
{
    public string escena = "MapaNuevo";
    public int semillaCiudad;
    public float posX, posY, posZ;
    public float rotacionY;
    public List<string> llaves = new List<string>();
    public float tiempoJugado;
    public string fecha;
    public long fechaTicks;
}

// Guarda y carga partidas en 3 ranuras (archivos JSON en la carpeta de datos del juego)
public static class SistemaGuardado
{
    public const int CantidadRanuras = 3;
    public const string EscenaJuego = "MapaNuevo";
    public const string EscenaMenu = "MenuPrincipal";

    // Partida que se va a aplicar cuando cargue la escena del juego (null = partida nueva)
    public static DatosPartida PartidaPendiente { get; private set; }

    // Semilla de la ciudad de la partida actual (asi se puede regenerar la misma ciudad al cargar)
    public static int SemillaActual { get; private set; }
    private static bool haySemilla;

    // Ranura en la que se guardo/cargo por ultima vez (-1 = ninguna)
    public static int RanuraActual { get; set; } = -1;

    static string RutaRanura(int ranura)
    {
        return Path.Combine(Application.persistentDataPath, "partida_" + (ranura + 1) + ".json");
    }

    public static bool ExisteRanura(int ranura)
    {
        return File.Exists(RutaRanura(ranura));
    }

    public static bool HayAlgunaPartida()
    {
        for (int i = 0; i < CantidadRanuras; i++)
        {
            if (ExisteRanura(i)) return true;
        }
        return false;
    }

    public static DatosPartida LeerRanura(int ranura)
    {
        if (!ExisteRanura(ranura)) return null;
        try
        {
            return JsonUtility.FromJson<DatosPartida>(File.ReadAllText(RutaRanura(ranura)));
        }
        catch (Exception e)
        {
            Debug.LogWarning("No se pudo leer la partida " + (ranura + 1) + ": " + e.Message);
            return null;
        }
    }

    public static int RanuraMasReciente()
    {
        int mejor = -1;
        long mejorTicks = long.MinValue;
        for (int i = 0; i < CantidadRanuras; i++)
        {
            DatosPartida datos = LeerRanura(i);
            if (datos != null && datos.fechaTicks > mejorTicks)
            {
                mejorTicks = datos.fechaTicks;
                mejor = i;
            }
        }
        return mejor;
    }

    public static bool Guardar(int ranura, DatosPartida datos)
    {
        try
        {
            datos.semillaCiudad = SemillaActual;
            datos.fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            datos.fechaTicks = DateTime.Now.Ticks;
            File.WriteAllText(RutaRanura(ranura), JsonUtility.ToJson(datos, true));
            RanuraActual = ranura;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("No se pudo guardar la partida: " + e.Message);
            return false;
        }
    }

    public static void BorrarRanura(int ranura)
    {
        if (ExisteRanura(ranura)) File.Delete(RutaRanura(ranura));
    }

    public static void NuevaPartida()
    {
        PartidaPendiente = null;
        RanuraActual = -1;
        SemillaActual = Environment.TickCount;
        haySemilla = true;
    }

    public static bool PrepararCarga(int ranura)
    {
        DatosPartida datos = LeerRanura(ranura);
        if (datos == null) return false;

        PartidaPendiente = datos;
        RanuraActual = ranura;
        SemillaActual = datos.semillaCiudad;
        haySemilla = true;
        return true;
    }

    // La llama el generador de la ciudad: devuelve siempre la misma semilla para la partida actual
    public static int ObtenerSemillaDeLaPartida()
    {
        if (!haySemilla)
        {
            // Se dio Play directo en la escena del juego, sin pasar por el menu
            SemillaActual = Environment.TickCount;
            haySemilla = true;
        }
        return SemillaActual;
    }

    // Escena que hay que abrir para la partida preparada con PrepararCarga
    public static string EscenaACargar()
    {
        if (PartidaPendiente != null && !string.IsNullOrEmpty(PartidaPendiente.escena)
            && Application.CanStreamedLevelBeLoaded(PartidaPendiente.escena))
        {
            return PartidaPendiente.escena;
        }
        return EscenaJuego;
    }

    // La llama el menu de pausa despues de aplicar la partida cargada
    public static DatosPartida TomarPartidaPendiente()
    {
        DatosPartida datos = PartidaPendiente;
        PartidaPendiente = null;
        return datos;
    }

    public static string FormatearTiempo(float segundos)
    {
        int total = Mathf.FloorToInt(segundos);
        int horas = total / 3600;
        int minutos = (total % 3600) / 60;
        int segs = total % 60;
        if (horas > 0) return horas + ":" + minutos.ToString("00") + ":" + segs.ToString("00");
        return minutos + ":" + segs.ToString("00");
    }
}
