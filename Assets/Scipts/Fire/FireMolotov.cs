using UnityEngine;
using Photon.Pun;
using System.Collections;

public class FireMolotov : MonoBehaviourPun
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
                if (col.CompareTag("Player"))
                {
                    var pc = col.GetComponent<PlayerController>();
                    if (pc.photonView.IsMine)
                    {
                        if (pc.CompareTag("Player"))
                            pc.AddHealth(healAmount);
                        else
                            pc.TakeDamage(photonView.Owner.NickName, damageAmount, photonView.ViewID);
                    }
                }
            }

            timer += 1f;
            yield return new WaitForSeconds(1f);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}
