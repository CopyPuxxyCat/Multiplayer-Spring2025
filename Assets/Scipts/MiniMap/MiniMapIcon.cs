using UnityEngine;
using UnityEngine.UI;

public class MiniMapIcon : MonoBehaviour
{
    public Transform target; // Transform của player đại diện cho icon
    public float mapScale = 1f;
    public bool isSelf;

    private Image iconImage;
    private RectTransform iconRect;

    private Transform mapCenter; // chính là transform của player local (follow target)

    void Awake()
    {
        iconRect = GetComponent<RectTransform>();
        iconImage = GetComponent<Image>();
        gameObject.SetActive(true);
    }

    public void Init(Transform target, bool isSelf, Transform mapFollowTarget)
    {
        this.target = target;
        this.isSelf = isSelf;
        mapCenter = mapFollowTarget;

        iconImage.color = isSelf ? Color.green : Color.red;
    }

    private void Update()
    {

        if (target == null || mapCenter == null) return;
        // Tính offset giữa target và mapCenter (tức player local)
        Vector3 offset = target.position - mapCenter.position;

        // Lấy vị trí icon trên minimap
        float x = offset.x * mapScale;
        float y = offset.z * mapScale;

        iconRect.anchoredPosition = new Vector2(x, y);

        if (isSelf)
        {
            float angle = target.eulerAngles.y;
            iconRect.localRotation = Quaternion.Euler(0, 0, -angle);
        }
        else
        {
            iconRect.localRotation = Quaternion.identity;
        }
    }
}
