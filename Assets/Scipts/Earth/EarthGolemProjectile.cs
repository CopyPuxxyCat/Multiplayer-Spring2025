using UnityEngine;
using Photon.Pun;

public class EarthGolemProjectile : MonoBehaviourPun
{
    public float lifeTime = 5f;
    private int ownerActorNumber;
    private float damage;

    public void Init(int ownerActor, float dmg)
    {
        ownerActorNumber = ownerActor;
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        PlayerController player = collision.collider.GetComponent<PlayerController>();
        if (player != null)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.OwnerActorNr != ownerActorNumber)
            {
                pv.RPC("DealDamage", pv.Owner, "Golem", damage, ownerActorNumber);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
