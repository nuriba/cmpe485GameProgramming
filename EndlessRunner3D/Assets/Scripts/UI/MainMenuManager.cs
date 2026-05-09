using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenuManager — handles the main menu scene.
/// Attach to a MainMenu GameObject. Wire buttons in Inspector.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject      settingsPanel;

    private void Start()
    {
        Time.timeScale = 1f;  // safety reset in case we came from a paused state

        int best = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = best > 0 ? $"BEST: {best}" : string.Empty;

        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ─── Button Callbacks ──────────────────────────────────────────────────

    public void OnPlayButton()
    {
        SceneManager.LoadScene("Game");
        // GameManager.StartGame() is called by the Game scene's start button or auto-start
    }

    public void OnSettingsButton()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnBGMVolumeChanged(float value)
    {
        AudioManager.Instance?.SetBGMVolume(value);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance?.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
