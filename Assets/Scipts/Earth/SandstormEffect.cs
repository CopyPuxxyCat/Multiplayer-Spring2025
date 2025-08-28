using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.AI;

public class SandstormEffect : MonoBehaviourPun
{
    public float growSpeed = 5f;
    public float maxRadius = 15f;
    public float duration = 10f;
    private SphereCollider triggerCol;
    private float lifeTimer = 0f;

    void Awake()
    {
        triggerCol = GetComponent<SphereCollider>();
        if (triggerCol == null)
        {
            triggerCol = gameObject.AddComponent<SphereCollider>();
            triggerCol.isTrigger = true;
        }
        triggerCol.radius = 0.5f;
    }

    void Start()
    {
        // optionally set params from EarthStats if available
        var s = FindObjectOfType<EarthStats>();
        if (s != null)
        {
            growSpeed = s.sandstormGrowSpeed;
            maxRadius = s.sandstormMaxRadius;
            duration = s.sandstormDuration;
        }
    }

    void Update()
    {
        if (triggerCol.radius < maxRadius)
        {
            triggerCol.radius += growSpeed * Time.deltaTime;
        }

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= duration && photonView.IsMine)
        {
            StartCoroutine(DisableSandStormOverlay(0f));
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // Apply slow via RPC on that player's owner
        // RPC name based on your PlayerController: "RPC_ApplySlow(float slowFactor, float duration, bool islow)"
        pc.photonView.RPC("RPC_ApplySlow", pc.photonView.Owner, statsDefaultSlowAmount(), duration, true);

        // For local player show overlay
        if (pc.photonView.IsMine)
        {
            UIController.instance.ShowSandstormOverlay(true);
            StartCoroutine(DisableSandStormOverlay(9.5f));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // trying to remove slow early by calling RPC_ApplySlow with islow=false may not restore speed
        // depending on your PlayerController implementation. In your code earlier, slow removal relied on coroutine.
        // We'll still call the RPC with islow=false to indicate leaving.
        pc.photonView.RPC("RPC_ApplySlow", pc.photonView.Owner, 0f, 0f, false);

        if (pc.photonView.IsMine)
        {
            UIController.instance.ShowSandstormOverlay(false);
            StartCoroutine(DisableSandStormOverlay(0.5f));
        }
    }

    IEnumerator DisableSandStormOverlay(float time)
    {
        yield return new WaitForSeconds(time);
        UIController.instance.ShowSandstormOverlay(false);
    }    

    // helper to fetch default slow amount from EarthStats if available
    private float statsDefaultSlowAmount()
    {
        var s = FindObjectOfType<EarthStats>();
        return s != null ? s.sandstormSlowAmount : 0.5f;
    }
}
