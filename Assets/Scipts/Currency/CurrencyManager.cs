// =========================
// CurrencyManager.cs (attached to Player)
// =========================
using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public int CurrentCoin = 5000;
    public Action<int> OnMoneyChanged;

    public void SetCoin(int amount)
    {
        CurrentCoin = amount;
        OnMoneyChanged?.Invoke(CurrentCoin);
        UIController.instance.UpdateMoneyUI(CurrentCoin);
    }

    public bool TrySpend(int amount)
    {
        if (CurrentCoin >= amount)
        {
            CurrentCoin -= amount;
            OnMoneyChanged?.Invoke(CurrentCoin);
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        CurrentCoin += amount;
        OnMoneyChanged?.Invoke(CurrentCoin);
    }
}
