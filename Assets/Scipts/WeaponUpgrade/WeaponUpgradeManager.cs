// =========================
// 7️⃣ WeaponUpgradeManager.cs (linked to GameObject in scene)
// =========================
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;

public class WeaponUpgradeManager : MonoBehaviour
{
    public static WeaponUpgradeManager Instance;

    public int[] upgradeCosts = { 400, 600, 800, 1000, 1000, 1000 };
    public int smallArmorCost = 500;
    public int bigArmorCost = 1000;

    public UIWeaponUpgrade UI;
    private CurrencyManager currencyManager;
    private PlayerController playerController;

    private Dictionary<int, Gun> indexToGun = new();
    private Dictionary<Gun, UpgradeData> currentUpgrades = new();
    private Dictionary<Gun, UpgradeData> tempUpgrades = new();

    private bool armorSmall, armorBig;
    private int coinInStore;

    void Awake() => Instance = this;

    private void Start()
    {
    }

    public bool HasValidRefs()
    {
        return currencyManager != null && playerController != null;
    }

    public void Init(Gun[] guns)
    {
        indexToGun.Clear();
        currentUpgrades.Clear();
        tempUpgrades.Clear();

        for (int i = 0; i < guns.Length; i++)
        {
            indexToGun[i] = guns[i];
            currentUpgrades[guns[i]] = new UpgradeData();
            tempUpgrades[guns[i]] = new UpgradeData();
        }
    }

    public void StartUpgradeSession()
    {

        if (currencyManager == null || playerController == null || UI == null)
        {
            Debug.LogWarning("[WeaponUpgradeManager] Missing references! Retrying...");
            FindLocalPlayerRefs();
            return;
        }

        if (currencyManager == null || playerController == null)
        {
            Debug.LogWarning("[WeaponUpgradeManager] Missing references! Retrying...");
            FindLocalPlayerRefs(); // Tìm lại nếu chưa có
            return; // hoặc bạn có thể delay và gọi lại sau
        }
        coinInStore = currencyManager.CurrentCoin;
        armorSmall = false;
        armorBig = false;
        foreach (var g in currentUpgrades)
            tempUpgrades[g.Key] = g.Value.Clone();

        UI.UpdateUI(coinInStore);
    }

    public void UpgradeGunStat(int index, UpgradeButton.StatType stat)
    {
        var gun = indexToGun[index];
        var data = tempUpgrades[gun];
        int lvl = data.GetLevel(stat);
        int cost = upgradeCosts[Mathf.Min(lvl, upgradeCosts.Length - 1)];

        if (coinInStore >= cost)
        {
            data.Increase(stat);
            coinInStore -= cost;
            UI.UpdateUI(coinInStore);
        }
    }

    public void DowngradeGunStat(int index, UpgradeButton.StatType stat)
    {
        var gun = indexToGun[index];
        var data = tempUpgrades[gun];
        int lvl = data.GetLevel(stat);
        if (lvl > 0)
        {
            int refund = upgradeCosts[lvl - 1];
            data.Decrease(stat);
            coinInStore += refund;
            UI.UpdateUI(coinInStore);
        }
    }

    public void ToggleArmorPending(ArmorButton.ArmorType type)
    {
        bool toggle = type == ArmorButton.ArmorType.Small ? (armorSmall = !armorSmall) : (armorBig = !armorBig);
        int cost = type == ArmorButton.ArmorType.Small ? smallArmorCost : bigArmorCost;

        if (toggle) coinInStore -= cost;
        else coinInStore += cost;

        UI.UpdateUI(coinInStore);
    }

    public void SubmitPurchase()
    {
        foreach (var pair in tempUpgrades)
        {
            currentUpgrades[pair.Key] = pair.Value.Clone();
            pair.Key.ApplyUpgrades(pair.Value.damage, pair.Value.fireRate, pair.Value.heat);
        }

        int totalArmor = 0;
        if (armorSmall) totalArmor += 100;
        if (armorBig) totalArmor += 200;
        playerController.AddArmor(totalArmor);

        currencyManager.SetCoin(coinInStore);
        UI.UpdateUI(coinInStore);
    }

    public void DiscardAll() => StartUpgradeSession();

    public void FindLocalPlayerRefs()
    {
        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            var view = player.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                currencyManager = player.GetComponent<CurrencyManager>();
                playerController = player.GetComponent<PlayerController>();
                /*Debug.Log($"[FindLocalPlayerRefs] Found player: {player.name}");
                Debug.Log($"CurrencyManager: {(currencyManager == null ? "null" : "OK")}");
                Debug.Log($"PlayerController: {(playerController == null ? "null" : "OK")}");*/
                break;
            }
        }

        if (currencyManager != null)
        {
            currencyManager.OnMoneyChanged += UpdateCoinDisplayInStore;
            UpdateCoinDisplayInStore(currencyManager.CurrentCoin); // cập nhật lần đầu
        }
    }

    private void UpdateCoinDisplayInStore(int currentCoin)
    {
        UI.UpdateUI(currentCoin);
    }

    public int GetUpgradeLevel(int idx, UpgradeButton.StatType type) => tempUpgrades[indexToGun[idx]].GetLevel(type);
    public int GetUpgradePrice(int idx, UpgradeButton.StatType type)
    {
        int lvl = GetUpgradeLevel(idx, type);
        return upgradeCosts[Mathf.Min(lvl, upgradeCosts.Length - 1)];
    }
    public bool CanAffordUpgrade(int idx, UpgradeButton.StatType type) => coinInStore >= GetUpgradePrice(idx, type);

    private class UpgradeData
    {
        public int damage, fireRate, heat;

        public int GetLevel(UpgradeButton.StatType stat) =>
            stat switch
            {
                UpgradeButton.StatType.Damage => damage,
                UpgradeButton.StatType.FireRate => fireRate,
                UpgradeButton.StatType.Heat => heat,
                _ => 0
            };

        public void Increase(UpgradeButton.StatType stat)
        {
            switch (stat)
            {
                case UpgradeButton.StatType.Damage: damage++; break;
                case UpgradeButton.StatType.FireRate: fireRate++; break;
                case UpgradeButton.StatType.Heat: heat++; break;
            }
        }

        public void Decrease(UpgradeButton.StatType stat)
        {
            switch (stat)
            {
                case UpgradeButton.StatType.Damage: damage--; break;
                case UpgradeButton.StatType.FireRate: fireRate--; break;
                case UpgradeButton.StatType.Heat: heat--; break;
            }
        }

        public UpgradeData Clone() => new UpgradeData { damage = damage, fireRate = fireRate, heat = heat };
    }
}