using UnityEngine;

public class Jugador2 : MonoBehaviour
{
    private CharacterController control;
    private Transform cam;
    private Vector3 direction;
    private bool grounded;
    private bool onWall;
    private Vector3 wallNormal;
    private float lockRotationTimer;

    public float speed = 7f;
    private float gravity = -7.8f;

    public float jump1 = 0.8f;
    public float jump2 = 1.0f;
    public float jump3 = 1.1f;
    public float jumpChainTime = 0.15f;
    private int jumpCount;
    private float jumpResetTimer;

    public float wallJumpForce = 6f;
    public float wallJumpUp = 5f;
    public float wallSlideSpeed = -1.4f;
    private float wallTimer;

    public Animator anim;

    void Awake()
    {
        control = GetComponent<CharacterController>();
        cam = Camera.main.transform;
    }

    void Update()
    {
        grounded = control.isGrounded;
        anim.SetBool("isgroun", grounded);
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if (grounded && direction.y < 0)
            direction.y = -2f;

        if (grounded)
        {
            if (jumpResetTimer > 0) jumpResetTimer -= Time.deltaTime;
            else if (jumpCount > 0) jumpCount = 0;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (onWall && !grounded)
                WallJump();
            else if (grounded)
                NormalJump();
        }

        if (onWall && !grounded && direction.y < 0)
        {
            direction.y = wallSlideSpeed;
            direction.x *= 0.4f;
            direction.z *= 0.4f;
        }
        else
        {
            direction.y += gravity * Time.deltaTime;
        }

        Vector3 f = cam.forward; f.y = 0; f.Normalize();
        Vector3 r = cam.right; r.y = 0; r.Normalize();
        Vector3 move = f * v + r * h;

        if (move.magnitude > 0.1f)
        {
            direction.x = move.x;
            direction.z = move.z;

            if (lockRotationTimer <= 0)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), Time.deltaTime * 14f);
        }
        else if (grounded)
        {
            direction.x = 0;
            direction.z = 0;
        }

        if (lockRotationTimer > 0) lockRotationTimer -= Time.deltaTime;
        if (wallTimer > 0) wallTimer -= Time.deltaTime;
        else if (onWall)
        {
            onWall = false;
            jumpCount = 0;
        }
        if (direction.x != 0f || direction.z != 0f)
        {
            anim.SetBool("run", true);
        }
        else
        {
            anim.SetBool("run", false);
        }
        control.Move(direction * speed * Time.deltaTime);
        Debug.Log(direction);
    }

    void NormalJump()
    {
        anim.SetTrigger("jump");
        jumpResetTimer = jumpChainTime;
        jumpCount++;

        if (jumpCount == 1)
            direction.y = Mathf.Sqrt(jump1 * -2f * gravity);
        else if (jumpCount == 2)
            direction.y = Mathf.Sqrt(jump2 * -2f * gravity);
        else
        {
            direction.y = Mathf.Sqrt(jump3 * -2f * gravity);
            jumpCount = 0;
        }
    }

    void WallJump()
    {
        direction.x = wallNormal.x * wallJumpForce;
        direction.z = wallNormal.z * wallJumpForce;
        direction.y = wallJumpUp;

        transform.rotation = Quaternion.LookRotation(wallNormal);
        lockRotationTimer = 0.12f;

        onWall = false;
        jumpCount = 0;
        jumpResetTimer = jumpChainTime;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!grounded && hit.gameObject.CompareTag("pared"))
        {
            onWall = true;
            wallNormal = hit.normal;
            wallTimer = 0.28f;
        }
    }
}
