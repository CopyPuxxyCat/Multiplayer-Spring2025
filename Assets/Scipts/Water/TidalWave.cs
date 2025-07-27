using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class TidalWave : MonoBehaviourPun
{
    private float moveSpeed = 10f;
    private float stunDuration = 2f;
    private Vector3 moveDirection;
    private HashSet<GameObject> stunnedTargets = new HashSet<GameObject>();

    [SerializeField] float lifeTime = 6f;
    [SerializeField] float groundOffset = 0.1f;
    [SerializeField] float raycastDistance = 5f;
    [SerializeField] LayerMask groundLayer;

    private float timer;

    public void Init(Vector3 direction, float speed, float stunTime)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        stunDuration = stunTime;

        timer = lifeTime;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        StickToGround();
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    void StickToGround()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 2f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayer))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                Vector3 targetPos = hit.point + Vector3.up * groundOffset;
                transform.position = new Vector3(transform.position.x, targetPos.y, transform.position.z);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;

        if (other.CompareTag("Player") && !stunnedTargets.Contains(other.gameObject))
        {
            stunnedTargets.Add(other.gameObject);
            PhotonView targetPV = other.GetComponent<PhotonView>();
            if (targetPV != null)
            {
                targetPV.RPC("StunPlayer", targetPV.Owner, stunDuration);
            }
        }
    }
}
