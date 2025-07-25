// =========================
// 2️⃣ Gun.cs (attached to each gun in Player prefab)
// =========================
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GunData gunData;
    public GameObject MuzzleFlash;
    public AudioSource ShotSound;

    // Runtime attributes
    public float currentDamage;
    public float currentFireRate;
    public float currentHeat;
    public float currentZoom;
    public bool currentIsAutomatic;

    void Awake()
    {
        if (gunData != null)
        {
            currentIsAutomatic = gunData.IsAutomatic;
            currentDamage = gunData.ShotDamage;
            currentFireRate = gunData.TimeBetweenShots;
            currentHeat = gunData.HeatPerShot;
            currentZoom = gunData.ADSZoom;
        }
    }

    public void ApplyUpgrades(int dmg, int fireRate, int heat)
    {
        currentDamage = gunData.ShotDamage + dmg * 5f;
        currentFireRate = Mathf.Max(0.05f, gunData.TimeBetweenShots - fireRate * 0.01f);
        currentHeat = Mathf.Max(0, gunData.HeatPerShot - heat * 0.2f);
    }
}