using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Spikes child object (where the Renderer is).
/// Uses a proximity coroutine instead of OnCollisionEnter because
/// Starter Assets uses CharacterController, not Rigidbody.
/// </summary>
public class TrapController : MonoBehaviour
{
    [Header("Timing")]
    public float downDuration = 2.5f;
    public float upDuration   = 1.5f;

    [Header("Movement")]
    public float raisedHeight = 1.5f;
    public float moveSpeed    = 6f;

    [Header("Kill Detection")]
    public float killRadius = 1.8f;

    [Header("Visual Feedback")]
    public Renderer trapRenderer;
    public Color safeColor   = Color.green;
    public Color dangerColor = Color.red;

    private bool      _isDangerous = false;
    private Vector3   _downPos;
    private Vector3   _upPos;
    private Transform _player;

    void Start()
    {
        _downPos = transform.position;
        _upPos   = _downPos + Vector3.up * raisedHeight;

        if (trapRenderer == null)
            trapRenderer = GetComponent<Renderer>();

        var ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) _player = ph.transform;

        SetColor(safeColor);
        StartCoroutine(TrapCycle());
        StartCoroutine(ProximityKillCheck());
    }

    void Update()
    {
        Vector3 target = _isDangerous ? _upPos : _downPos;
        transform.position = Vector3.MoveTowards(
            transform.position, target, moveSpeed * Time.deltaTime);
    }

    // Coroutine 1: alternate safe/dangerous phases
    private IEnumerator TrapCycle()
    {
        while (true)
        {
            _isDangerous = false;
            SetColor(safeColor);
            yield return new WaitForSeconds(downDuration);

            _isDangerous = true;
            SetColor(dangerColor);
            yield return new WaitForSeconds(upDuration);
        }
    }

    // Coroutine 2: check player proximity every 0.1s
    private IEnumerator ProximityKillCheck()
    {
        while (true)
        {
            if (_isDangerous && _player != null)
            {
                if (Vector3.Distance(transform.position, _player.position) <= killRadius)
                {
                    var health = _player.GetComponent<PlayerHealth>();
                    if (health != null) health.Die();
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetColor(Color c)
    {
        if (trapRenderer != null)
            trapRenderer.material.color = c;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);
    }
}
