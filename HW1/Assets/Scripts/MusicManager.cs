using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to an empty "MusicManager" GameObject.
/// Drag an AudioSource (set to Play On Awake + Loop) onto this component.
/// Wire the Toggle button's OnClick to ToggleMusic().
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("References")]
    public AudioSource audioSource;

    [Header("UI")]
    public Text   toggleButtonText;    // legacy UI Text
    // -- OR --
    public TMPro.TextMeshProUGUI toggleButtonTMP; // TextMeshPro version

    void Start()
    {
        UpdateLabel();
    }

    /// <summary>Wired to the ON/OFF button's OnClick event.</summary>
    public void ToggleMusic()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
            audioSource.Pause();
        else
            audioSource.Play();

        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (audioSource == null) return;
        string label = audioSource.isPlaying ? "Music: ON" : "Music: OFF";

        if (toggleButtonText    != null) toggleButtonText.text    = label;
        if (toggleButtonTMP     != null) toggleButtonTMP.text     = label;
    }
}
