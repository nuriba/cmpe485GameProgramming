using UnityEngine;

/// <summary>
/// CameraFollow — smooth third-person camera that follows the player.
/// Attach to your Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3   offset      = new Vector3(0f, 3f, -6f);
    [SerializeField] private float     smoothSpeed = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired  = target.position + target.rotation * offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.position = smoothed;
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
