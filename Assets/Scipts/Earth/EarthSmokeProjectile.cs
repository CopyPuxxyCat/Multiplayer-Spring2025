using UnityEngine;
using Photon.Pun;
using System.Collections;

public class EarthSmokeProjectile : MonoBehaviourPun
{
    private string smokeAreaResourcePath; // e.g. "Earth/EarthSmoke"
    private float smokeDuration;
    private Rigidbody rb;
    private bool hasCollided = false;

    public void Init(string smokeAreaResourcePathWithFolder, float duration)
    {
        smokeAreaResourcePath = smokeAreaResourcePathWithFolder;
        smokeDuration = duration;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        // initial downward velocity if desired
        rb.velocity = Vector3.down * 5f;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        // When hit something, sample position on navmesh near contact point to place smoke area cleanly
        Vector3 spawnPoint = collision.contacts[0].point;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPoint, out UnityEngine.AI.NavMeshHit navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            spawnPoint = navHit.position;

        // Instantiate smoke area on the network. smokeAreaResourcePath includes folder "Earth/..."
        if (!string.IsNullOrEmpty(smokeAreaResourcePath))
        {
            // extract resource name (Photon requires path inside Resources; we pass "Earth/Name")
            string path = smokeAreaResourcePath;
            // PhotonNetwork.Instantiate expects Resources path relative to Resources/, e.g. "Earth/EarthSmoke"
            GameObject smoke = PhotonNetwork.Instantiate(path, spawnPoint, Quaternion.identity);
            // schedule destruction of smoke area via coroutine on the owner who spawned it
            if (smoke != null)
            {
                // Let the smoke area handle its own lifetime (recommended).
                var area = smoke.GetComponent<MonoBehaviour>();
                // If smoke prefab doesn't destroy itself, we can destroy after duration:
                StartCoroutine(DestroyAfterSeconds(smoke, smokeDuration));
            }
        }

        // Destroy projectile after short delay to allow physics callbacks to finish
        StartCoroutine(DestroySelfNextFrame());
    }

    IEnumerator DestroyAfterSeconds(GameObject obj, float t)
    {
        yield return new WaitForSeconds(t);
        if (obj != null)
        {
            PhotonNetwork.Destroy(obj);
        }
    }

    IEnumerator DestroySelfNextFrame()
    {
        yield return null;
        if (photonView != null && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // if not network-owned (unlikely), just destroy locally
            Destroy(gameObject);
        }
    }
}
