using UnityEngine;
public class Jugador : MonoBehaviour
{
    private CharacterController control;
    private Vector3 direction;
    private bool grunding;
    private Vector3 moveDirection;
    Quaternion targetRotation;

    [Header("Fisicas")]
    private float gravity = -7.8f;
    private float speed = 5f;
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
    }

    void Update()
    {
        this.grunding = this.control.isGrounded;
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Salto();

        this.moveDirection = new Vector3(horizontalInput, 0, verticalInput);

        if (this.moveDirection.magnitude >= 0.1f)
        {
            rotacion();
        }

        this.direction.x = horizontalInput;
        this.direction.z = verticalInput;
        this.control.Move(this.direction * this.speed * Time.deltaTime);

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
    void rotacion()
    {
        this.targetRotation = Quaternion.LookRotation(moveDirection);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 10f);
    }
}