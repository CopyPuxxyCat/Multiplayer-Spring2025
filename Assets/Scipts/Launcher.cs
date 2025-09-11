using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using PlayFab;

public class Launcher : MonoBehaviourPunCallbacks
{
    #region Public Variables
    
    public static Launcher Instance;
    public GameObject LoadingScreen;
    public TMP_Text LoadingText; 
    public GameObject MenuButtons;
    public GameObject CreateRoomScreen;
    public TMP_InputField RoomNameInput;
    public GameObject RoomScreen;
    public TMP_Text RoomNameText, PlayerNameLabel;
    public GameObject ErrorScreen;
    public TMP_Text ErrorText;
    public GameObject RoomBrowserScreen;
    public RoomButton RoomButton;
    public GameObject NameInputScreen;
    public TMP_InputField NameInputText;
    public string LevelToPlay;
    public GameObject StartButton;
    public GameObject RoomTestButton;
    public bool HasSetNickName;
    public string[] AllMaps;
    public bool ChangeMapBetweenRounds = true;

    public Color validColor = Color.green;
    public Color errorColor = Color.red;
    public Color emptyColor = Color.white;

    [Header("Google PlayFab UI")]
    public GameObject SignUpPanel;
    public GameObject LoginPanel;
    public TMP_InputField RegisterGmail;
    public TMP_InputField RegisterPassword;
    public TMP_InputField RegisterPasswordAgain;
    public TMP_InputField RegisterPlayerName;
    public TMP_InputField LoginGmail;
    public TMP_InputField LoginPassword;
    public TMP_InputField ForgotPasswordEmailField;
    public GameObject PasswordRecoverPanel;

    [Header("UI Panels")]
    public GameObject EmailVerificationPanel;
    public TMP_Text VerificationText;
    public TMP_Text VerificationStateText;

    #endregion

    #region Private Variables 

    private List<TMP_Text> AllPlayerNames = new List<TMP_Text>();
    private List<RoomButton> AllRoomButtons = new List<RoomButton>();

    #endregion

    #region Overrides and Methods

    /// <summary>
    /// Awake method of unity
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Start method of unity
    /// </summary>
    private void Start()
    {
        CloseMenus();
        LoadingScreen.SetActive(true);
        LoadingText.text = "Connecting To Network...";

        //PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.ConnectToRegion("asia"); // make sure every player in the same region

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.ConnectToRegion("asia");/// Uses photon server settings to connect to photon network
        }

#if UNITY_EDITOR
        RoomTestButton.SetActive(true);
#endif

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Method called when connected to master server and ready for matchmaking or lobby
    /// </summary>
    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        //CloseMenus();
        //MenuButtons.SetActive(true);
        PhotonNetwork.JoinLobby();

        /// Tells the client to load same scene as master
        PhotonNetwork.AutomaticallySyncScene = true;
        
