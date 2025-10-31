using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    [Tooltip("El transform del jugador que la c�mara debe seguir.")]
    public Transform target;

    [Header("Configuraci�n de Distancia")]
    [Tooltip("La distancia ideal que la c�mara mantendr� del objetivo.")]
    public float distance = 10f;
    [Tooltip("Qu� tan r�pido la c�mara se ajusta a las colisiones.")]
    public float collisionSmoothSpeed = 10f;
    [Tooltip("Un peque�o margen para que la c�mara no se pegue exactamente a la pared.")]
    public float wallOffset = 0.3f;

    [Header("Configuraci�n de Movimiento")]
    [Tooltip("Sensibilidad del movimiento del mouse (horizontal y vertical).")]
    public Vector2 mouseSensitivity = new Vector2(2.5f, 2.5f);
    [Tooltip("Sensibilidad del movimiento del stick derecho del joystick.")]
    public Vector2 joystickSensitivity = new Vector2(80f, 80f); 
    [Tooltip("Qu� tan suavemente la c�mara sigue la posici�n del jugador.")]
    public float positionSmoothTime = 0.15f;

    [Header("L�mites de Rotaci�n Vertical")]
    [Tooltip("El �ngulo m�nimo (mirando hacia abajo) y m�ximo (mirando hacia arriba).")]
    public Vector2 pitchMinMax = new Vector2(-20, 80);

    [Header("Colisiones")]
    [Tooltip("Las capas que la c�mara considerar� como obst�culos.")]
    public LayerMask obstacleLayers;

    private float yaw;
    private float pitch;
    private Vector3 _currentVelocity = Vector3.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        float horizontalRotation = (Input.GetAxis("Mouse X") * mouseSensitivity.x) +
                                   (Input.GetAxis("RightStickHorizontal") * joystickSensitivity.x * Time.deltaTime);

        float verticalRotation = (Input.GetAxis("Mouse Y") * mouseSensitivity.y) +
                                 (Input.GetAxis("RightStickVertical") * joystickSensitivity.y * Time.deltaTime);

        yaw += horizontalRotation;
        pitch -= verticalRotation; 
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPositionOffset = desiredRotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + desiredPositionOffset;

        float actualDistance = distance;
        RaycastHit hit;

        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleLayers))
        {
            actualDistance = Vector3.Distance(target.position, hit.point) - wallOffset;
        }

        Vector3 finalPosition = target.position + (desiredRotation * new Vector3(0, 0, -actualDistance));

        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _currentVelocity, positionSmoothTime);

        transform.LookAt(target.position);
    }
}