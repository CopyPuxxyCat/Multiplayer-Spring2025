using UnityEngine;

public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager instance;
    [Header("Config")]
    public int[] upgradeCosts = { 400, 600, 800, 1000, 1200, 1500 };
    public int maxUpgradeLevel = 6;
    public int armorCost = 500;

    [Header("References")]
    public Gun currentGun;
    public CurrencyManager currencyManager; 

    // Các cấp hiện tại
    private int damageLevel = 0;
    private int fireRateLevel = 0;
    private int heatLevel = 0;

    private bool armorPurchased = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool UpgradeDamage()
    {
        if (damageLevel >= maxUpgradeLevel) return false;
        int cost = upgradeCosts[damageLevel];
        if (!currencyManager.TrySpend(cost)) return false;

        damageLevel++;
        ApplyUpgradesToGun();
        return true;
    }

    public bool UpgradeFireRate()
    {
        if (fireRateLevel >= maxUpgradeLevel) return false;
        int cost = upgradeCosts[fireRateLevel];
        if (!currencyManager.TrySpend(cost)) return false;

        fireRateLevel++;
        ApplyUpgradesToGun();
        return true;
    }

    public bool UpgradeHeatReduction()
    {
        if (heatLevel >= maxUpgradeLevel) return false;
        int cost = upgradeCosts[heatLevel];
        if (!currencyManager.TrySpend(cost)) return false;

        heatLevel++;
        ApplyUpgradesToGun();
        return true;
    }

    public bool PurchaseArmor()
    {
        if (armorPurchased) return false;
        if (!currencyManager.TrySpend(armorCost)) return false;

        armorPurchased = true;
        // TODO: Gọi hàm nào đó để thêm giáp cho player
        return true;
    }

    public void SetCurrentGun(Gun gun)
    {
        currentGun = gun;
        ApplyUpgradesToGun(); // Gán lại súng sẽ re-apply nâng cấp
    }

    private void ApplyUpgradesToGun()
    {
        Debug.Log("level của súng: " + damageLevel + "-" + fireRateLevel + "-" + heatLevel);
        if (currentGun != null)
        {
            currentGun.ApplyUpgrades(damageLevel, fireRateLevel, heatLevel);
        }
    }

    public void ResetUpgrades()
    {
        damageLevel = 0;
        fireRateLevel = 0;
        heatLevel = 0;
        armorPurchased = false;

        if (currentGun != null)
        {
            currentGun.ResetToBaseStats();
        }
    }
}

