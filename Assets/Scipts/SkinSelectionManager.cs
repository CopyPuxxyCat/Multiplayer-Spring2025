using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class SkinSelectionManager : MonoBehaviour
{
    public static SkinSelectionManager Instance;

    public GameObject SkinPickScreen; // Bảng chọn skin
    public Button[] SkinButtons; // Các nút chọn skin
    private PlayerController playerController;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SkinPickScreen.SetActive(false); // Ẩn menu chọn skin ban đầu

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
            int skinIndex = i; // Cần biến cục bộ để tránh lỗi delegate
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
    }
}

