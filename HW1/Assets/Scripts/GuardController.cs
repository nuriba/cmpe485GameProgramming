using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to a Guard GameObject (Capsule or imported mesh).
/// NO Rigidbody needed — uses Transform.MoveTowards to avoid wall-sticking.
///
/// Two waypoints (A → B → A → …) are set in the Inspector.
/// If the player comes within killRadius, the player dies.
///
/// The patrol uses a Coroutine as required by the assignment.
/// </summary>
public class GuardController : MonoBehaviour
{
    [Header("Waypoints (world positions)")]
    public Transform waypointA;
    public Transform waypointB;

    [Header("Movement")]
    public float moveSpeed   = 2.5f;
    public float waitAtPoint = 0.4f;  // seconds to pause at each end

    [Header("Kill Detection")]
    public float killRadius  = 1.8f;  // metres — player too close = death
    public float checkInterval = 0.1f; // how often proximity is checked (seconds)

    // ── Internal ──────────────────────────────────────────────────────────────
    private Transform _player;
    private bool      _active = true;

    void Start()
    {
        // Cache player reference
        var ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) _player = ph.transform;

        if (waypointA == null || waypointB == null)
        {
            Debug.LogWarning($"GuardController on {name}: waypoints not set!");
            return;
        }

        // Start both coroutines
        StartCoroutine(PatrolRoutine());
        StartCoroutine(ProximityCheckRoutine());
    }

    // ── Coroutine 1: walk A→B→A forever ─────────────────────────────────────
    private IEnumerator PatrolRoutine()
    {
        Transform current = waypointA;
        Transform next    = waypointB;

        while (_active)
        {
            // Walk toward current target
            while (Vector3.Distance(transform.position, current.position) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    current.position,
                    moveSpeed * Time.deltaTime);

                // Face the direction of travel
                Vector3 dir = (current.position - transform.position);
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir);

                yield return null;  // wait one frame
            }

            // Snap to waypoint and pause briefly
            transform.position = current.position;
            yield return new WaitForSeconds(waitAtPoint);

            // Swap targets
            Transform tmp = current;
            current = next;
            next    = tmp;
        }
    }

    // ── Coroutine 2: periodically check distance to player ───────────────────
    private IEnumerator ProximityCheckRoutine()
    {
        while (_active)
        {
            if (_player != null)
            {
                float dist = Vector3.Distance(transform.position, _player.position);
                if (dist <= killRadius)
                {
                    _active = false;
                    var health = _player.GetComponent<PlayerHealth>();
                    if (health != null) health.Die();
                }
            }
            yield return new WaitForSeconds(checkInterval);
        }
    }

    // ── Draw gizmo so you can see kill radius in Scene view ──────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);
    }
}
