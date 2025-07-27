using UnityEngine;

public class HitWallCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            TidalWave wave = GetComponentInParent<TidalWave>();
            if (wave != null)
            {
                wave.HitWallDetected();
            }
        }
    }
}
