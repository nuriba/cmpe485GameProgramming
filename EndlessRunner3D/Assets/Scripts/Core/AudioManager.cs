using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioManager — Singleton for BGM and SFX.
/// Add AudioClip references in Inspector using the SFXEntry list.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip   bgmClip;

    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private List<SFXEntry> sfxEntries;

    private Dictionary<string, AudioClip> _sfxMap = new();

    [System.Serializable]
    public struct SFXEntry
    {
        public string    key;    // e.g. "Coin", "Death", "PowerUp", "Jump"
        public AudioClip clip;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var entry in sfxEntries)
            _sfxMap[entry.key] = entry.clip;
    }

    private void Start()
    {
        PlayBGM();
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDestroy() => GameManager.OnGameStateChanged -= HandleStateChanged;

    // ─── Public API ────────────────────────────────────────────────────────

    public void PlaySFX(string key)
    {
        if (_sfxMap.TryGetValue(key, out AudioClip clip))
            sfxSource.PlayOneShot(clip);
        else
            Debug.LogWarning($"[Audio] SFX key '{key}' not found.");
    }

    public void PlayBGM()
    {
        if (bgmClip == null) return;
        bgmSource.clip   = bgmClip;
        bgmSource.loop   = true;
        bgmSource.Play();
    }

    public void SetBGMVolume(float v) => bgmSource.volume = v;
    public void SetSFXVolume(float v) => sfxSource.volume = v;

    // ─── State-driven audio ────────────────────────────────────────────────

    private void HandleStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Playing:
                if (!bgmSource.isPlaying) PlayBGM();
                break;
            case GameManager.GameState.GameOver:
                PlaySFX("Death");
                break;
            case GameManager.GameState.Paused:
                bgmSource.Pause();
                break;
        }
    }
}
