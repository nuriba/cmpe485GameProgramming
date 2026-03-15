using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject hudPanel;
    public GameObject winPanel;
    public GameObject deathPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI hintText;
    public TextMeshProUGUI keyStatusText;

    public bool HasKey { get; private set; } = false;
    private bool _gameOver = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SetPanel(winPanel,   false);
        SetPanel(deathPanel, false);
        SetPanel(hudPanel,   true);
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        UpdateKeyHUD();
        if (hintText != null) hintText.text = "";
    }

    /// <summary>
    /// Called when player first TOUCHES the key (sphere trigger).
    /// Updates HUD immediately. Player still needs to push the key to the door.
    /// </summary>
    public void TouchKey()
    {
        HasKey = true;
        UpdateKeyHUD();
        StartCoroutine(ShowHintRoutine("You found the key! Push it to the door!"));
    }

    /// <summary>Called by DoorController when key reaches door via OnCollisionEnter.</summary>
    public void WinGame()
    {
        if (_gameOver) return;
        _gameOver = true;
        StartCoroutine(ShowEndScreen(winPanel));
    }

    /// <summary>Called by TrapController, GuardController, DeathZone.</summary>
    public void PlayerDied()
    {
        if (_gameOver) return;
        _gameOver = true;
        StartCoroutine(ShowEndScreen(deathPanel));
    }

    public void ShowHint(string message)
    {
        StartCoroutine(ShowHintRoutine(message));
    }

    public void RestartGame()
    {
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator ShowEndScreen(GameObject panel)
    {
        yield return new WaitForSecondsRealtime(0.8f);
        Time.timeScale   = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SetPanel(hudPanel, false);
        SetPanel(panel,    true);
    }

    private IEnumerator ShowHintRoutine(string message)
    {
        if (hintText == null) yield break;
        hintText.text = message;
        yield return new WaitForSeconds(3f);
        hintText.text = "";
    }

    private void UpdateKeyHUD()
    {
        if (keyStatusText != null)
            keyStatusText.text = HasKey ? "KEY: YES" : "KEY: NO";
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
