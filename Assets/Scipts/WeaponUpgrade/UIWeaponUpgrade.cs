// =========================
// UIWeaponUpgrade.cs (linked in Upgrade UI Panel)
// =========================
using UnityEngine;
using TMPro;

public class UIWeaponUpgrade : MonoBehaviour
{
    public TMP_Text coinText;
    public UpgradeButton[] upgradeButtons;
    public ArmorButton[] armorButtons;

    public void UpdateUI(int coin)
    {
        coinText.text = coin.ToString("000000");
        foreach (var btn in upgradeButtons) btn.UpdateUI();
        foreach (var btn in armorButtons) btn.UpdateUI();
    }
}

