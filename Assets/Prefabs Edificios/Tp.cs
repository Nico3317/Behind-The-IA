using UnityEngine;

public class Tp : MonoBehaviour
{
    public Transform Target;
    public GameObject ThePlayer;

    private CharacterController characterController;

    void Start()
    {
        // Los edificios se crean desde un prefab y no pueden tener al jugador asignado: lo busco por tag
        if (ThePlayer == null) ThePlayer = GameObject.FindGameObjectWithTag("Player");

        if (ThePlayer != null)
        {
            characterController = ThePlayer.GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("ThePlayer no tiene un CharacterController adjunto.");
            }
        }
        else
        {
            Debug.LogError("Asigna el GameObject del jugador a ThePlayer en el Inspector.");
        }

    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && characterController != null)
        {
            characterController.enabled = false;
            ThePlayer.transform.position = Target.transform.position;
            characterController.enabled = true;
        }
    }
}