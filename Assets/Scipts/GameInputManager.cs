using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;

    private int lockCount = 0; // Số panel đang yêu cầu khóa input

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Gọi khi một panel mở cần khóa input và bật trỏ chuột
    /// </summary>
    public void LockInput()
    {
        lockCount++;
        UpdateInputState();
    }

    /// <summary>
    /// Gọi khi một panel đóng, giảm số lượng khóa
    /// </summary>
    public void UnlockInput()
    {
        lockCount = Mathf.Max(0, lockCount - 1);
        UpdateInputState();
    }

    /// <summary>
    /// Cho biết có được phép bắn không
    /// </summary>
    public bool CanShoot()
    {
        return lockCount == 0;
    }

    /// <summary>
    /// Cập nhật trạng thái trỏ chuột dựa vào số panel đang mở
    /// </summary>
    private void UpdateInputState()
    {
        if (lockCount > 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /// <summary>
    /// Reset hoàn toàn input, dùng khi force tắt mọi thứ
    /// </summary>
    public void ForceReset()
    {
        lockCount = 0;
        UpdateInputState();
    }

    /// <summary>
    /// Chỉ lock chuột không ảnh hưởng tới bắn
    /// </summary>
    public void LockCursorOnly()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void UnlockCursorOnly()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
