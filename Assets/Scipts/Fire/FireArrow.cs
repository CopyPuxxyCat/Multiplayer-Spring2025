using UnityEngine;
using Photon.Pun;

public class FireArrow : MonoBehaviourPun
{
    private float radius;
    private float damage;

    public void Init(float radius, float damage)
    {
        this.radius = radius;
        this.damage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Player"))
            {
                var pc = col.GetComponent<PlayerController>();
                if (pc != null)
                    pc.TakeDamage(photonView.Owner.NickName, damage, photonView.ViewID);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
