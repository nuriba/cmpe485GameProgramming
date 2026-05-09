using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UIManager — drives all HUD elements during gameplay.
/// Attach to a UIManager GameObject in your Game scene.
/// Wire up the TMP references in the Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Power-Up Indicators")]
    [SerializeField] private GameObject shieldIcon;
    [SerializeField] private GameObject magnetIcon;

    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;

    [Header("Game Over Panel")]
    [SerializeField] private GameObject  gameOverPanel;
    [SerializeField] private TextMeshProUGUI goScoreText;
    [SerializeField] private TextMeshProUGUI goHighScoreText;
    [SerializeField] private TextMeshProUGUI goNewRecordText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnScoreChanged     += UpdateScore;
        GameManager.OnHighScoreChanged += UpdateHighScore;
        GameManager.OnSpeedChanged     += UpdateSpeed;
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged     -= UpdateScore;
        GameManager.OnHighScoreChanged -= UpdateHighScore;
        GameManager.OnSpeedChanged     -= UpdateSpeed;
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        // Initialise displays
        if (highScoreText != null)
            highScoreText.text = $"BEST: {GameManager.Instance?.HighScore ?? 0}";

        SetPanels(false, false);
    }

    private void Update()
    {
        // Distance updates every frame (cheaper than an event per metre)
        if (distanceText != null && GameManager.Instance != null)
            distanceText.text = $"{GameManager.Instance.Distance:F0} m";
    }

    // ─── Event callbacks ───────────────────────────────────────────────────

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = $"SCORE: {score}";
    }

    private void UpdateHighScore(int hs)
    {
        if (highScoreText != null) highScoreText.text = $"BEST: {hs}";
    }

    private void UpdateSpeed(float speed)
    {
        if (speedText != null) speedText.text = $"{speed:F1} m/s";
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Playing:
                SetPanels(false, false);
                break;

            case GameManager.GameState.Paused:
                SetPanels(true, false);
                break;

            case GameManager.GameState.GameOver:
                ShowGameOver();
                break;
        }
    }

    // ─── Power-up HUD ──────────────────────────────────────────────────────

    public void ShowShieldIcon(bool show)
    {
        if (shieldIcon != null) shieldIcon.SetActive(show);
    }

    public void ShowMagnetIcon(bool show)
    {
        if (magnetIcon != null) magnetIcon.SetActive(show);
    }

    // ─── Game Over Panel ───────────────────────────────────────────────────

    private void ShowGameOver()
    {
        SetPanels(false, true);

        int score = GameManager.Instance?.Score ?? 0;
        int best  = GameManager.Instance?.HighScore ?? 0;

        if (goScoreText     != null) goScoreText.text     = $"SCORE: {score}";
        if (goHighScoreText != null) goHighScoreText.text  = $"BEST:  {best}";
        if (goNewRecordText != null) goNewRecordText.gameObject.SetActive(score >= best && score > 0);
    }

    // ─── Button callbacks (hook up in Inspector) ───────────────────────────

    public void OnResumeButton()  => GameManager.Instance?.ResumeGame();
    public void OnRestartButton() => GameManager.Instance?.StartGame();
    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private void SetPanels(bool pause, bool gameOver)
    {
        if (pausePanel    != null) pausePanel.SetActive(pause);
        if (gameOverPanel != null) gameOverPanel.SetActive(gameOver);
    }
}
