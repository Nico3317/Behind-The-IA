using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Objetivo a Seguir")]
    [Tooltip("El transform del jugador que la cámara debe seguir.")]
    public Transform target;

    [Header("Configuración de Distancia")]
    [Tooltip("La distancia ideal que la cámara mantendrá del objetivo.")]
    public float distance = 5.0f;
    [Tooltip("Qué tan rápido la cámara se ajusta a las colisiones.")]
    public float collisionSmoothSpeed = 10f;
    [Tooltip("Un pequeño margen para que la cámara no se pegue exactamente a la pared.")]
    public float wallOffset = 0.3f;

    [Header("Configuración de Movimiento")]
    [Tooltip("Sensibilidad del movimiento del mouse (horizontal y vertical).")]
    public Vector2 mouseSensitivity = new Vector2(2.5f, 2.5f);
    [Tooltip("Qué tan suavemente la cámara sigue la posición del jugador.")]
    public float positionSmoothTime = 0.15f;

    [Header("Límites de Rotación Vertical")]
    [Tooltip("El ángulo mínimo (mirando hacia abajo) y máximo (mirando hacia arriba).")]
    public Vector2 pitchMinMax = new Vector2(-20, 80);

    [Header("Colisiones")]
    [Tooltip("Las capas que la cámara considerará como obstáculos.")]
    public LayerMask obstacleLayers;

    // Variables privadas
    private float yaw; // Rotación horizontal
    private float pitch; // Rotación vertical
    private Vector3 _currentVelocity = Vector3.zero; // Para el SmoothDamp de posición

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
            Debug.LogWarning("La cámara no tiene un objetivo (Target) asignado.");
            return;
        }

        // 1. OBTENER INPUT DEL MOUSE PARA LA ROTACIÓN
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity.x;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity.y;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y); // Limitar la rotación vertical

        // 2. CALCULAR LA ROTACIÓN Y POSICIÓN DESEADA DE LA CÁMARA
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPositionOffset = desiredRotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + desiredPositionOffset;

        // 3. MANEJAR COLISIONES
        float actualDistance = distance;
        RaycastHit hit;
        // Lanzamos un rayo desde el jugador hacia la posición deseada de la cámara
        if (Physics.Linecast(target.position, desiredPosition, out hit, obstacleLayers))
        {
            // Si el rayo choca, la nueva distancia es la distancia al punto de choque
            // Le restamos un pequeño offset para que no se meta en la pared
            actualDistance = Vector3.Distance(target.position, hit.point) - wallOffset;
        }

        // 4. CALCULAR LA POSICIÓN FINAL
        // Recalculamos la posición final usando la distancia real (ajustada por colisión)
        Vector3 finalPosition = target.position + (desiredRotation * new Vector3(0, 0, -actualDistance));

        // 5. APLICAR MOVIMIENTO SUAVE Y POSICIONAR LA CÁMARA
        transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _currentVelocity, positionSmoothTime);

        // 6. HACER QUE LA CÁMARA MIRE SIEMPRE AL JUGADOR
        transform.LookAt(target.position);
    }
}