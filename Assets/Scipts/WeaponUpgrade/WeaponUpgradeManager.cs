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

    #region save and load gun data

    public void SaveBuild()
    {
        var build = new SavedBuild
        {
            armorSmall = armorSmall,
            armorBig = armorBig
        };

        foreach (var kv in currentUpgrades)
        {
            build.guns.Add(new GunUpgrade
            {
                gunName = kv.Key.gunData.gunName,
                damage = kv.Value.damage,
                fireRate = kv.Value.fireRate,
                heat = kv.Value.heat
            });
        }

        string json = JsonUtility.ToJson(build);

        PlayFab.PlayFabClientAPI.UpdateUserData(new PlayFab.ClientModels.UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "SavedBuild", json } }
        },
        res => Debug.Log("Build saved to PlayFab."),
        err => Debug.LogError("Save build failed: " + err.GenerateErrorReport()));
    }

    public void LoadBuild()
    {
        PlayFab.PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest(),
        res =>
        {
            if (res.Data != null && res.Data.ContainsKey("SavedBuild"))
            {
                var build = JsonUtility.FromJson<SavedBuild>(res.Data["SavedBuild"].Value);

                int totalCost = 0;

                foreach (var gunUpgrade in build.guns)
                {
                    foreach (var gun in indexToGun.Values)
                    {
                        if (gun.gunData.gunName == gunUpgrade.gunName)
                        {
                            var data = new UpgradeData
                            {
                                damage = gunUpgrade.damage,
                                fireRate = gunUpgrade.fireRate,
                                heat = gunUpgrade.heat
                            };

                            // Gán vào current + temp
                            currentUpgrades[gun] = data.Clone();
                            tempUpgrades[gun] = data.Clone();

                            // Áp stats lên súng
                            gun.ApplyUpgrades(data.damage, data.fireRate, data.heat);

                            // Tính tổng chi phí cho các level
                            totalCost += CalcUpgradeCost(data.damage);
                            totalCost += CalcUpgradeCost(data.fireRate);
                            totalCost += CalcUpgradeCost(data.heat);
                        }
                    }
                }

                // Armor
                int armor = 0;
                if (build.armorSmall) { armor += 100; totalCost += smallArmorCost; }
                if (build.armorBig) { armor += 200; totalCost += bigArmorCost; }
                playerController.AddArmor(armor);

                // Update UI ArmorButtons
                foreach (var btn in UI.armorButtons)
                {
                    if (btn.armorType == ArmorButton.ArmorType.Small)
                        btn.SetSelected(build.armorSmall);
                    else if (btn.armorType == ArmorButton.ArmorType.Big)
                        btn.SetSelected(build.armorBig);
                }

                // Trừ tiền
                coinInStore = Mathf.Max(0, currencyManager.CurrentCoin - totalCost);
                currencyManager.SetCoin(coinInStore);

                // Refresh UI
                UI.UpdateUI(coinInStore);
                UI.RefreshLevels(); // <- cần viết trong UIWeaponUpgrade để update level nút

                Debug.Log($"Build loaded! Cost: {totalCost}, Remaining Coin: {coinInStore}");
            }
            else
            {
                Debug.Log("No saved build found.");
            }
        },
        err => Debug.LogError("Load build failed: " + err.GenerateErrorReport()));
    }

    public int GetCurrentCoin() => coinInStore;

    // Hàm phụ tính chi phí upgrade
    private int CalcUpgradeCost(int level)
    {
        int sum = 0;
        for (int i = 0; i < level; i++)
            sum += upgradeCosts[Mathf.Min(i, upgradeCosts.Length - 1)];
        return sum;
    }


    // Helper
    [System.Serializable]
    public class SavedBuild
    {
        public List<GunUpgrade> guns = new();
        public bool armorSmall;
        public bool armorBig;
    }

    [System.Serializable]
    public class GunUpgrade
    {
        public string gunName;
        public int damage;
        public int fireRate;
        public int heat;
    }
    #endregion
}