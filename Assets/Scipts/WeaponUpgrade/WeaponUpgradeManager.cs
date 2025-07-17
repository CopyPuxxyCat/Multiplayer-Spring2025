using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager instance;

    [Header("Config")]
    public int[] upgradeCosts = { 400, 600, 800, 1000, 1200, 1500 };
    public int maxUpgradeLevel = 6;
    public int armorCost = 500;

    [Header("References")]
    public CurrencyManager currencyManager;
    public Gun currentGun;

    private Dictionary<int, Gun> indexToGun = new Dictionary<int, Gun>();
    private Dictionary<Gun, GunUpgradeData> gunUpgradeLevels = new Dictionary<Gun, GunUpgradeData>();

    private bool armorPurchased = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitUpgradeData(Gun[] guns)
    {
        indexToGun.Clear();
        gunUpgradeLevels.Clear();

        for (int i = 0; i < guns.Length; i++)
        {
            indexToGun[i] = guns[i];
            gunUpgradeLevels[guns[i]] = new GunUpgradeData();
        }
    }

    public void SetCurrentGun(Gun gun)
    {
        currentGun = gun;

        if (!gunUpgradeLevels.ContainsKey(gun))
        {
            gunUpgradeLevels[gun] = new GunUpgradeData();
        }

        ApplyUpgradesToGun();
    }

    public void UpgradeGunStat(int gunIndex, UpgradeButton.UpgradeType type)
    {
        if (!indexToGun.ContainsKey(gunIndex)) return;

        Gun targetGun = indexToGun[gunIndex];
        if (!gunUpgradeLevels.ContainsKey(targetGun)) return;

        GunUpgradeData data = gunUpgradeLevels[targetGun];
        int level = 0;
        int cost = 0;

        switch (type)
        {
            case UpgradeButton.UpgradeType.Damage:
                level = data.damageLevel;
                if (level >= maxUpgradeLevel) return;
                cost = upgradeCosts[level];
                if (!currencyManager.TrySpend(cost)) return;
                data.damageLevel++;
                break;

            case UpgradeButton.UpgradeType.FireRate:
                level = data.fireRateLevel;
                if (level >= maxUpgradeLevel) return;
                cost = upgradeCosts[level];
                if (!currencyManager.TrySpend(cost)) return;
                data.fireRateLevel++;
                break;

            case UpgradeButton.UpgradeType.Heat:
                level = data.heatLevel;
                if (level >= maxUpgradeLevel) return;
                cost = upgradeCosts[level];
                if (!currencyManager.TrySpend(cost)) return;
                data.heatLevel++;
                break;
        }

        if (targetGun == currentGun)
        {
            ApplyUpgradesToGun();
        }
    }

    public bool PurchaseArmor()
    {
        if (armorPurchased) return false;
        if (!currencyManager.TrySpend(armorCost)) return false;

        armorPurchased = true;
        // TODO: Cấp giáp cho player
        return true;
    }

    public void ResetUpgrades()
    {
        foreach (var upgrade in gunUpgradeLevels.Values)
        {
            upgrade.Reset();
        }

        armorPurchased = false;

        if (currentGun != null)
        {
            currentGun.ResetToBaseStats();
        }
    }

    private void ApplyUpgradesToGun()
    {
        if (currentGun == null || !gunUpgradeLevels.ContainsKey(currentGun)) return;

        GunUpgradeData upgrades = gunUpgradeLevels[currentGun];
        currentGun.ApplyUpgrades(upgrades.damageLevel, upgrades.fireRateLevel, upgrades.heatLevel);
    }

    [System.Serializable]
    public class GunUpgradeData
    {
        public int damageLevel = 0;
        public int fireRateLevel = 0;
        public int heatLevel = 0;

        public void Reset()
        {
            damageLevel = 0;
            fireRateLevel = 0;
            heatLevel = 0;
        }
    }
}


