using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class TidalWave : MonoBehaviourPun
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float lifeTime = 6f;

    [Header("Ground Detection")]
    [SerializeField] float raycastHeight = 2f;
    [SerializeField] float raycastDistance = 5f;
    [SerializeField] float groundOffset = 0.1f;
    [SerializeField] LayerMask groundLayer;

    [Header("Effect")]
    [SerializeField] float stunDuration = 2f;

    private Vector3 moveDirection;
    private float timer;
    private Rigidbody rb;
    private HashSet<GameObject> stunnedTargets = new HashSet<GameObject>();

    public void Init(Vector3 direction, float speed, float stunTime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        stunDuration = stunTime;
        timer = lifeTime;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        MoveAndStickToGround();
    }

    private void MoveAndStickToGround()
    {
        // Vị trí tiếp theo theo hướng di chuyển
        Vector3 nextPos = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
        Vector3 rayOrigin = nextPos + Vector3.up * raycastHeight;

        // Raycast xuống từ điểm đó để tìm mặt đất
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance + raycastHeight, groundLayer))
        {
            Vector3 groundPos = new Vector3(nextPos.x, hit.point.y + groundOffset, nextPos.z);
            rb.MovePosition(groundPos);

            // Xoay theo mặt phẳng nghiêng
            Vector3 slopeDir = Vector3.ProjectOnPlane(moveDirection, hit.normal).normalized;
            Quaternion slopeRot = Quaternion.LookRotation(slopeDir, hit.normal);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, slopeRot, 10f * Time.fixedDeltaTime));
        }
        else
        {
            // Fallback nếu không raycast trúng
            rb.MovePosition(nextPos);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        // Bỏ qua decoration
        if (other.CompareTag("Decoration")) return;

        // Gặp player → stun
        if (other.CompareTag("Player") && !stunnedTargets.Contains(other.gameObject))
        {
            stunnedTargets.Add(other.gameObject);

            PhotonView targetPV = other.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                targetPV.RPC("ApplyStun", targetPV.Owner, stunDuration);
            }
        }
    }

    // Gọi từ collider con (ví dụ HitWall trigger)
    public void HitWallDetected()
    {
        if (!photonView.IsMine) return;
        PhotonNetwork.Destroy(gameObject);
    }
}
