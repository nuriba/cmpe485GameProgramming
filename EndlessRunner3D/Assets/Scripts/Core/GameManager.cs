using UnityEngine;

/// <summary>
/// GameManager Singleton — owns game state and broadcasts events.
/// Place on a persistent GameObject in every scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── Singleton ─────────────────────────────────────────────────────────────
    public static GameManager Instance { get; private set; }

    // ─── Game State ────────────────────────────────────────────────────────────
    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;

    // ─── Score & Distance ──────────────────────────────────────────────────────
    public int   Score     { get; private set; }
    public int   HighScore { get; private set; }
    public float Distance  { get; private set; }

    // Tracks the last whole-metre value so distance score only ever adds, never subtracts
    private int _lastDistanceScore = 0;

    // ─── Difficulty ────────────────────────────────────────────────────────────
    [Header("Difficulty")]
    [SerializeField] private float baseSpeed         = 8f;
    [SerializeField] private float maxSpeed          = 28f;
    [SerializeField] private float speedIncreaseRate = 0.05f;   // units/s per second
    public float CurrentSpeed { get; private set; }

    // ─── Delegates & Events ────────────────────────────────────────────────────
    public delegate void GameStateChanged(GameState newState);
    public delegate void ScoreChanged(int newScore);
    public delegate void HighScoreChanged(int newHighScore);
    public delegate void SpeedChanged(float newSpeed);

    public static event GameStateChanged OnGameStateChanged;
    public static event ScoreChanged     OnScoreChanged;
    public static event HighScoreChanged OnHighScoreChanged;
    public static event SpeedChanged     OnSpeedChanged;

    // ─── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        HighScore    = PlayerPrefs.GetInt("HighScore", 0);
        CurrentSpeed = baseSpeed;
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Advance distance
        Distance += CurrentSpeed * Time.deltaTime;

        // Ramp speed (clamped)
        float newSpeed = Mathf.Min(CurrentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
        if (!Mathf.Approximately(newSpeed, CurrentSpeed))
        {
            CurrentSpeed = newSpeed;
            OnSpeedChanged?.Invoke(CurrentSpeed);
        }

        // ── FIX: Distance score only ever increments forward ──────────────────
        // Previously used (newScore - Score) which subtracted coin bonuses.
        // Now we track distance points separately so coins stack on top correctly.
        int newDistScore = Mathf.FloorToInt(Distance);
        if (newDistScore > _lastDistanceScore)
        {
            AddScore(newDistScore - _lastDistanceScore);
            _lastDistanceScore = newDistScore;
        }
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    public void StartGame()
    {
        Score              = 0;
        Distance           = 0f;
        _lastDistanceScore = 0;   // reset so distance score starts fresh
        CurrentSpeed       = baseSpeed;
        OnScoreChanged?.Invoke(Score);
        SetState(GameState.Playing);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        Time.timeScale = 0f;
        SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        Time.timeScale = 1f;
        SetState(GameState.Playing);
    }

    public void TriggerGameOver()
    {
        if (CurrentState == GameState.GameOver) return;

        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt("HighScore", HighScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(HighScore);
        }

        SetState(GameState.GameOver);
    }

    public void AddScore(int amount)
    {
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    // ─── Private Helpers ───────────────────────────────────────────────────────

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }
}