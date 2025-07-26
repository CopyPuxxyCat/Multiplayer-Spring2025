using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

/// <summary>
/// Singleton
/// Cannot attach to player because player is killed and respawned which caues the null reference,
/// that's why create a singleton
/// </summary>

public class UIController : MonoBehaviour
{


    #region Instance
    
    public static UIController instance;

    void Awake()
    {
        instance = this;
    }

    #endregion

    #region Public Variables
    
    public TMP_Text OverheatedMessage;
    public Slider WeaponTemperatureSlider;
    public GameObject DeathScreen;
    public TMP_Text DeathText;
    public Slider HealthSlider;
    public Slider ShieldSlider;
    public TMP_Text KillsText;
    public TMP_Text DeathsLabel;
    public GameObject LeaderBoard;
    public LeaderBoardPlayer leaderBoardPlayerDisplay;
    public GameObject EndScreen;
    public GameObject OptionsScreen;
    public SkillUIEntry skill1UI;
    public SkillUIEntry skill2UI;
    public SkillUIEntry skill3UI;
    public SkillUIEntry ultimateUI;
    public TMP_Text ingameMoney;
    public GameObject WeaponUpgradePanel;
    [SerializeField] private GameObject pickSkillPanel;

    #endregion

    #region Methods and Overrides

    private void Start()
    {
        DisableAllPanelWhenStart();
        UpdateMoneyUI(5000);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            ShowHideOptions();
            ShowHideWeaponUpgrade(); // dong 69
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            ShowHideOptions();
            ShowHidePickSkillPanel();
        }    
    }

    public void DisableAllPanelWhenStart()
    {
        WeaponUpgradePanel.SetActive(false);
        pickSkillPanel.SetActive(false);
    }

    public void ShowHidePickSkillPanel()
    {
        bool active = !pickSkillPanel.activeInHierarchy;
        pickSkillPanel.SetActive(active);

        if (active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Show or hide pause screen
    /// </summary>
    public void ShowHideOptions()
    {
        if(!OptionsScreen.activeInHierarchy)
        {
            OptionsScreen.SetActive(true);
            if(Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            OptionsScreen.SetActive(false);
        }
    }

    public void ShowHideWeaponUpgrade()
    {
        if (!WeaponUpgradeManager.Instance.HasValidRefs())
        {
            Debug.LogWarning("[UIController] WeaponUpgradeManager chưa sẵn sàng.");
            return;
        }

        // Toggle panel
        bool active = !WeaponUpgradePanel.activeInHierarchy;
        WeaponUpgradePanel.SetActive(active);

        if (active)
        {
            WeaponUpgradeManager.Instance.StartUpgradeSession();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// close current game and return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Quit game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateMoneyUI(int amount)
    {
        ingameMoney.text = "$" + amount.ToString();
    }
    #endregion
}
