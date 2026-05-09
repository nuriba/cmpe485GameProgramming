using UnityEngine;

public class ShieldPowerUp : PowerUp
{
    [SerializeField] private float duration = 5f;

    public override void Activate(PlayerController player)
    {
        player.ActivateShield(duration);
    }
}