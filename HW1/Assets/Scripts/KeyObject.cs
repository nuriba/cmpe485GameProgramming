using UnityEngine;

/// <summary>
/// Attach to the Key cube.
/// - Rigidbody (Is Kinematic = OFF, Use Gravity = ON, Mass = 0.5)
/// - BoxCollider (Is Trigger = FALSE) for physics push + OnCollisionEnter with door
/// - SphereCollider (Is Trigger = TRUE) added by this script for HUD update on first touch
/// </summary>
public class KeyObject : MonoBehaviour
{
    [Header("Visual")]
    public float pulseSpeed  = 2f;
    public float pulseAmount = 0.15f;

    [Header("Touch Detection")]
    public float touchRadius = 1.5f; // how close player needs to be to register "touched"

    private Vector3 _baseScale;
    private bool    _touched = false;

    void Start()
    {
        _baseScale = transform.localScale;

        // Tune Rigidbody for easy pushing
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass            = 0.3f;   // very light
            rb.drag            = 2f;     // stops quickly so it doesn't slide too far
            rb.angularDrag     = 5f;     // prevents rolling chaos
            rb.isKinematic     = false;
            rb.useGravity      = true;
            rb.constraints     = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ; // no tipping over
        }

        // Main BoxCollider must NOT be trigger (for OnCollisionEnter with door)
        var box = GetComponent<BoxCollider>();
        if (box != null) box.isTrigger = false;

        // Add a sphere trigger for "first touch" HUD update
        var sphere          = gameObject.AddComponent<SphereCollider>();
        sphere.isTrigger    = true;
        sphere.radius       = touchRadius;
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = _baseScale * pulse;
            transform.Rotate(0f, 45f * Time.deltaTime, 0f);
        }
    }

    // Sphere trigger — fires when player enters the larger radius
    void OnTriggerEnter(Collider other)
    {
        if (_touched) return;
        if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null)
        {
            _touched = true;
            // Update HUD to KEY: YES as soon as player touches the key
            if (GameManager.Instance != null)
                GameManager.Instance.TouchKey();
        }
    }
}
