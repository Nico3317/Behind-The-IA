using UnityEngine;

public class Puerta : MonoBehaviour
{
    [Header("Referencias de Llaves")]

    [SerializeField] private DataItemInventario llave1Data;
    [SerializeField] private DataItemInventario llave2Data;
    [SerializeField] private DataItemInventario llave3Data;

    [Header("Configuración de la Puerta")]
    [Tooltip("La distancia que se moverá la puerta al abrirse")]
    public float distanciaMovimiento = 3f;
    public float velocidadApertura = 1f;

    private bool estaAbierta = false;
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (estaAbierta)
        {
            AbrirPuerta();
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !estaAbierta)
        {
            if (SistemaInventario.Instance == null)
            {
                Debug.LogError("SistemaInventario no está listo.");
                return;
            }


            bool tieneLlave1 = SistemaInventario.Instance.Get(llave1Data) != null;
            bool tieneLlave2 = SistemaInventario.Instance.Get(llave2Data) != null;
            bool tieneLlave3 = SistemaInventario.Instance.Get(llave3Data) != null;

            if (tieneLlave1 && tieneLlave2 && tieneLlave3)
            {
                Debug.Log("¡Las 3 llaves encontradas! Abriendo la puerta.");
                estaAbierta = true;
            }
            else
            {

                Debug.Log("Faltan llaves para abrir esta puerta.");
            }
        }
    }

    private void AbrirPuerta()
    {
        Vector3 posicionFinal = posicionInicial + Vector3.up * distanciaMovimiento;

        transform.position = Vector3.Lerp(
            transform.position,
            posicionFinal,
            Time.deltaTime * velocidadApertura
        );

    }
}