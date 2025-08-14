using UnityEngine;
using Photon.Pun;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class EarthSmokeProjectile : MonoBehaviourPun
{
    [SerializeField] private LayerMask groundMask; // set trong Inspector: Ground
    private string smokeAreaPath; // "Earth/<PrefabName>"
    private float smokeDuration;
    private Rigidbody rb;
    private bool hasCollided = false;
    private bool armed = false; // chỉ rơi khi đã được "thả"

    public void Init(string smokeAreaResourcePathWithFolder, float duration, LayerMask ground)
    {
        smokeAreaPath = smokeAreaResourcePathWithFolder; // ví dụ "Earth/EarthSmoke"
        smokeDuration = duration;
        groundMask = ground;

        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;         // treo sẵn trên trời
        rb.velocity = Vector3.zero;
        armed = false;
    }

    // Gọi khi người chơi bấm chuột phải để "thả" tất cả
    public void ArmAndDrop(float fallSpeed = 0f)
    {
        armed = true;
        rb.useGravity = true;
        if (fallSpeed > 0f) rb.velocity = Vector3.down * fallSpeed;
    }

    void OnCollisionEnter(Collision col)
    {
        if (!armed || hasCollided) return; // chỉ nổ khi đã "thả"
        // Chỉ nổ nếu va chạm Ground
        if ((groundMask.value & (1 << col.gameObject.layer)) == 0) return;
        hasCollided = true;

        Vector3 pos = col.contacts.Length > 0 ? col.contacts[0].point : transform.position;

        if (!string.IsNullOrEmpty(smokeAreaPath))
        {
            GameObject smoke = PhotonNetwork.Instantiate(smokeAreaPath, pos, Quaternion.identity);
            smoke.GetComponent<EarthSmoke>().Init(smokeDuration);
        }

        if (photonView.IsMine)
            PhotonNetwork.Destroy(gameObject);
        else
            Destroy(gameObject);
    }   
}
