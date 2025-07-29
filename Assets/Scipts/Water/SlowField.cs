using UnityEngine;
using Photon.Pun;

public class SlowField : MonoBehaviourPun
{
    [Header("Slow Settings")]
    public float slowAmount = 0.5f;   
    public float radius = 4f;         
    public float duration = 5f;       
    private bool Slow = true;

    private float timer;

    public void Init(float dur)
    {
        duration = dur;
        timer = 0f;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        timer += Time.deltaTime;
        if (timer > duration)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var pc = hit.GetComponent<PhotonView>();
                if (pc != null)
                {
                    pc.RPC("RPC_ApplySlow", pc.Owner, slowAmount, 5.5f, Slow);
                }
            }
        }
    }
}
