using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SkinSelectionManager : MonoBehaviour
{
    #region Public Variables
    public static SkinSelectionManager Instance;

    public GameObject SkinPickScreen; 
    public Button[] SkinButtons; 
    private PlayerController playerController;
    #endregion

    #region Methods and Overrides

    void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        SkinPickScreen.SetActive(false); 

        if (PhotonNetwork.LocalPlayer != null)
        {
            GameObject localPlayer = GameObject.FindGameObjectWithTag("Player");
            if (localPlayer != null)
            {
                playerController = localPlayer.GetComponent<PlayerController>();
            }
        }

        for (int i = 0; i < SkinButtons.Length; i++)
        {
            int skinIndex = i; 
            SkinButtons[i].onClick.AddListener(() => SelectSkin(skinIndex));
        }
    }

    public void SelectSkin(int skinIndex)
    {
        if (playerController != null)
        {
            playerController.SetSkin(skinIndex);
        }
    }

    public void ToggleSkinPanel(bool state)
    {
        SkinPickScreen.SetActive(state);

        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerController != null)
        {
            playerController.canShoot = !state;
        }
    }
    #endregion
}

