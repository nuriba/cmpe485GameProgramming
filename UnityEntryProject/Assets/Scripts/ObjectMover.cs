using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public Rigidbody rb;
    public float forceMagnitude = 10f;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.up * forceMagnitude);
    }
}