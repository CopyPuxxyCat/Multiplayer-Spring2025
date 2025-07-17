using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int CurrentMoney { get; private set; }
    public Action<int> OnMoneyChanged;

    private const string CurrencyCode = "CO"; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public void RefreshMoney()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            result =>
            {
                if (result.VirtualCurrency.ContainsKey(CurrencyCode))
                {
                    CurrentMoney = result.VirtualCurrency[CurrencyCode];
                    OnMoneyChanged?.Invoke(CurrentMoney);
                    Debug.Log("Money refreshed: " + CurrentMoney);
                }
            },
            error => Debug.LogError("GetMoney failed: " + error.GenerateErrorReport()));
    }

    public void AddMoney(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = CurrencyCode,
            Amount = amount
        };

        PlayFabClientAPI.AddUserVirtualCurrency(request,
            result =>
            {
                CurrentMoney += amount;
                OnMoneyChanged?.Invoke(CurrentMoney);
                Debug.Log("Added money: " + amount);
            },
            error => Debug.LogError("AddMoney failed: " + error.GenerateErrorReport()));
    }

    public bool TrySpend(int amount)
    {
        if (CurrentMoney < amount)
        {
            Debug.Log("Not enough money.");
            return false;
        }

        var request = new SubtractUserVirtualCurrencyRequest
        {
            VirtualCurrency = CurrencyCode,
            Amount = amount
        };

        PlayFabClientAPI.SubtractUserVirtualCurrency(request,
            result =>
            {
                CurrentMoney -= amount;
                OnMoneyChanged?.Invoke(CurrentMoney);
                Debug.Log("Spent money: " + amount);
            },
            error => Debug.LogError("SpendMoney failed: " + error.GenerateErrorReport()));

        return true;
    }

    public void GrantMoneyWhenStartMatch()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "grantInitialMoney"
        },
        res => {
            Debug.Log("Đã nhận tiền đầu game: " + res.FunctionResult.ToString());
            
            UIController.instance.UpdateMoneyUI(500);
        },
        err => {
            Debug.LogError("CloudScript error: " + err.GenerateErrorReport());
        });
    }    
}

