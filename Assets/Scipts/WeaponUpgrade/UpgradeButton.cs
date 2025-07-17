using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    public enum UpgradeType { Damage, FireRate, Heat }

    public UpgradeType upgradeType;
    public int gunIndex; // index trong AllGuns[]

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ApplyUpgrade);
    }

    void ApplyUpgrade()
    {
        WeaponUpgradeManager.instance.UpgradeGunStat(gunIndex, upgradeType);
    }
}

