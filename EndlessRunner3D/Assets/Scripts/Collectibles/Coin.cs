using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float bobHeight   = 0.2f;
    [SerializeField] private float bobSpeed    = 2f;

    private Vector3 _startPos;

    private void OnEnable()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        float y = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Award score
        GameManager.Instance.AddScore(10);

        // Disable (pool will recycle it)
        gameObject.SetActive(false);
    }
}