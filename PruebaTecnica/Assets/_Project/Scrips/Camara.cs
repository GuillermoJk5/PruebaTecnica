using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    // ==========================================
    // INSPECTOR CONFIGURATIONS
    // ==========================================
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 25f;

    // ==========================================
    // INTERNAL STATES
    // ==========================================
    private float distance;
    private float pitch;
    private float yaw;

    // ==========================================
    // UNITY LIFECYCLE
    // ==========================================
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("¡Por favor, asigna el Target (Centro del cubo) en el Inspector!");
            return;
        }

        // ¡AQUÍ ESTÁ LA MAGIA! 
        // El script lee la posición EXACTA en la que tú hayas dejado la cámara a mano en el editor.
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Calcula automáticamente la distancia actual a la que la dejaste respecto al target
        distance = Vector3.Distance(transform.position, target.position);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    // ==========================================
    // METODS (Alphabetical)
    // ==========================================
    void HandleRotation()
    {
        // Clic derecho para girar y no interferir con los clics de las piezas
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed;
            pitch -= mouseY * rotationSpeed;

            // Evita que la cámara se ponga completamente del revés en vertical
            pitch = Mathf.Clamp(pitch, -85f, 85f);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void UpdateCameraPosition()
    {
        // Posicionamiento matemático puro basado en la distancia actual
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = target.position + offset;
        transform.LookAt(target.position);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}