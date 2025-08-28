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

    /// <summary>
    /// Refresh toàn bộ UI sau khi load build
    /// </summary>
    public void RefreshLevels()
    {
        foreach (var btn in upgradeButtons)
        {
            btn.UpdateUI();
        }

        foreach (var armor in armorButtons)
        {
            armor.UpdateUI();
        }

        coinText.text = WeaponUpgradeManager.Instance.GetCurrentCoin().ToString("000000");
    }
}
