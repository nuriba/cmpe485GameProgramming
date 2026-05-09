using UnityEngine;

/// <summary>
/// Base class for all power-ups. Attach a derived class to each power-up prefab.
/// PlayerController calls Activate() on contact.
/// </summary>
public abstract class PowerUp : MonoBehaviour
{
    /// <summary>Override in MagnetPowerUp, ShieldPowerUp, etc.</summary>
    public abstract void Activate(PlayerController player);
}

// ─── Example: ShieldPowerUp ────────────────────────────────────────────────
// public class ShieldPowerUp : PowerUp
// {
//     public override void Activate(PlayerController player)
//     {
//         player.ActivateShield(5f);   // 5 seconds of invincibility
//     }
// }
