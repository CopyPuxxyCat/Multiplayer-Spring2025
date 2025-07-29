using UnityEngine;
using Photon.Pun;

public class FireMolotov : MonoBehaviourPun
{
    private float duration, radius, damageAmount;
    private int healAmount;
    private bool hasExploded;

    public void Init(float duration, float radius, float damage, int heal)
    {
        this.duration = duration;
        this.radius = radius;
        this.damageAmount = damage;
        this.healAmount = heal;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.IsMine || hasExploded) return;

        if (collision.collider.CompareTag("Ground") && collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasExploded = true;
            GameObject field = PhotonNetwork.Instantiate("Fire/FireMolotovField", transform.position, Quaternion.identity);
            field.GetComponent<FireMolotovField>().Init(duration, radius, damageAmount, healAmount);

            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // Nảy nếu chưa va vào Ground
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 bounce = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
                rb.velocity = bounce * 0.6f;
            }
        }
    }
}
