using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class EarthGolem : MonoBehaviourPun
{
    [Header("Stats")]
    public float maxHP = 150f;
    public float fireRate = 1f;
    public float detectionRange = 15f;
    public float projectileSpeed = 20f;
    public float damagePerShot = 25f;

    [Header("Animation")]
    public Animator animator;
    public string sleepAnim = "Sleep";
    public string fireAnim = "Fire";
    public string deathAnim = "Death";

    [Header("Material Change")]
    public SkinnedMeshRenderer meshRenderer;
    public Material normalMat;
    public Material angryMat;

    [Header("References")]
    public Transform projectileSpawnPoint;
    public Transform headOrBody;
    public Transform eyePoint;

    [Header("Detection Settings")]
    public LayerMask visibleLayers; // Chọn Player + Ground trong Inspector

    private EarthStats stats;
    private float currentHP;
    private float lifeTimer;
    private float fireCooldown;
    private bool isSleeping = true;
    private bool isDead = false;
    private int ownerActorNumber;

    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform currentTarget;

    void Start()
    {
        stats = FindObjectOfType<EarthStats>();
        currentHP = maxHP;
        lifeTimer = stats != null ? stats.golemLifetime : 60f;
        ownerActorNumber = photonView.OwnerActorNr;

        if (animator != null) animator.Play(sleepAnim);
        if (meshRenderer != null && normalMat != null) meshRenderer.material = normalMat;
    }

    void Update()
    {
        if (isDead) return;

        if (photonView.IsMine)
        {
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                PhotonNetwork.Destroy(gameObject);
                return;
            }
        }

        if (!photonView.IsMine) return;

        fireCooldown -= Time.deltaTime;

        UpdateTarget();

        if (currentTarget != null)
        {
            Vector3 lookPos = currentTarget.position - transform.position;
            lookPos.y = 0;
            headOrBody.rotation = Quaternion.Slerp(headOrBody.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 5f);

            if (isSleeping)
            {
                isSleeping = false;
                photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All, fireAnim);
                photonView.RPC(nameof(RPC_SetMaterial), RpcTarget.All, true);
            }

            if (fireCooldown <= 0f)
            {
                fireCooldown = fireRate;
                photonView.RPC(nameof(RPC_GolemShoot), RpcTarget.All, currentTarget.position);
            }
        }
        else
        {
            if (!isSleeping)
            {
                isSleeping = true;
                photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All, sleepAnim);
                photonView.RPC(nameof(RPC_SetMaterial), RpcTarget.All, false);
            }
        }
    }

    void UpdateTarget()
    {
        enemiesInRange.Clear();

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        foreach (var hit in hits)
        {
            PlayerController pc = hit.GetComponent<PlayerController>();
            if (pc != null && pc.photonView.OwnerActorNr != ownerActorNumber)
            {
                enemiesInRange.Add(pc.transform);
            }
        }

        if (currentTarget != null && enemiesInRange.Contains(currentTarget) && CanSeeTarget(currentTarget))
        {
            return; // Giữ target hiện tại
        }

        currentTarget = null;
        foreach (var enemy in enemiesInRange)
        {
            if (CanSeeTarget(enemy))
            {
                currentTarget = enemy;
                break;
            }
        }
    }

    bool CanSeeTarget(Transform target)
    {
        Vector3 start = eyePoint.position;
        Vector3 end = target.position + Vector3.up * 1f;
        Vector3 dir = (end - start).normalized;
        float dist = Vector3.Distance(start, end);

        if (Physics.Raycast(start, dir, out RaycastHit hit, dist, visibleLayers))
        {
            if (hit.collider.GetComponent<PlayerController>() != null)
            {
                Debug.DrawLine(start, hit.point, Color.red);
                return true;
            }
            else
            {
                Debug.DrawLine(start, hit.point, Color.white);
            }
        }
        return false;
    }

    [PunRPC]
    void RPC_PlayAnim(string animName)
    {
        if (animator != null) animator.Play(animName);
    }

    [PunRPC]
    void RPC_SetMaterial(bool angry)
    {
        if (meshRenderer != null)
            meshRenderer.material = angry ? angryMat : normalMat;
    }

    [PunRPC]
    void RPC_GolemShoot(Vector3 targetPos)
    {
        if (stats == null) return;

        Vector3 spawn = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position + Vector3.up * 1.2f;
        GameObject proj = PhotonNetwork.Instantiate("Earth/" + stats.golemProjectileResourceName, spawn, Quaternion.identity);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (targetPos - spawn).normalized;
            rb.velocity = dir * projectileSpeed;
        }

        EarthGolemProjectile egp = proj.GetComponent<EarthGolemProjectile>();
        if (egp != null) egp.Init(ownerActorNumber, damagePerShot);
    }

    [PunRPC]
    public void GolemReceiveDamage(float dmg)
    {
        if (isDead) return;

        currentHP -= dmg;
        if (currentHP <= 0)
        {
            isDead = true;
            photonView.RPC(nameof(RPC_PlayAnim), RpcTarget.All, deathAnim);
            photonView.RPC(nameof(RPC_SetMaterial), RpcTarget.All, false);

            if (photonView.IsMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
