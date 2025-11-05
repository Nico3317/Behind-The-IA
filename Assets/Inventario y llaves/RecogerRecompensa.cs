using UnityEngine;

public class RecogerRecompensa : MonoBehaviour
{
    public DataItemInventario ItemData;

    private bool jugadorCerca = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
        }
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            IntentarRecoger();
        }
    }

    private void IntentarRecoger()
    {
        if (SistemaInventario.Instance == null)
        {
            Debug.LogError("El SistemaInventario no está instanciado.");
            return;
        }

        if (SistemaInventario.Instance.Get(ItemData) != null)
        {
            return;
        }

        SistemaInventario.Instance.Add(ItemData);

        Destroy(gameObject);
    }
}