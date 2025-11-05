using UnityEngine;
using UnityEngine.UI;

public class InventarioUI : MonoBehaviour
{
    [Header("Referencias de las images")]
    [SerializeField] private Image iconoLlave1;
    [SerializeField] private Image iconoLlave2;
    [SerializeField] private Image iconoLlave3;

    [Header("Referencias del objeto")]
    [SerializeField] private DataItemInventario dataLlave1;
    [SerializeField] private DataItemInventario dataLlave2;
    [SerializeField] private DataItemInventario dataLlave3;

    [Header("Colores")]
    public Color colorGris = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color colorNaranja = new Color(1f, 0.65f, 0f, 1f);

    void Start()
    {
        ActualizarVisual();
    }

    void Update()
    {
        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        if (SistemaInventario.Instance == null) return;


        if (SistemaInventario.Instance.Get(dataLlave1) != null)
        {
            iconoLlave1.color = colorNaranja;
        }
        else
        {
            iconoLlave1.color = colorGris;
        }

        if (SistemaInventario.Instance.Get(dataLlave2) != null)
        {
            iconoLlave2.color = colorNaranja;
        }
        else
        {
            iconoLlave2.color = colorGris;
        }

        if (SistemaInventario.Instance.Get(dataLlave3) != null)
        {
            iconoLlave3.color = colorNaranja;
        }
        else
        {
            iconoLlave3.color = colorGris;
        }
    }
}