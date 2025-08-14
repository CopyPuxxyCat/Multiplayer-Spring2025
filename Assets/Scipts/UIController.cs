using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;

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
    [SerializeField] private GameObject flashPanel;
    public GameObject GP1;
    public GameObject GP2;

    #endregion

    #region Methods and Overrides

    private void Start()
    {
        DisableAllPanelWhenStart();
        UpdateMoneyUI(5000);
    }

    private void Update()
    {
        if(ChatUIManager.Instance.isTyping)
        {
            return;
        }    

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            ShowHideWeaponUpgrade(); // dong 68
        }

        if(Input.GetKeyDown(KeyCode.H))
        {
            ShowHidePickSkillPanel();
        }    
    }

    public void DisableAllPanelWhenStart()
    {
        WeaponUpgradePanel.SetActive(false);
        pickSkillPanel.SetActive(false);
        earthMinimapPanel.SetActive(false);
    }

    public void ShowHidePickSkillPanel()
    {
        bool active = !pickSkillPanel.activeInHierarchy;
        pickSkillPanel.SetActive(active);

        if (active)
            GameInputManager.Instance.LockInput(); 
        else
            GameInputManager.Instance.UnlockInput(); 
    }

    /// <summary>
    /// Show or hide pause screen
    /// </summary>
    public void ShowHideOptions()
    {
        bool active = !OptionsScreen.activeInHierarchy;
        OptionsScreen.SetActive(active);

        if (active)
        {
            GameInputManager.Instance.LockInput();  
        }
        else
        {
            GameInputManager.Instance.UnlockInput();
        }
    }


    public void ShowHideWeaponUpgrade()
    {
        if (!WeaponUpgradeManager.Instance.HasValidRefs())
        {
            Debug.LogWarning("[UIController] WeaponUpgradeManager chưa sẵn sàng.");
            return;
        }

        bool active = !WeaponUpgradePanel.activeInHierarchy;
        WeaponUpgradePanel.SetActive(active);

        if (active)
        {
            WeaponUpgradeManager.Instance.StartUpgradeSession();
            GameInputManager.Instance.LockInput();
        }
        else
        {
            GameInputManager.Instance.UnlockInput();
        }
    }

    /// <summary>
    /// flash screen
    /// </summary>

    public void FlashScreen()
    {
        StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        flashPanel.SetActive(true);
        CanvasGroup cg = flashPanel.GetComponent<CanvasGroup>();
        cg.alpha = 1f;

        float time = 0.5f;
        while (time > 0)
        {
            time -= Time.deltaTime;
            cg.alpha = Mathf.Clamp01(time / 0.5f);
            yield return null;
        }

        flashPanel.SetActive(false);
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


    public GameObject earthMinimapPanel;
    public EarthMinimap earthMinimap;
    public GameObject SandStormOverlay;

    public void ShowEarthMinimap(bool show)
    {
        if (earthMinimapPanel != null)
            earthMinimapPanel.SetActive(show);
    }

    public void ShowSandstormOverlay(bool statenow)
    {
        SandStormOverlay.SetActive(statenow);
    }    
}
