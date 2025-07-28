using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FlashBall : MonoBehaviourPun
{
    [SerializeField] float speed = 8f;
    [SerializeField] float delayToExplode = 1f;
    private Vector3 moveDir;

    private float radius;
    private float duration;

    public void Init(Vector3 dir, float radius, float duration)
    {
        moveDir = dir.normalized;
        this.radius = radius;
        this.duration = duration;

        StartCoroutine(ExplodeAfterDelay());
    }

    void Update()
    {
        transform.position += moveDir * speed * Time.deltaTime;
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(delayToExplode);
        Explode();
    }

    void Explode()
    {
        if (!photonView.IsMine) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                PhotonView pv = col.GetComponent<PhotonView>();
                if (pv != null)
                    pv.RPC("FlashScreen", pv.Owner, duration);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
