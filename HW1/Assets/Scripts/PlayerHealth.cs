using UnityEngine;
using StarterAssets; // Starter Assets namespace

/// <summary>
/// Attach this to the same GameObject as ThirdPersonController.
/// It acts as the bridge between game hazards and GameManager.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // Only one public method - everything else is driven externally.

    private bool _isDead = false;

    /// <summary>
    /// Called by TrapController, GuardController, DeathZone.
    /// Disables player input and notifies GameManager.
    /// </summary>
    public void Die()
    {
        if (_isDead) return;
        _isDead = true;

        // Disable Starter Assets input & controller so the player stops moving
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null) controller.enabled = false;

        var input = GetComponent<StarterAssetsInputs>();
        if (input != null) input.enabled = false;

        // Tell the game manager
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }
}
