using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Photon.Pun;
using System.Collections.Generic;

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

                Launcher.Instance.OpenThisPanel(Launcher.Instance.MenuButtons);
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
                Debug.Log("Đăng ký thành công, PlayFabId: " + result.PlayFabId);

                // Gọi UpdateContactEmail và xử lý kết quả
                UpdateContactEmail(email, success => {
                    if (success)
                    {
                        Debug.Log("Email verification request sent successfully to: " + email);
                        Launcher.Instance.ShowEmailVerificationPrompt(false); // Hiển thị prompt yêu cầu xác thực
                    }
                    else
                    {
                        Debug.LogWarning("Failed to send verification email. User may need to verify manually.");
                        Launcher.Instance.ShowError(Launcher.Instance.RegisterGmail, "Gửi email xác thực thất bại. Vui lòng kiểm tra lại.");
                    }

                    // Set nickname cho Photon
                    PhotonNetwork.NickName = playername;
                    PlayerPrefs.SetString("PlayerName", playername);
                    Launcher.Instance.OpenThisPanel(Launcher.Instance.LoginPanel);
                });
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

    // Hàm UpdateContactEmail được điều chỉnh để trả về callback
    private void UpdateContactEmail(string email, System.Action<bool> onComplete)
    {
        var request = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = email
        };

        PlayFabClientAPI.AddOrUpdateContactEmail(request,
            result => {
                Debug.Log("Verification email sent to: " + email);
                onComplete?.Invoke(true); // Gửi thành công
            },
            error => {
                Debug.LogError("Failed to add/update contact email: " + error.GenerateErrorReport());
                onComplete?.Invoke(false); // Gửi thất bại
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
                GetPlayerProfile = true,
                ProfileConstraints = new PlayerProfileViewConstraints { ShowContactEmailAddresses = true } // Yêu cầu thêm thông tin email
            }
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
            result => {
                Debug.Log("Login bằng email thành công! PlayFabId: " + result.PlayFabId);

                // Lấy PlayFabId từ kết quả đăng nhập
                string playFabId = result.PlayFabId;

                // Kiểm tra trạng thái xác thực trực tiếp từ InfoResultPayload (nếu bật Contact email addresses)
                if (result.InfoResultPayload != null && result.InfoResultPayload.PlayerProfile != null)
                {
                    var contactEmails = result.InfoResultPayload.PlayerProfile.ContactEmailAddresses;
                    if (contactEmails != null && contactEmails.Count > 0)
                    {
                        bool isEmailVerified = contactEmails[0].VerificationStatus.ToString() == "Confirmed"; // Hoặc dùng EmailVerified nếu có
                        Debug.Log("Direct verification status: " + contactEmails[0].VerificationStatus + " -> isVerified: " + isEmailVerified);

                        HandleLoginResult(isEmailVerified, result);
                        return; // Thoát nếu đã xử lý
                    }
                }

                // Nếu không có dữ liệu email từ client, gọi CloudScript
                PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                {
                    FunctionName = "CheckEmailVerificationStatus",
                    FunctionParameter = new { playFabId = playFabId }
                },
                cloudResult => {
                    if (cloudResult == null || cloudResult.FunctionResult == null)
                    {
                        Debug.LogError("CloudScript function returned null.");
                        Launcher.Instance.ShowError(Launcher.Instance.LoginGmail, "Lỗi khi kiểm tra tài khoản.");
                        return;
                    }

                    var cloudData = (IDictionary<string, object>)cloudResult.FunctionResult;

                    if (!cloudData.ContainsKey("isVerified"))
                    {
                        Debug.LogError("CloudScript result does not contain 'isVerified' key.");
                        Launcher.Instance.ShowError(Launcher.Instance.LoginGmail, "Lỗi khi kiểm tra dữ liệu tài khoản.");
                        return;
                    }

                    string debugMessage = cloudData.ContainsKey("debugMessage") ? cloudData["debugMessage"].ToString() : "No debug message.";
                    Debug.Log("CloudScript Debug Info: " + debugMessage);

                    bool isEmailVerified = (bool)cloudData["isVerified"];
                    Debug.Log("Email verification status: " + isEmailVerified);

                    HandleLoginResult(isEmailVerified, result);
                },
                cloudError => {
                    Debug.LogError("Failed to check email verification status via CloudScript: " + cloudError.GenerateErrorReport());
                    Launcher.Instance.ShowError(Launcher.Instance.LoginGmail, "Lỗi khi kiểm tra trạng thái tài khoản.");
                });
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

    // Hàm xử lý kết quả login để tránh lặp code
    private void HandleLoginResult(bool isEmailVerified, LoginResult result)
    {
        if (isEmailVerified)
        {
            // Đã xác thực, cho phép vào game
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

            // Mở màn hình chính
            Launcher.Instance.OpenThisPanel(Launcher.Instance.MenuButtons);
            Launcher.Instance.HasSetNickName = true;
        }
        else
        {
            // Chưa xác thực, hiển thị thông báo lỗi
            Launcher.Instance.ShowError(Launcher.Instance.LoginGmail, "Email chưa được xác thực (Pending). Vui lòng kiểm tra email và click link xác thực.");
            Launcher.Instance.ShowEmailVerificationPrompt(false);
        }
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

