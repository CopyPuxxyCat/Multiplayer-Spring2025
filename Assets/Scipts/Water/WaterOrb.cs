using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
public class WaterOrb : MonoBehaviourPun
{
    private float slowFieldDuration;
    private GameObject slowFieldPrefab;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(float duration, GameObject slowPrefab)
    {
        slowFieldDuration = duration;
        slowFieldPrefab = slowPrefab;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        // Dội lại nếu đụng Building
        if (collision.collider.CompareTag("Building"))
        {
            Vector3 reflect = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
            rb.velocity = reflect;
            return;
        }

        // Tạo SlowField khi chạm đất
        if (collision.collider.CompareTag("Ground"))
        {
            PhotonNetwork.Instantiate("Water/SlowField", transform.position, Quaternion.identity)
                .GetComponent<SlowField>().Init(slowFieldDuration);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
