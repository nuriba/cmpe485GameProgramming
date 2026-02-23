using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float forceAmount = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, 0f, moveZ);
        rb.AddForce(movement * forceAmount);
    }
}