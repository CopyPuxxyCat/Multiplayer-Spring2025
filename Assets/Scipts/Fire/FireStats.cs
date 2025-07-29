using UnityEngine;

public class FireStats : MonoBehaviour
{
    [Header("Flash")]
    public float flashRadius = 8f;
    public float flashDuration = 2f;

    [Header("Molotov")]
    public float molotovDuration = 5f;
    public float molotovRadius = 4f;
    public float molotovDamageAmount = 10f;
    public int molotovHealAmount = 5;

    [Header("Buff")]
    public float buffDuration = 10f;
    public float buffSpeedMultiplier = 1.5f;

    [Header("Arrow")]
    public float arrowRadius = 5f;
    public float arrowDamage = 180f;

    [Header("Common")]
    public float throwSpeed = 15f;

}
