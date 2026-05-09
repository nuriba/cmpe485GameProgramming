using UnityEngine;

/// <summary>
/// Obstacle — no logic needed beyond the tag.
/// Tag: "Obstacle"
/// This component can optionally add visual feedback on hit.
/// </summary>
public class Obstacle : MonoBehaviour
{
    // Intentionally minimal — collision is handled by PlayerController.
    // Add particle effects or animation triggers here if desired.

    [SerializeField] private GameObject hitVFXPrefab;

    /// <summary>Called by PlayerController when player hits this obstacle.</summary>
    public void OnPlayerHit()
    {
        if (hitVFXPrefab != null)
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
    }
}
