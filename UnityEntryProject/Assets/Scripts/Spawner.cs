using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject prefab;
    public Vector3 spawnPosition = new Vector3(0, 5, 0);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }
}