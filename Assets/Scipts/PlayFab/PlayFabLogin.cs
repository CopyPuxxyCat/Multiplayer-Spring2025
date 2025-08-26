using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;

public class PlayFabLogin : MonoBehaviour
{
    public static PlayFabLogin instance;
    private const string CurrencyCode = "CO";
    private const int InitialCurrencyAmount = 500;

    void Awake()
    {
        instance = this;
    }

    void OnLoginSuccess(LoginResult result)
    {

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

    public void PlayAsGuest(string playername)
    {
        
        string customId;
        if (PlayerPrefs.HasKey("GuestID"))
            customId = PlayerPrefs.GetString("GuestID");
        else
        {
            customId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("GuestID", customId);
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = customId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request,
            result => {
                Debug.Log("Guest login success! PlayFabId: " + result.PlayFabId);

                // Set display name bằng playername
                PlayFabClientAPI.UpdateUserTitleDisplayName(
                    new UpdateUserTitleDisplayNameRequest { DisplayName = playername },
                    displayNameResult => Debug.Log("Display name set to: " + displayNameResult.DisplayName),
                    error => Debug.LogError(error.GenerateErrorReport())
                );
            },
            error => Debug.LogError("Guest login failed: " + error.GenerateErrorReport())
        );
    }
    public void RegisterWithEmail(string email, string password, string playername)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            DisplayName = playername,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request,
            result => {
                Debug.Log("Đăng ký thành công, PlayFabId : " + result.PlayFabId);

                // Sau khi đăng ký xong là user đã đăng nhập rồi, có thể gọi UpdateContactEmail luôn
                UpdateContactEmail(email);

                // Set nickname cho Photon
                PhotonNetwork.NickName = playername;
                PlayerPrefs.SetString("PlayerName", playername);
                Launcher.Instance.OpenThisPanel(Launcher.Instance.LoginPanel);
            },
            error =>
            {
                if (error.Error == PlayFabErrorCode.EmailAddressNotAvailable)
                    Launcher.Instance.ShowError(Launcher.Instance.RegisterGmail, "Email này đã được dùng để đăng ký.");
                else if (error.Error == PlayFabErrorCode.NameNotAvailable)
                    Launcher.Instance.ShowError(Launcher.Instance.RegisterPlayerName, "Tên này đã có người dùng.");
                else
                    Launcher.Instance.ShowError(Launcher.Instance.RegisterGmail, error.ErrorMessage);
            });
    }


    public void LoginWithEmail(string email, string password)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
            result => {
                Debug.Log("Login bằng email thành công! PlayFabId: " + result.PlayFabId);
                // lấy DisplayName để làm PhotonNetwork.NickName
                string displayName = result.InfoResultPayload?.PlayerProfile?.DisplayName;

                if (!string.IsNullOrEmpty(displayName))
                {
                    PhotonNetwork.NickName = displayName;
                    PlayerPrefs.SetString("PlayerName", displayName);
                }
                else
                {
                    PhotonNetwork.NickName = "Player" + Random.Range(1000, 9999);
                }

                Launcher.Instance.OpenThisPanel(Launcher.Instance.MenuButtons);
                Launcher.Instance.HasSetNickName = true;
            },
            error =>
            {
                if (error.Error == PlayFabErrorCode.AccountNotFound)
                {
                    Launcher.Instance.ShowError(Launcher.Instance.LoginGmail, "* Email này chưa được đăng ký.");
                }
                else if (error.Error == PlayFabErrorCode.InvalidEmailOrPassword)
                {
                    Launcher.Instance.ShowError(Launcher.Instance.LoginPassword, "* Sai mật khẩu.");
                }
                else
                {
                    Launcher.Instance.ShowError(Launcher.Instance.LoginGmail, error.ErrorMessage);
                }
            });
    }

    private void UpdateContactEmail(string email)
    {
        var request = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = email
        };

        PlayFabClientAPI.AddOrUpdateContactEmail(request,
            result => {
                Debug.Log("Verification email sent to: " + email);
            },
            error => {
                Debug.LogError("Failed to add/update contact email: " + error.GenerateErrorReport());
            });
    }

    /// <summary>
    /// Gửi yêu cầu reset password tới PlayFab (người dùng sẽ nhận mail để đặt lại)
    /// </summary>
    public void ForgotPassword(string email, System.Action onSuccess, System.Action<string> onError)
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = email,
            TitleId = PlayFabSettings.TitleId
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request,
            result =>
            {
                Debug.Log("Gửi mail reset password thành công.");
                onSuccess?.Invoke();
                Launcher.Instance.OpenThisPanel(Launcher.Instance.LoginPanel);
            },
            error =>
            {
                if (error.Error == PlayFabErrorCode.AccountNotFound)
                {
                    Launcher.Instance.ShowError(Launcher.Instance.ForgotPasswordEmailField, "* Email này không tồn tại.");
                }
                Debug.LogError("ForgotPassword error: " + error.GenerateErrorReport());
                onError?.Invoke(error.ErrorMessage);
            });
    }

}

