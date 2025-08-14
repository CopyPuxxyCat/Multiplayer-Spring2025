using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EarthMinimap : MonoBehaviour
{
    [Header("Refs")]
    public Camera minimapCamera;         // Camera top-down render vào RenderTexture
    public RectTransform minimapRect;    // RectTransform của RawImage minimap
    public RectTransform minimapHolder;  // Holder chứa các icon UI

    [Header("Prefabs/Assets")]
    // Icon preview trên minimap: lấy từ EarthStats.smokePreviewMiniResourceName => Resources/Earth/<name>
    // Nếu bạn dùng Sprite thuần thay vì prefab UI, có thể đổi cách load tuỳ bạn.
    public string smokePreviewMiniResourceName = "SmokePreviewOnMiniMap";
    public Sprite cursorSprite;          // Sprite icon con trỏ (UI) di chuyển theo chuột

    [Header("Layers")]
    public LayerMask groundMask;         // chỉ Ground

    // Icon con trỏ chạy theo chuột khi panel mở
    private RectTransform cursorIcon;
    // Lưu danh sách (icon UI, world pos)
    private readonly List<(RectTransform icon, Vector3 world)> previews = new();

    void Update()
    {
        // Cập nhật vị trí cursor icon nếu đang bật
        if (cursorIcon != null)
        {
            if (ScreenPointToMinimapLocalPoint(Input.mousePosition, out Vector2 local))
            {
                cursorIcon.anchoredPosition = local;
            }
        }

        // Cập nhật các icon preview bám world pos → UI
        for (int i = 0; i < previews.Count; i++)
        {
            var (icon, world) = previews[i];
            if (icon)
            {
                icon.anchoredPosition = WorldToMinimapLocal(world);
            }
        }
    }

    // Bật/tắt cursor icon
    public void ShowCursor(bool show)
    {
        if (show)
        {
            if (cursorIcon == null)
            {
                GameObject go = new GameObject("MinimapCursor", typeof(Image));
                go.transform.SetParent(minimapHolder, false);
                var img = go.GetComponent<Image>();
                img.sprite = cursorSprite;
                img.SetNativeSize();
                cursorIcon = go.GetComponent<RectTransform>();
            }
        }
        else
        {
            if (cursorIcon) Destroy(cursorIcon.gameObject);
            cursorIcon = null;
        }
    }

    // Có đang hover bên trong minimap panel không?
    public bool IsPointerOverMinimap()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(minimapRect, Input.mousePosition);
    }

    // Lấy world pos ngay dưới con trỏ minimap bằng ray của minimapCamera
    public bool TryGetCursorWorld(out Vector3 world, float planeHeight = 0f)
    {
        world = Vector3.zero;
        if (!IsPointerOverMinimap()) return false;

        // Convert screen → normalized viewport [0..1] theo minimapRect
        if (!ScreenPointToViewportOnMinimap(Input.mousePosition, out Vector2 vp)) return false;

        Ray ray = minimapCamera.ViewportPointToRay(new Vector3(vp.x, vp.y, 0f));

        // Ưu tiên cast vào Ground
        if (Physics.Raycast(ray, out RaycastHit hit, 2000f, groundMask))
        {
            world = hit.point;
            return true;
        }

        // Fallback: cắt mặt phẳng ngang tại planeHeight
        Plane plane = new Plane(Vector3.up, new Vector3(0, planeHeight, 0));
        if (plane.Raycast(ray, out float enter))
        {
            world = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

    // Thêm icon preview tại world pos (cố định)
    public void AddPreviewIcon(Vector3 worldPos)
    {
        if (previews.Count >= 3) return;

        // Nếu bạn đã có prefab UI trong Resources/Earth/<smokePreviewMiniResourceName>, dùng cách này:
        GameObject prefab = Resources.Load<GameObject>("Earth/" + smokePreviewMiniResourceName);
        RectTransform icon;
        if (prefab != null)
        {
            var go = Instantiate(prefab, minimapHolder);
            icon = go.GetComponent<RectTransform>();
            if (icon == null) icon = go.AddComponent<RectTransform>();
        }
        else
        {
            // Nếu không có prefab UI, dùng 1 Image runtime
            var go = new GameObject("SmokePreviewIcon", typeof(Image));
            go.transform.SetParent(minimapHolder, false);
            var img = go.GetComponent<Image>();
            // Bạn có thể thay bằng Sprite theo ý
            img.sprite = cursorSprite;
            img.SetNativeSize();
            icon = go.GetComponent<RectTransform>();
        }

        icon.anchoredPosition = WorldToMinimapLocal(worldPos);
        previews.Add((icon, worldPos));
    }

    public void ClearPreviewIcons()
    {
        foreach (var (icon, _) in previews)
            if (icon) Destroy(icon.gameObject);
        previews.Clear();
    }

    // ——— Helpers ———

    // Convert world → local pos trong minimapRect
    private Vector2 WorldToMinimapLocal(Vector3 world)
    {
        Vector3 vp3 = minimapCamera.WorldToViewportPoint(world);
        Vector2 size = minimapRect.rect.size; // width,height
        // viewport (0..1) → local (-w/2..w/2, -h/2..h/2)
        return new Vector2((vp3.x - 0.5f) * size.x, (vp3.y - 0.5f) * size.y);
    }

    // Screen point → local point trong minimapRect
    private bool ScreenPointToMinimapLocalPoint(Vector2 screen, out Vector2 local)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, screen, null, out local);
    }

    // Screen point → viewport (0..1) theo vùng minimapRect
    private bool ScreenPointToViewportOnMinimap(Vector2 screen, out Vector2 viewport01)
    {
        viewport01 = Vector2.zero;
        if (!ScreenPointToMinimapLocalPoint(screen, out Vector2 local)) return false;

        Vector2 size = minimapRect.rect.size;
        // local (-w/2..w/2) -> (0..1)
        viewport01 = new Vector2(local.x / size.x + 0.5f, local.y / size.y + 0.5f);
        // clamp để tránh ra ngoài
        viewport01 = Vector2.Min(Vector2.one, Vector2.Max(Vector2.zero, viewport01));
        return true;
    }
}
