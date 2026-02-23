using UnityEngine;

public class ScalePulse : MonoBehaviour
{
    public float speed = 1f;
    public float minScale = 1f;
    public float maxScale = 2f;

    void Update()
    {
        float scale = Mathf.Lerp(minScale, maxScale, (Mathf.Sin(Time.time * speed) + 1) / 2);
        transform.localScale = new Vector3(scale, scale, scale);
    }
}