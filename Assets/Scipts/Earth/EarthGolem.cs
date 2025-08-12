using UnityEngine;
using Photon.Pun;
using System.Linq;

public class EarthGolem : MonoBehaviourPun
{
    private EarthStats stats;
    private float lifeTimer;
    private float fireCooldown = 0f;

    void Start()
    {
        stats = FindObjectOfType<EarthStats>();
        if (stats == null)
        {
            Debug.LogWarning("[EarthGolem] EarthStats not found in scene.");
        }
        lifeTimer = stats != null ? stats.golemLifetime : 60f;
    }

    void Update()
    {
        // Only owner runs AI to avoid duplicate firing
        if (!photonView.IsMine) return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        fireCooldown -= Time.deltaTime;
        if (fireCooldown <= 0f)
        {
            Transform target = FindClosestEnemy();
            if (target != null)
            {
                fireCooldown = stats != null ? stats.golemFireRate : 1f;
                SpawnProjectileTowards(target.position);
            }
        }
    }

    Transform FindClosestEnemy()
    {
        var players = FindObjectsOfType<PlayerController>()
            .Where(p => !p.photonView.IsMine) // enemies relative to owner
            .Select(p => p.transform);

        Transform closest = null;
        float best = stats != null ? stats.golemDetectionRange : 15f;

        foreach (var t in players)
        {
            float d = Vector3.Distance(transform.position, t.position);
            if (d <= best)
            {
                // simple line of sight check (consider obstacles)
                if (!Physics.Linecast(transform.position + Vector3.up * 0.5f, t.position + Vector3.up * 0.5f))
                {
                    closest = t;
                    best = d;
                }
            }
        }
        return closest;
    }

    void SpawnProjectileTowards(Vector3 targetPos)
    {
        if (stats == null) return;
        Vector3 spawn = transform.position + Vector3.up * 1.2f;
        GameObject proj = PhotonNetwork.Instantiate("Earth/" + stats.golemProjectileResourceName, spawn, Quaternion.identity);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (targetPos - spawn).normalized;
            rb.velocity = dir * stats.golemProjectileSpeed;
        }
    }
}
