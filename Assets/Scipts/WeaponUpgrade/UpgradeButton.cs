// =========================
// UpgradeButton.cs (used for each stat: Damage/FireRate/Heat)
// =========================
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    public enum StatType { Damage, FireRate, Heat }

    public StatType stat;
    public int gunIndex;
    public TMP_Text levelText;
    public TMP_Text priceText;
    public Button plusButton, minusButton;

    public void UpdateUI()
    {
        var mgr = WeaponUpgradeManager.Instance;
        int lvl = mgr.GetUpgradeLevel(gunIndex, stat);
        int price = mgr.GetUpgradePrice(gunIndex, stat);
        levelText.text = $"LV.{lvl}";
        priceText.text = $"{price}";

        bool canUpgrade = mgr.CanAffordUpgrade(gunIndex, stat);
        plusButton.interactable = canUpgrade;
        minusButton.interactable = lvl > 0;
    }

    public void OnClickPlus() => WeaponUpgradeManager.Instance.UpgradeGunStat(gunIndex, stat);
    public void OnClickMinus() => WeaponUpgradeManager.Instance.DowngradeGunStat(gunIndex, stat);
}