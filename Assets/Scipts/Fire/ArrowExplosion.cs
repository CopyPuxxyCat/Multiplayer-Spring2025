using UnityEngine;
using Photon.Pun;

public class ArrowExplosion : MonoBehaviour
{
    [SerializeField] float explosionRadius = 6f;

    public void Explode(Photon.Realtime.Player owner, float damage, int attackerViewId)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Player")) continue;

            PhotonView targetPV = col.GetComponent<PhotonView>();
            if (targetPV != null && targetPV.IsMine == false)
            {
                targetPV.RPC(
                    "DealDamage",
                    RpcTarget.All,
                    owner.NickName,
                    damage,
                    owner.ActorNumber
                );
            }
        }
    }
}
