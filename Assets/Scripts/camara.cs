using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    [Tooltip("El transform del jugador que la c�mara debe seguir.")]
    public Transform target;

    [Header("Configuraci�n de Distancia")]
    [Tooltip("La distancia ideal que la c�mara mantendr� del objetivo.")]
    public float distance = 5.0f;
    [Tooltip("Qu� tan r�pido la c�mara se ajusta a las colisiones.")]
    public float collisionSmoothSpeed = 10f;
    [Tooltip("Un peque�o margen para que la c�mara no se pegue exactamente a la pared.")]
    public float wallOffset = 0.3f;

    [Header("Configuraci�n de Movimiento")]
    [Tooltip("Sensibilidad del movimiento del mouse (horizontal y vertical).")]
    public Vector2 mouseSensitivity = new Vector2(2.5f, 2.5f);
    [Tooltip("Qu� tan suavemente la c�mara sigue la posici�n del jugador.")]
    public float positionSmoothTime = 0.15f;

    [Header("L�mites de Rotaci�n Vertical")]
    [Tooltip("El �ngulo m�nimo (mirando hacia abajo) y m�ximo (mirando hacia arriba).")]
    public Vector2 pitchMinMax = new Vector2(-20, 80);

    [Header("Colisiones")]
    [Tooltip("Las capas que la c�mara considerar� como obst�culos.")]
    public LayerMask obstacleLayers;

    // Variables privadas
    private float yaw; // Rotaci�n horizontal
    private float pitch; // Rotaci�n vertical
    private Vector3 _currentVelocity = Vector3.zero; // Para el SmoothDamp de posici�n

    void Start()
    {
        // Opcional: Bloquear y ocultar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("La c�mara no tiene un objetivo (Target) asignado.");
            return;
        }

        // 1. OBTENER INPUT DEL MOUSE PARA LA ROTACI�N
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity.x;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity.y;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y); // Limitar la rotaci�n vertical

        // 2. CALCULAR LA ROTACI�N Y POSICI�N DESEADA DE LA C�MARA
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPositionOffset = desiredRotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + desiredPositionOffset;

        // 3. MANEJAR COLISIONES
        float actualDistance = distance;
        RaycastHit hit;
        // Lanzamos un rayo desde el jugador hacia la posici�n deseada de la c�mara
        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleLayers))
        {
            // Si el rayo choca, la nueva distancia es la distancia al punto de choque
            // Le restamos un peque�o offset para que no se meta en la pared
            actualDistance = Vector3.Distance(target.position, hit.point) - wallOffset;
        }

        // 4. CALCULAR LA POSICI�N FINAL
        // Recalculamos la posici�n final usando la distancia real (ajustada por colisi�n)
        Vector3 finalPosition = target.position + (desiredRotation * new Vector3(0, 0, -actualDistance));

        // 5. APLICAR MOVIMIENTO SUAVE Y POSICIONAR LA C�MARA
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _currentVelocity, positionSmoothTime);

        // 6. HACER QUE LA C�MARA MIRE SIEMPRE AL JUGADOR
        transform.LookAt(target.position);
    }
}