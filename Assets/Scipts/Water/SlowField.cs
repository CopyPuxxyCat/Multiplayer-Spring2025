using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SlowField : MonoBehaviourPun
{
    public float slowAmount = 0.5f;
    public float radius = 4f;

    private float duration = 5f;
    private float startTime;

    private HashSet<PlayerController> slowedPlayers = new();

    public void Init(float dur)
    {
        duration = dur;
        startTime = Time.time;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (Time.time - startTime > duration)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                var pc = hit.GetComponent<PlayerController>();
                if (pc != null && !slowedPlayers.Contains(pc))
                {
                    pc.MoveSpeed *= slowAmount;
                    pc.RunSpeed *= slowAmount;
                    slowedPlayers.Add(pc);
                }
            }
        }
    }

    void OnDestroy()
    {
        // Reset speed for all affected players
        foreach (var pc in slowedPlayers)
        {
            if (pc != null)
            {
                pc.MoveSpeed /= slowAmount;
                pc.RunSpeed /= slowAmount;
            }
        }
        slowedPlayers.Clear();
    }
}
