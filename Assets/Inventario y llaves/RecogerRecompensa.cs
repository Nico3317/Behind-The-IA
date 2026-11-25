using UnityEngine;

public class RecogerRecompensa : MonoBehaviour
{
    public DataItemInventario ItemData;

    private bool jugadorCerca = false;

    // La llave tiene varios triggers: guardo cuando fue el ultimo contacto para no perderlo al salir de uno solo
    private float ultimoContacto = -1f;

    public bool JugadorCerca
    {
        get { return jugadorCerca; }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ultimoContacto = Time.time;
        }
    }

    void Update()
    {
        jugadorCerca = ultimoContacto >= 0f && Time.time - ultimoContacto < 0.15f;

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