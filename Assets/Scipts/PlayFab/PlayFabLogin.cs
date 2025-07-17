using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin instance;
    private const string CurrencyCode = "CO";
    private const int InitialCurrencyAmount = 500;

    void Start()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Logged into PlayFab successfully");

        // Reset currency at the start of each match
    }

    void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab login failed: " + error.GenerateErrorReport());
    }

    public void ResetCurrency()
    {
        // Set player currency to 500 coins for this match
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "ResetCurrencyForMatch", // function to run (see below)
            GeneratePlayStreamEvent = false
        },
        result => Debug.Log("Currency reset to 500"),
        error => Debug.LogError("Currency reset failed: " + error.GenerateErrorReport()));
    }
}