        LoadingText.text = "Joining Lobby...";
    }

    /// <summary>
    /// Called when enter a lobby on master server
    /// </summary>
    public override void OnJoinedLobby()
    {
        //base.OnJoinedLobby();
        CloseMenus();
        MenuButtons.SetActive(true);

        /// nickname is the displayed name to us
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if(!HasSetNickName)
        {
            CloseMenus();
            NameInputScreen.SetActive(true);
            
            /// If we have name stored, than set name to that string
            if(PlayerPrefs.HasKey("PlayerName"))
            {
                NameInputText.text = PlayerPrefs.GetString("PlayerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
        }
    }

    /// <summary>
    /// Close all menus and buttons
    /// </summary>
    void CloseMenus()
    {
        LoadingScreen.SetActive(false);
        MenuButtons.SetActive(false);
        CreateRoomScreen.SetActive(false);
        RoomScreen.SetActive(false);
        ErrorScreen.SetActive(false);
        RoomBrowserScreen.SetActive(false);
        NameInputScreen.SetActive(false);
        LoginPanel.SetActive(false);
        SignUpPanel.SetActive(false);
        PasswordRecoverPanel.SetActive(false);
        HistoryPanel.SetActive(false);
        EmailVerificationPanel.SetActive(false);
    }

    /// <summary>
    /// Open create room menu
    /// Called on button click in scene
    /// </summary>
    public void OpenCreateRoom()
    {
        CloseMenus();
        CreateRoomScreen.SetActive(true);
    }

    /// <summary>
    /// Create Room
    /// </summary>
    public void CreateRoom()
    {
        if(!string.IsNullOrEmpty(RoomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;
            PhotonNetwork.CreateRoom(RoomNameInput.text, options);
            CloseMenus();
            LoadingText.text = "Creating Room...";
            LoadingScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Set name of the joined room
    /// </summary>
    public override void OnJoinedRoom()
    {
        CloseMenus();
        RoomScreen.SetActive(true);


        RoomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayers();

        /// if current player is the master enable start game button
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }


    /// <summary>
    /// Display name of all players in the lobby
    /// </summary>
    private void ListAllPlayers()
    {
        foreach (var p in AllPlayerNames)
            Destroy(p.gameObject);
        AllPlayerNames.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
            CreatePlayerEntry(player);
    }

    /// <summary>
    /// When player enters the room
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CreatePlayerEntry(newPlayer);


    }

    /// <summary>
    /// When player leaves the room
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    /// <summary>
    /// Called when room creation failed
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        CloseMenus();
        ErrorText.text = $"Failed to create new room : {message}";
        ErrorScreen.SetActive(true);
    }

    /// <summary>
    /// Close error screen and open main menu
    /// </summary>
    public void CloseErrorScreen()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    /// <summary>
    /// Leave room
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        LoadingText.text = "Leaving Room... ";
        LoadingScreen.SetActive(true);
    }

    /// <summary>
    /// After room is left
    /// </summary>
    public override void OnLeftRoom()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }
    
    /// <summary>
    /// open room browser window
    /// </summary>
    public void OpenRoomBrowser()
    {
        CloseMenus();
        RoomBrowserScreen.SetActive(true);
    }

    /// <summary>
    /// Close room browser window
    /// </summary>
    public void CloseRoomBrowser()
    {
        CloseMenus();
        MenuButtons.SetActive(true);
    }

    /// <summary>
    /// Called when room list is updated
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomButton rb in AllRoomButtons)
        {
            Destroy(rb.gameObject);
        }
        AllRoomButtons.Clear();

        RoomButton.gameObject.SetActive(false);

        for(int i=0;i<roomList.Count;i++)
        {
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList)
            {
                /// instantiate new button
                RoomButton newButton = Instantiate(RoomButton, RoomButton.transform.parent);
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);
                AllRoomButtons.Add(newButton);
            }
            /// Workaround for data not updating when player joins an already started room
            if (!roomList[i].IsOpen)
            {
                AllRoomButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Join room
    /// </summary>
    /// <param name="inputInfo"></param>
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        LoadingText.text = "Joining Room...";
        LoadingScreen.SetActive(true);
    }

    /// <summary>
    /// Set Player nick name
    /// </summary>
    public void SetNickName()
    {
        if(PlayFabLogin.instance == null)
        {
            Debug.Log("chua co playfablogin");

        }
            
        if(!string.IsNullOrEmpty(NameInputText.text))
        {
            PhotonNetwork.NickName = NameInputText.text;

            /// use playerprefs for storing nick name to avoid new name everytime game runs
            PlayerPrefs.SetString("PlayerName", NameInputText.text);

            PlayFabLogin.instance.PlayAsGuest(NameInputText.text);
            HasSetNickName = true;
        }
    }

    /// <summary>
    /// google sign up
    /// </summary>
    public void SignUpForGoogle()
    {
        OpenThisPanel(SignUpPanel);

        bool valid = true;

        string email = RegisterGmail.text.Trim();
        string password = RegisterPassword.text.Trim();
        string passwordAgain = RegisterPasswordAgain.text.Trim();
        string playerName = RegisterPlayerName.text.Trim();

        // Reset error UI
        ResetField(RegisterGmail);
        ResetField(RegisterPassword);
        ResetField(RegisterPasswordAgain);
        ResetField(RegisterPlayerName);

        // Check email
        if (string.IsNullOrEmpty(email) || !IsValidGmail(email))
        {
            ShowError(RegisterGmail, "* Email không hợp lệ, phải là Gmail.");
            valid = false;
        }
        else MarkValid(RegisterGmail);

        // Check password
        if (string.IsNullOrEmpty(password) || !IsValidPassword(password))
        {
            ShowError(RegisterPassword, "* Mật khẩu phải ≥8 ký tự, có số, có chữ, bắt đầu bằng chữ Hoa.");
            valid = false;
        }
        else MarkValid(RegisterPassword);

        // Check repeat password
        if (password != passwordAgain)
        {
            ShowError(RegisterPasswordAgain, "* Mật khẩu nhập lại không khớp.");
            valid = false;
        }
        else MarkValid(RegisterPasswordAgain);

        // Check player name
        if (string.IsNullOrEmpty(playerName) || playerName.Contains(" "))
        {
            ShowError(RegisterPlayerName, "* Tên không được để trống hoặc chứa khoảng trắng.");
            valid = false;
        }
        else MarkValid(RegisterPlayerName);

        if (!valid) return;

        PlayFabLogin.instance.RegisterWithEmail(email, password, playerName);
    }
        

    /// <summary>
    /// google sign in
    /// </summary>
    public void GoogleLogin()
    {
        OpenThisPanel(LoginPanel);

        bool valid = true;

        string email = LoginGmail.text.Trim();
        string password = LoginPassword.text.Trim();

        ResetField(LoginGmail);
        ResetField(LoginPassword);

        // Check email
        if (string.IsNullOrEmpty(email) || !IsValidGmail(email))
        {
            ShowError(LoginGmail, "* Email không hợp lệ, phải là Gmail.");
            valid = false;
        }
        else MarkValid(LoginGmail);

        // Check password
        if (string.IsNullOrEmpty(password))
        {
            ShowError(LoginPassword, "* Mật khẩu không được để trống.");
            valid = false;
        }
        else MarkValid(LoginPassword);

        if (!valid) return;

        PlayFabLogin.instance.LoginWithEmail(email, password);
    }

    public void OnClickForgotPassword()
    {
        
        string email = ForgotPasswordEmailField.text.Trim();

        ResetField(ForgotPasswordEmailField);

        // Check input
        if (string.IsNullOrEmpty(email))
        {
            ShowError(ForgotPasswordEmailField, "* Vui lòng nhập Gmail.");
            return;
        }
        if (!IsValidGmail(email))
        {
            ShowError(ForgotPasswordEmailField, "* Email không hợp lệ, phải là @gmail.com.");
            return;
        }

        // Gọi PlayFab
        PlayFabLogin.instance.ForgotPassword(email,
            () =>
            {
                MarkValid(ForgotPasswordEmailField);
                Debug.Log("Vui lòng kiểm tra email để đặt lại mật khẩu.");
            },
            (errMsg) =>
            {
                ShowError(ForgotPasswordEmailField, "* Không tìm thấy email trong hệ thống.");
            });
    }

    public void OpenThisPanel(GameObject panelOBJ)
    {
        CloseMenus();
        panelOBJ.SetActive(true);
    }    
    /// </summary>

    /// <summary>
    /// Method to start game and tell master the scene to load
    /// </summary>
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(AllMaps[Random.Range(0, AllMaps.Length)]);
        //PhotonNetwork.LoadLevel(LevelToPlay);
        /// Workaround for data not updating when player joins an already started room
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    /// <summary>
    /// Switch master if previous master terminates the game
    /// </summary>
    /// <param name="newMasterClient"></param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        /// if current player is the master enable start game button
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
        }
        else
        {
            StartButton.SetActive(false);
        }
    }

    /// <summary>
    /// Method to run quick join on unity editor for test
    /// </summary>
    public void QuickJoin()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
        PhotonNetwork.CreateRoom("Test", options);
        CloseMenus();
        LoadingText.text = "Creating Room...";
        LoadingScreen.SetActive(true);
    }

    /// <summary>
    /// Close Game window (only builds)
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// History panel
    /// </summary>

    [Header("History UI")]
    public GameObject HistoryPanel;
    public Transform HistoryContent;
    public GameObject HistoryItemPrefab;

    public void OpenHistoryPanel()
    {
        CloseMenus();
        HistoryPanel.SetActive(true);

        // Xóa cũ
        foreach (Transform child in HistoryContent)
            Destroy(child.gameObject);

        // Lấy dữ liệu từ PlayFab
        PlayFabClientAPI.GetUserData(new PlayFab.ClientModels.GetUserDataRequest(),
            result =>
            {
                foreach (var record in result.Data)
                {
                    if (!record.Key.StartsWith("Match")) continue;

                    var entry = JsonUtility.FromJson<HistoryEntry>(record.Value.Value);

                    GameObject go = Instantiate(HistoryItemPrefab, HistoryContent);
                    var texts = go.GetComponentsInChildren<TMPro.TMP_Text>();
                    texts[0].text = entry.Time;
                    texts[1].text = $"Kills: {entry.Kills} | Deaths: {entry.Deaths}";
                    texts[2].text = entry.Result;

                    // Đổi màu theo kết quả
                    var img = go.GetComponent<UnityEngine.UI.Image>();
                    img.color = entry.Result == "Win" ? Color.green : Color.red;
                }
            },
            error =>
            {
                Debug.LogError("Load history failed: " + error.GenerateErrorReport());
            });
    }

    [System.Serializable]
    public class HistoryEntry
    {
        public string Time;
        public string Kills;
        public string Deaths;
        public string Result;
    }


    #endregion

    #region Helper
    private void CreatePlayerEntry(Player player)
    {
        GameObject entryGO = Instantiate(PlayerNameLabel.gameObject, PlayerNameLabel.transform.parent);
        entryGO.SetActive(true);

        var entryUI = entryGO.GetComponent<PlayerListEntryDropdownUI>();
        entryUI.Setup(player);

        AllPlayerNames.Add(entryUI.playerNameText);
    }

    private bool IsValidGmail(string email)
    {
        return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@gmail\.com$");
    }

    private bool IsValidPassword(string password)
    {
        // ≥8 ký tự, chứa số, chữ, bắt đầu bằng chữ Hoa
        return Regex.IsMatch(password, @"^[A-Z][A-Za-z0-9]{7,}$") &&
               Regex.IsMatch(password, @"\d") && // ít nhất 1 số
               Regex.IsMatch(password, @"[A-Za-z]"); // ít nhất 1 chữ
    }

    public void ShowError(TMP_InputField field, string message)
    {
        field.image.color = errorColor;

        // Tạo 1 error text nếu chưa có
        TMP_Text errorText = field.transform.Find("ErrorText")?.GetComponent<TMP_Text>();
        if (errorText == null)
        {
            GameObject errObj = new GameObject("ErrorText", typeof(RectTransform));
            errObj.transform.SetParent(field.transform);
            errObj.transform.localScale = Vector3.one;

            errorText = errObj.AddComponent<TextMeshProUGUI>();
            errorText.fontSize = 20;
            errorText.color = Color.red;

            RectTransform rt = errObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -20);
            rt.sizeDelta = new Vector2(0, 25);
        }

        errorText.text = message;
    }

    public void ShowEmailVerificationPrompt(bool isverified)
    {
        EmailVerificationPanel.SetActive(true);
        VerificationText.text = "Đăng ký thành công! Vui lòng kiểm tra email của bạn để xác nhận tài khoản trước khi đăng nhập. Email có thể ở trong mục Spam. Dưới đây là trạng thái đã xác thực tài khoản chưa: ";
        if (isverified == false)
        {
            VerificationStateText.text = "Chưa xác thực. Vui lòng kiểm tra Email. Email có thể ở trong mục Spam!";
        }
        else
        {
            VerificationStateText.text = "Đã xác thực. Bạn có thể đăng nhập bằng tài khoản này!";
        }    
    }

    public void CloseShowEmailVerificationPrompt()
    {
        EmailVerificationPanel.SetActive(false);
    }    

    private void ResetField(TMP_InputField field)
    {
        field.image.color = emptyColor;
        var err = field.transform.Find("ErrorText");
        if (err != null) Destroy(err.gameObject);
    }

    private void MarkValid(TMP_InputField field)
    {
        field.image.color = validColor;
        var err = field.transform.Find("ErrorText");
        if (err != null) Destroy(err.gameObject);
    }

    #endregion
}
