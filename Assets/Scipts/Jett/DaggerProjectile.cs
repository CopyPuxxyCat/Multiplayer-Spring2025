using UnityEngine;
using Photon.Pun;

public class DaggerProjectile : MonoBehaviourPun
{
    [SerializeField] private float damage = 25;
    [SerializeField] private float lifeTime = 5f;

    private void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(DestroySelf), lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (photonView.IsMine)
        {
            GameObject hitObject = collision.collider.gameObject;

            if (hitObject.CompareTag("Player"))
            {
                PhotonView targetPV = hitObject.GetComponent<PhotonView>();
                if (targetPV != null && !targetPV.IsMine)
                {
                    targetPV.RPC(
                        "DealDamage",
                        RpcTarget.All,
                        PhotonNetwork.NickName,          
                        damage,                          
                        PhotonNetwork.LocalPlayer.ActorNumber 
                    );
                }
            }
        }

        DestroySelf();
    }

    private void DestroySelf()
    {
        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}

