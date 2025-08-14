using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class MiniMapController : MonoBehaviourPun
{
    public static MiniMapController instance;

    [Header("MiniMap Setup")]
    public Transform mapFollowTarget; // player transform sẽ gán lúc spawn
    public RectTransform iconParent;
    public GameObject miniMapIconPrefab;
    public float mapScale = 2f;

    private Dictionary<int, MiniMapIcon> icons = new();

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (mapFollowTarget != null)
        {
            // Dịch chuyển minimap theo player
            transform.position = new Vector3(mapFollowTarget.position.x, transform.position.y, mapFollowTarget.position.z);
        }
    }

    public void SetIconVisibility(int viewID, bool visible)
    {
        if (icons.TryGetValue(viewID, out MiniMapIcon icon))
        {
            icon.gameObject.SetActive(visible);
        }
        else
        {
            Debug.LogWarning($"MiniMap icon not found for ViewID: {viewID}");
        }
    }

    public void RegisterPlayer(GameObject player)
    {
        PhotonView pv = player.GetComponent<PhotonView>();
        bool isSelf = pv.IsMine;

        GameObject iconGO = Instantiate(miniMapIconPrefab, iconParent);
        MiniMapIcon icon = iconGO.GetComponent<MiniMapIcon>();

        if (isSelf)
            mapFollowTarget = player.transform;

        icon.mapScale = mapScale;
        icon.Init(player.transform, isSelf, mapFollowTarget);

        icons[pv.ViewID] = icon;
    }
}
