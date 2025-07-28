using UnityEngine;

public class FireStats : MonoBehaviour
{
    [Header("Flash")]
    public float flashRadius = 6f;
    public float flashDelay = 2f;
    public float flashDuration = 3f;

    [Header("Buff")]
    public float buffDuration = 10f;
    public float buffSpeedMultiplier = 1.5f;

    [Header("Molotov")]
    public float molotovRadius = 5f;
    public float molotovDuration = 6f;
    public int molotovHealAmount = 15;
    public float molotovDamageAmount = 10f;

    [Header("Arrow")]
    public float arrowRadius = 4f;
    public float arrowDamage = 25f;
}
