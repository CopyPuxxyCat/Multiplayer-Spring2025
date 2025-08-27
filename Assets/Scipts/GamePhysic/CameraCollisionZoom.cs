using UnityEngine;

public class CameraCollisionZoom : MonoBehaviour
{
    [Header("References")]
    public Transform target; // Player / đối tượng camera theo dõi

    [Header("Camera Settings")]
    public float desiredDistance = 5f; // Khoảng cách mong muốn
    public float minDistance = 1f;     // Thu gần nhất
    public float smoothSpeed = 10f;    // Mượt khi zoom
    public LayerMask collisionMask;    // Layer nào tính là vật cản (Walls, Obstacles...)

    private float currentDistance;

    void Start()
    {
        currentDistance = desiredDistance;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Vị trí camera mong muốn
        Vector3 desiredPos = target.position - target.forward * desiredDistance;

        // Raycast từ target -> camera
        if (Physics.Raycast(target.position, (desiredPos - target.position).normalized, out RaycastHit hit, desiredDistance, collisionMask))
        {
            // Nếu va chạm, thu khoảng cách lại
            currentDistance = Mathf.Clamp(hit.distance - 0.2f, minDistance, desiredDistance);
        }
        else
        {
            // Không va chạm → dần dần về khoảng cách chuẩn
            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);
        }

        // Đặt camera
        Vector3 finalPos = target.position - target.forward * currentDistance;
        transform.position = finalPos;
        transform.LookAt(target.position);
    }
}
