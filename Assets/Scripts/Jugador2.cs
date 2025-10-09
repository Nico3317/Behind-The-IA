using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Jugador2 : MonoBehaviour
{
    private CharacterController control;
    private Vector3 direction; // Vector principal que contiene X, Y (salto/gravedad), Z
    private bool grunding;
    private Transform cameraTransform;

    [Header("Fisicas")]
    private float gravity = -7.8f;
    public float speed = 5f;
    public float jumpforce = .80f;
    public float jumpforce2 = 1f;
    public float jumpforce3 = 1.1f;
    public int jumpcount = 0;

    [Header("Encadenamiento salto")]
    public float jumptime = 0.1f;
    private float jumpResetTimer;

    void Awake()
    {
        this.control = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1. OBTENER ESTADO E INPUT
        this.grunding = this.control.isGrounded;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 2. MANEJAR SALTO Y GRAVEDAD
        // Esta función modifica 'this.direction.y' con la física de salto y gravedad.
        Salto();

        // 3. CALCULAR DIRECCIÓN HORIZONTAL BASADA EN LA CÁMARA
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 'moveDirection' es solo para el cálculo horizontal y la rotación.
        Vector3 moveDirection = (camForward * verticalInput) + (camRight * horizontalInput);

        // 4. MANEJAR LA ROTACIÓN
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // 5. APLICAR MOVIMIENTO (LÓGICA RESTAURADA DE JUGADOR.CS)
        // Asignamos el movimiento horizontal calculado (relativo a la cámara)
        // a nuestro vector 'direction', que ya contiene la velocidad vertical del Salto().
        this.direction.x = moveDirection.x;
        this.direction.z = moveDirection.z;

        // Movemos el personaje usando el vector 'direction' unificado,
        // multiplicado por la velocidad, tal como en el script original.
        this.control.Move(this.direction * speed * Time.deltaTime);

        Debug.Log(this.jumpcount);
    }

    void Salto()
    {
        if (this.grunding)
        {
            if (jumpcount > 0)
            {
                jumpResetTimer -= Time.deltaTime;
                if (jumpResetTimer <= 0f)
                {
                    jumpcount = 0;
                }
            }

            if (this.direction.y < 0)
            {
                this.direction.y = -1f;
            }

            if (Input.GetButtonDown("Jump"))
            {
                jumpResetTimer = jumptime;

                if (this.jumpcount == 2)
                {
                    this.direction.y = Mathf.Sqrt(this.jumpforce3 * -2.0f * this.gravity);
                    this.jumpcount = 0;
                }
                else if (this.jumpcount == 1)
                {
                    this.direction.y = Mathf.Sqrt(this.jumpforce2 * -2.0f * this.gravity);
                    this.jumpcount++;
                }
                else
                {
                    this.direction.y = Mathf.Sqrt(this.jumpforce * -2.0f * this.gravity);
                    this.jumpcount++;
                }
            }
        }
        else
        {
            this.direction.y += this.gravity * Time.deltaTime;
        }
    }
}