using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FireMolotovField : MonoBehaviourPun
{
    private float duration;
    private float radius;
    private float damageAmount;
    private int healAmount;

    public void Init(float duration, float radius, float damage, int heal)
    {
        this.duration = duration;
        this.radius = radius;
        this.damageAmount = damage;
        this.healAmount = heal;

        StartCoroutine(EffectRoutine());
    }

    IEnumerator EffectRoutine()
    {
        float timer = 0f;

        while (timer < duration)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (var col in hits)
            {
                if (!col.CompareTag("Player")) continue;

                var pc = col.GetComponent<PlayerController>();
                if (pc == null) continue;

                PhotonView targetPV = pc.photonView;

                if (targetPV.IsMine && PhotonNetwork.LocalPlayer == photonView.Owner)
                {
                    pc.AddHealth(healAmount);
                }

                else if (photonView.IsMine && !targetPV.IsMine)
                {
                    targetPV.RPC(
                        "DealDamage",
                        RpcTarget.All,
                        PhotonNetwork.NickName,              // killer
                        damageAmount,                       // damage
                        PhotonNetwork.LocalPlayer.ActorNumber // actorID
                    );
                }
            }

            timer += 1f;
            yield return new WaitForSeconds(1f);
        }

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
