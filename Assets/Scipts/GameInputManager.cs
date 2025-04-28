using UnityEngine;

public class GameInputManager : MonoBehaviour
{
    public static GameInputManager Instance;
    private bool inputLocked = false;

    private void Awake()
    {
        Instance = this;
    }

    public void LockInput()
    {
        inputLocked = true;
    }

    public void UnlockInput()
    {
        inputLocked = false;
    }

    public bool CanShoot()
    {
        return !inputLocked;
    }
}

