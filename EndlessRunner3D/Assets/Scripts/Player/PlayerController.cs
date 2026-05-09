using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// PlayerController v4 — adds post-shield grace period to prevent
/// multi-frame OnControllerColliderHit from killing the player
/// immediately after the shield absorbs a hit.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ─── FSM ───────────────────────────────────────────────────────────────
    public enum PlayerState { Running, Jumping, Sliding, Dead }
    public PlayerState CurrentState { get; private set; } = PlayerState.Running;

    // ─── Inspector ─────────────────────────────────────────────────────────
    [Header("Lane Settings")]
    [SerializeField] private float laneOffset      = 2.5f;
    [SerializeField] private float laneSwitchSpeed = 12f;

    [Header("Jump")]
    [SerializeField] private float jumpForce       = 10f;
    [SerializeField] private float gravity         = -25f;

    [Header("Slide")]
    [SerializeField] private float slideDuration   = 0.8f;
    [SerializeField] private float slideCCHeight   = 0.6f;
    [SerializeField] private float slideCCCenter   = 0.3f;

    [Header("Shield VFX")]
    [SerializeField] private GameObject shieldVisual;   // optional child sphere

    [Header("Animator Parameters")]
    [SerializeField] private string animRunning = "IsRunning";
    [SerializeField] private string animJump    = "Jump";
    [SerializeField] private string animSlide   = "Slide";
    [SerializeField] private string animDead    = "Dead";

    // ─── Private ───────────────────────────────────────────────────────────
    private CharacterController _cc;
    private Animator            _animator;

    private int   _targetLane  = 0;
    private float _targetX     = 0f;
    private float _verticalVel = 0f;

    private float     _normalCCHeight;
    private float     _normalCCCenter;
    private Coroutine _slideCoroutine;

    // Power-up state
    private bool      _shieldActive    = false;
    private bool      _magnetActive    = false;
    private float     _magnetRadius    = 5f;
    private float     _postShieldGrace = 0f;   // ── NEW: invincibility window after absorb
    private Coroutine _shieldCoroutine;
    private Coroutine _magnetCoroutine;

    private PlayerInputActions _inputActions;

    // ─── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _cc             = GetComponent<CharacterController>();
        _animator       = GetComponent<Animator>();
        _normalCCHeight = _cc.height;
        _normalCCCenter = _cc.center.y;
        _targetX        = transform.position.x;

        _inputActions = new PlayerInputActions();
        _inputActions.Player.SwipeLeft .performed += _ => TryLaneSwitch(-1);
        _inputActions.Player.SwipeRight.performed += _ => TryLaneSwitch( 1);
        _inputActions.Player.Jump      .performed += _ => TryJump();
        _inputActions.Player.Slide     .performed += _ => TrySlide();

        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        _inputActions.Disable();
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void Update()
    {
        // Tick the shield grace timer every frame regardless of state
        if (_postShieldGrace > 0f) _postShieldGrace -= Time.deltaTime;

        if (CurrentState == PlayerState.Dead) return;
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Playing) return;

        float speed = GameManager.Instance.CurrentSpeed;

        // Gravity
        if (_cc.isGrounded && _verticalVel < 0f)
        {
            _verticalVel = -2f;
            if (CurrentState == PlayerState.Jumping)
                TransitionTo(PlayerState.Running);
        }
        else
        {
            _verticalVel += gravity * Time.deltaTime;
        }

        // Lateral movement
        _targetX = _targetLane * laneOffset;
        float newX = Mathf.MoveTowards(transform.position.x, _targetX, laneSwitchSpeed * Time.deltaTime);

        Vector3 move = new Vector3(
            newX - transform.position.x,
            _verticalVel * Time.deltaTime,
            speed * Time.deltaTime
        );
        _cc.Move(move);

        // Magnet — pull nearby coins
        if (_magnetActive)
            PullCoins();
    }

    // ─── Input ─────────────────────────────────────────────────────────────

    private void TryLaneSwitch(int dir)
    {
        if (CurrentState == PlayerState.Dead) return;
        int next = Mathf.Clamp(_targetLane + dir, -1, 1);
        if (next != _targetLane) _targetLane = next;
    }

    private void TryJump()
    {
        if (CurrentState == PlayerState.Dead) return;
        if (CurrentState == PlayerState.Sliding) StopSlideEarly();
        if (!_cc.isGrounded) return;
        _verticalVel = jumpForce;
        TransitionTo(PlayerState.Jumping);
    }

    private void TrySlide()
    {
        if (CurrentState == PlayerState.Dead || CurrentState == PlayerState.Sliding) return;
        if (CurrentState == PlayerState.Jumping) { _verticalVel = -jumpForce * 1.5f; return; }
        _slideCoroutine = StartCoroutine(SlideRoutine());
    }

    // ─── Slide ─────────────────────────────────────────────────────────────

    private IEnumerator SlideRoutine()
    {
        TransitionTo(PlayerState.Sliding);
        _cc.height = slideCCHeight;
        _cc.center = new Vector3(_cc.center.x, slideCCCenter, _cc.center.z);
        yield return new WaitForSeconds(slideDuration);
        RestoreCollider();
        TransitionTo(PlayerState.Running);
        _slideCoroutine = null;
    }

    private void StopSlideEarly()
    {
        if (_slideCoroutine != null) { StopCoroutine(_slideCoroutine); _slideCoroutine = null; }
        RestoreCollider();
    }

    private void RestoreCollider()
    {
        _cc.height = _normalCCHeight;
        _cc.center = new Vector3(_cc.center.x, _normalCCCenter, _cc.center.z);
    }

    // ─── Power-Ups ─────────────────────────────────────────────────────────

    public void ActivateShield(float duration)
    {
        if (_shieldCoroutine != null) StopCoroutine(_shieldCoroutine);
        _shieldCoroutine = StartCoroutine(ShieldRoutine(duration));
    }

    private IEnumerator ShieldRoutine(float duration)
    {
        _shieldActive = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);
        UIManager.Instance?.ShowShieldIcon(true);

        yield return new WaitForSeconds(duration);

        _shieldActive = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
        UIManager.Instance?.ShowShieldIcon(false);
        _shieldCoroutine = null;
    }

    public void ActivateMagnet(float duration, float radius)
    {
        _magnetRadius = radius;
        if (_magnetCoroutine != null) StopCoroutine(_magnetCoroutine);
        _magnetCoroutine = StartCoroutine(MagnetRoutine(duration));
    }

    private IEnumerator MagnetRoutine(float duration)
    {
        _magnetActive = true;
        UIManager.Instance?.ShowMagnetIcon(true);

        yield return new WaitForSeconds(duration);

        _magnetActive = false;
        UIManager.Instance?.ShowMagnetIcon(false);
        _magnetCoroutine = null;
    }

    private void PullCoins()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, _magnetRadius);
        foreach (Collider col in nearby)
        {
            if (!col.CompareTag("Coin")) continue;
            col.transform.position = Vector3.MoveTowards(
                col.transform.position,
                transform.position,
                15f * Time.deltaTime);
        }
    }

    // ─── Collision ─────────────────────────────────────────────────────────

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (CurrentState == PlayerState.Dead) return;

        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                if (_shieldActive)
                {
                    // Shield absorbs one hit — cancel it and start grace period
                    _shieldActive    = false;
                    _postShieldGrace = 0.5f;   // half-second invincibility
                    if (shieldVisual != null) shieldVisual.SetActive(false);
                    UIManager.Instance?.ShowShieldIcon(false);
                    if (_shieldCoroutine != null) { StopCoroutine(_shieldCoroutine); _shieldCoroutine = null; }
                }
                else if (_postShieldGrace <= 0f)
                {
                    Die();
                }
                break;

            case "Coin":
                hit.gameObject.SetActive(false);
                GameManager.Instance?.AddScore(10);
                AudioManager.Instance?.PlaySFX("Coin");
                break;

            case "PowerUp":
                hit.gameObject.GetComponent<PowerUp>()?.Activate(this);
                hit.gameObject.SetActive(false);
                AudioManager.Instance?.PlaySFX("PowerUp");
                break;
        }
    }

    // ─── Death ─────────────────────────────────────────────────────────────

    public void Die()
    {
        if (CurrentState == PlayerState.Dead) return;
        StopSlideEarly();
        TransitionTo(PlayerState.Dead);

        // Hide any active power-up icons
        UIManager.Instance?.ShowShieldIcon(false);
        UIManager.Instance?.ShowMagnetIcon(false);

        AudioManager.Instance?.PlaySFX("Death");
        GameManager.Instance?.TriggerGameOver();
    }

    // ─── FSM ───────────────────────────────────────────────────────────────

    private void TransitionTo(PlayerState newState)
    {
        if (CurrentState == newState) return;
        CurrentState = newState;
        if (_animator == null) return;

        _animator.ResetTrigger(animJump);
        _animator.ResetTrigger(animSlide);
        _animator.ResetTrigger(animDead);

        switch (newState)
        {
            case PlayerState.Running: _animator.SetBool(animRunning, true);                             break;
            case PlayerState.Jumping: _animator.SetBool(animRunning, false); _animator.SetTrigger(animJump);  break;
            case PlayerState.Sliding: _animator.SetTrigger(animSlide);                                  break;
            case PlayerState.Dead:    _animator.SetBool(animRunning, false); _animator.SetTrigger(animDead);  break;
        }
    }

    private void HandleGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.GameOver && CurrentState != PlayerState.Dead)
            Die();

        if (state == GameManager.GameState.Playing)
        {
            CurrentState     = PlayerState.Running;
            _targetLane      = 0;
            _verticalVel     = 0f;
            _shieldActive    = false;
            _magnetActive    = false;
            _postShieldGrace = 0f;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            UIManager.Instance?.ShowShieldIcon(false);
            UIManager.Instance?.ShowMagnetIcon(false);
            RestoreCollider();
            TransitionTo(PlayerState.Running);
        }
    }
}