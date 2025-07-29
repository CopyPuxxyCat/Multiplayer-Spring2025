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

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine) return;

        Debug.Log("va cham cai gi do");
        // Kích hoạt child xử lý damage
        Transform dmgArea = transform.Find("ExplosionArea");
        if (dmgArea != null)
        {
            dmgArea.gameObject.SetActive(true);
            dmgArea.GetComponent<ArrowExplosion>().Explode(photonView.Owner, damage, photonView.ViewID);
        }

        // Delay để đảm bảo RPC gửi
        Destroy(gameObject, 0.05f);
    }
}
