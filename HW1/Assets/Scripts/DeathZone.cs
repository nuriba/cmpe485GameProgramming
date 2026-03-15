using UnityEngine;

/// <summary>
/// Place an empty GameObject with a Box Collider (Is Trigger = TRUE)
/// far below the maze floor to catch players who fall off the edge.
///
/// Scale it large enough to cover the whole maze footprint.
/// </summary>
public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var health = other.GetComponent<PlayerHealth>();
        if (health != null) health.Die();
    }
}
