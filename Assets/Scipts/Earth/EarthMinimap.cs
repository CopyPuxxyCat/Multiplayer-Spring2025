using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;

public class EarthMinimap : MonoBehaviour, IPointerClickHandler
{
    [Header("References")]
    public Camera minimapCamera;
    public RectTransform minimapRect;
    public RectTransform minimapHolder;  // Holder chứa icon UI
    public Sprite previewIconSprite;     // Sprite icon preview
    public LayerMask anyLayerMask = Physics.DefaultRaycastLayers;
    public float navSampleMaxDistance = 3f;

    private Vector3 lastWorldPos = Vector3.zero;

    // Lưu (iconUI, worldPos)
    private readonly List<(RectTransform icon, Vector3 worldPos)> previewIcons =
        new List<(RectTransform, Vector3)>();

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 screenPos = eventData.position;
        Vector3 world = ScreenToWorld(screenPos);
        if (world != Vector3.zero)
        {
            lastWorldPos = world;
            SpawnPreviewIcon(world);
        }
    }

    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        if (minimapCamera == null) return Vector3.zero;
        Ray ray = minimapCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, anyLayerMask))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, navSampleMaxDistance, NavMesh.AllAreas))
            {
                return navHit.position;
            }
        }
        return Vector3.zero;
    }

    public Vector3 GetLastClickedWorldPosition()
    {
        return lastWorldPos;
    }

    public void SpawnPreviewIcon(Vector3 worldPos)
    {
        if (previewIcons.Count >= 3)
        {
            Debug.Log("Đã đủ 3 icon preview smoke.");
            return;
        }

        if (minimapHolder == null || previewIconSprite == null)
        {
            Debug.LogWarning("MinimapHolder hoặc previewIconSprite chưa được gán!");
            return;
        }

        GameObject iconGO = new GameObject("SmokePreviewIcon", typeof(Image));
        iconGO.transform.SetParent(minimapHolder, false);

        var img = iconGO.GetComponent<Image>();
        img.sprite = previewIconSprite;
        img.SetNativeSize();

        RectTransform iconRect = iconGO.GetComponent<RectTransform>();
        previewIcons.Add((iconRect, worldPos));

        UpdateIconPosition(iconRect, worldPos);
    }

    void Update()
    {
        // Cập nhật vị trí icon dựa trên worldPos
        foreach (var (icon, worldPos) in previewIcons)
        {
            if (icon != null)
                UpdateIconPosition(icon, worldPos);
        }
    }

    private void UpdateIconPosition(RectTransform iconRect, Vector3 worldPos)
    {
        Vector3 viewportPos = minimapCamera.WorldToViewportPoint(worldPos);
        Vector2 minimapSize = minimapRect.sizeDelta;

        Vector2 localPos = new Vector2(
            (viewportPos.x - 0.5f) * minimapSize.x,
            (viewportPos.y - 0.5f) * minimapSize.y
        );

        iconRect.anchoredPosition = localPos;
    }

    public void ClearPreviewIcons()
    {
        foreach (var (icon, _) in previewIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        previewIcons.Clear();
    }
}
