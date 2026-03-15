using UnityEngine;
using StarterAssets;

/// <summary>
/// Attach to GameManager GameObject.
/// Escape toggles cursor so player can click UI buttons.
/// Uses ThirdPersonController.LockCameraPosition to freeze camera while cursor is free.
/// </summary>
public class CursorManager : MonoBehaviour
{
    private ThirdPersonController _tpc;
    private bool _cursorLocked = true;

    void Start()
    {
        _tpc = FindObjectOfType<ThirdPersonController>();
        LockCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_cursorLocked) UnlockCursor();
            else               LockCursor();
        }
    }

    public void LockCursor()
    {
        _cursorLocked    = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        if (_tpc != null) _tpc.LockCameraPosition = false;
    }

    public void UnlockCursor()
    {
        _cursorLocked    = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        if (_tpc != null) _tpc.LockCameraPosition = true;
    }
}
