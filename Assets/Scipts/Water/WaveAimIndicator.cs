using UnityEngine;

public class WaveAimIndicator : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, groundLayer);

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Building")) continue;

            // Hợp lệ → Cập nhật vị trí + xoay
            transform.position = hit.point + Vector3.up * 0.1f;

            Vector3 forward = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized;
            transform.rotation = Quaternion.LookRotation(forward);
            return;
        }
    }
}
