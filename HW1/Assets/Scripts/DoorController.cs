using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Door GameObject.
/// BoxCollider Is Trigger = FALSE so OnCollisionEnter fires
/// when the Key Rigidbody physically hits the door.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Settings")]
    public float openAngle    = 90f;
    public float openDuration = 0.6f;

    private bool _opened = false;

    // ── Required by assignment: OnCollisionEnter ─────────────────────────────
    void OnCollisionEnter(Collision collision)
    {
        if (_opened) return;

        if (collision.gameObject.CompareTag("Key"))
        {
            _opened = true;
            StartCoroutine(OpenDoor(collision.gameObject));
        }
    }

    private IEnumerator OpenDoor(GameObject key)
    {
        // Hide the key
        if (key != null) key.SetActive(false);

        // Animate door opening
        Quaternion startRot = transform.rotation;
        Quaternion endRot   = startRot * Quaternion.Euler(0f, openAngle, 0f);

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot,
                                                  elapsed / openDuration);
            yield return null;
        }
        transform.rotation = endRot;

        // Trigger win
        if (GameManager.Instance != null)
            GameManager.Instance.WinGame();
        else
            Debug.LogWarning("DoorController: GameManager.Instance is null!");
    }
}
