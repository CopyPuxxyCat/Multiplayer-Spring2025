// =========================
// GunData.cs
// =========================
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;
    public bool IsAutomatic;
    public float TimeBetweenShots;
    public float HeatPerShot;
    public float ShotDamage;
    public float ADSZoom;
}

