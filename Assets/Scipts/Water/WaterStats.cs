
using UnityEngine;

public class WaterStats : MonoBehaviour
{
    [Header("Skill 1: Slowing Orb")]
    public float orbThrowForce = 15f;
    public float orbGravity = -9.8f;
    public float slowFieldDuration = 5f;

    [Header("Skill 2-3: Heal/Shield")]
    public int allyHealAmount = 100;
    public int selfHealAmount = 50;
    public int allyShieldAmount = 100;
    public int selfShieldAmount = 50;
    public float healEffectDuration = 3f;

    [Header("Ultimate: Tidal Wave")]
    public float waveSpeed = 20f;
    public float waveDuration = 3f;
    public float stunDuration = 2f;
}
