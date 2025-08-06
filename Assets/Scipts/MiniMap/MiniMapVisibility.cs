using Photon.Pun;
using UnityEngine;

public class MiniMapVisibility : MonoBehaviour
{
    public GameObject miniMapIcon;
    private PhotonView pv;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public void ShowOnMiniMap(bool show)
    {
            miniMapIcon.SetActive(show);
    }
}
