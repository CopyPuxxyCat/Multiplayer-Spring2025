// =========================
// ArmorButton.cs (for small/big armor)
// =========================
using UnityEngine;
using UnityEngine.UI;

public class ArmorButton : MonoBehaviour
{
    public enum ArmorType { Small, Big }
    public ArmorType armorType;
    public Image buttonImage;
    private bool isSelected = false;

    public void Toggle()
    {
        isSelected = !isSelected;
        WeaponUpgradeManager.Instance.ToggleArmorPending(armorType);
        UpdateUI();
    }

    public void UpdateUI()
    {
        buttonImage.color = isSelected ? Color.green : Color.white;
    }
}
