using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Settings")]
    public bool IsAutomatic;
    public GameObject MuzzleFlash;
    public AudioSource ShotSound;

    [Header("Gun Stats")]
    public float TimeBetweenShots = 0.1f;
    public float HeatPerShot = 1f;
    public int ShotDamage = 25;
    public float ADSZoom = 50f;

    // Gốc để làm base khi tính nâng cấp
    private float baseTimeBetweenShots;
    private float baseHeatPerShot;
    private int baseShotDamage;

    void Awake()
    {
        // Lưu giá trị gốc để reset/scale nâng cấp
        baseTimeBetweenShots = TimeBetweenShots;
        baseHeatPerShot = HeatPerShot;
        baseShotDamage = ShotDamage;
    }

    /// <summary>
    /// Cập nhật stats dựa trên level nâng cấp
    /// </summary>
    public void ApplyUpgrades(int damageLevel, int fireRateLevel, int heatLevel)
    {
        // ShotDamage +5% mỗi level
        ShotDamage = Mathf.RoundToInt(baseShotDamage * (1f + 0.05f * damageLevel));

        // TimeBetweenShots -1% mỗi level (fire rate tăng)
        TimeBetweenShots = baseTimeBetweenShots * (1f - 0.01f * fireRateLevel);

        // HeatPerShot -5% mỗi level
        HeatPerShot = baseHeatPerShot * (1f - 0.05f * heatLevel);
    }

    /// <summary>
    /// Reset lại về thông số ban đầu 
    /// </summary>
    public void ResetToBaseStats()
    {
        ShotDamage = baseShotDamage;
        TimeBetweenShots = baseTimeBetweenShots;
        HeatPerShot = baseHeatPerShot;
    }
}

