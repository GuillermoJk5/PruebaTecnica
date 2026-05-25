using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Distance")]
    [SerializeField] private float distance = 10f;

    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 20f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;

        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    // =========================
    // ROTATION (CLICK / TOUCH)
    // =========================

    void HandleRotation()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            yaw += mouseX * rotationSpeed;
            pitch -= mouseY * rotationSpeed;
        }
    }

    // =========================
    // ZOOM
    // =========================

    void HandleZoom()
    {
        // Mouse wheel (PC)
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
        }

        // Clamp zoom
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }

    // =========================
    // CAMERA POSITION
    // =========================

    void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);

        transform.position = target.position + offset;

        transform.LookAt(target.position);
    }

    // =========================
    // SET TARGET EXTERNALLY
    // =========================

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}