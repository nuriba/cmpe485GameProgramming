using UnityEngine;

public class MagnetPowerUp : PowerUp
{
    [SerializeField] private float duration = 8f;
    [SerializeField] private float radius   = 5f;

    public override void Activate(PlayerController player)
    {
        player.ActivateMagnet(duration, radius);
    }
}