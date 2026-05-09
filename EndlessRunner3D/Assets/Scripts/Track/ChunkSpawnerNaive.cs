using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ChunkSpawnerNaive — IDENTICAL logic to ChunkSpawner but uses raw
/// Instantiate/Destroy instead of the Object Pool.
///
/// PURPOSE: Profiler comparison for Challenge 1 & 2.
///   • Duplicate your Game scene → Game_Naive.unity
///   • On the ChunkSpawner GameObject, swap ChunkSpawner → ChunkSpawnerNaive
///   • Profile 60 seconds of gameplay and compare CPU/Memory with the pooled version.
/// </summary>
public class ChunkSpawnerNaive : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] chunkPrefabs;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private GameObject   coinPrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject magnetPrefab;

    [Header("Track Settings")]
    [SerializeField] private float chunkLength          = 20f;
    [SerializeField] private int   chunksAhead          = 5;
    [SerializeField] private float laneOffset           = 2.5f;

    [Header("Spawn Rates (0–1)")]
    [SerializeField] private float obstacleChance       = 0.4f;
    [SerializeField] private float coinRowChance        = 0.6f;
    [SerializeField] private float powerUpChance        = 0.05f;

    private Queue<GameObject> _activeChunks = new();
    private float _spawnZ  = 0f;
    private float _playerZ = 0f;

    private Transform _playerTransform;

    // ─── Unity Lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        // NO pool pre-warm — objects are created/destroyed on the fly

        // ── Guard: ensure prefab references are assigned in the Inspector ──
        if (chunkPrefabs == null || chunkPrefabs.Length == 0)
        {
            Debug.LogError($"[ChunkSpawnerNaive] chunkPrefabs array is empty! " +
                           $"Assign chunk prefabs in the Inspector on '{gameObject.name}'.", this);
            enabled = false;
            return;
        }
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            Debug.LogWarning($"[ChunkSpawnerNaive] obstaclePrefabs is empty — no obstacles will spawn.", this);
        if (coinPrefab == null)
            Debug.LogWarning($"[ChunkSpawnerNaive] coinPrefab is null — no coins will spawn.", this);
        if (shieldPrefab == null)
            Debug.LogWarning($"[ChunkSpawnerNaive] shieldPrefab is null — no shields will spawn.", this);
        if (magnetPrefab == null)
            Debug.LogWarning($"[ChunkSpawnerNaive] magnetPrefab is null — no magnets will spawn.", this);

        for (int i = 0; i < chunksAhead; i++)
            SpawnChunk();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        GameManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy() => GameManager.OnGameStateChanged -= OnGameStateChanged;

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (_playerTransform == null) return;

        _playerZ = _playerTransform.position.z;

        while (_spawnZ < _playerZ + chunksAhead * chunkLength)
            SpawnChunk();

        while (_activeChunks.Count > 0)
        {
            GameObject oldest = _activeChunks.Peek();
            if (oldest.transform.position.z + chunkLength < _playerZ - chunkLength)
            {
                _activeChunks.Dequeue();
                DestroyChunk(oldest);   // ← Destroy instead of ReturnToPool
            }
            else break;
        }
    }

    // ─── Chunk Spawning (Naive — Instantiate) ───────────────────────────────

    private void SpawnChunk()
    {
        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        GameObject chunk  = Instantiate(prefab);    // ← raw Instantiate

        chunk.transform.position = new Vector3(0f, 0f, _spawnZ);
        chunk.transform.rotation = Quaternion.identity;
        _activeChunks.Enqueue(chunk);

        if (_spawnZ > chunkLength)
            PopulateChunk(chunk);

        _spawnZ += chunkLength;
    }

    private void PopulateChunk(GameObject chunk)
    {
        float chunkStartZ = chunk.transform.position.z;

        // ── Obstacles ───────────────────────────────────────────────────────
        if (Random.value < obstacleChance)
        {
            int   lane    = Random.Range(-1, 2);
            float spawnX  = lane * laneOffset;
            float spawnZ  = chunkStartZ + chunkLength * Random.Range(0.3f, 0.7f);

            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            GameObject obstacle       = Instantiate(obstaclePrefab);     // ← raw Instantiate
            obstacle.transform.position = new Vector3(spawnX, 0f, spawnZ);
            obstacle.transform.SetParent(chunk.transform);
        }

        // ── Coin Row ────────────────────────────────────────────────────────
        if (Random.value < coinRowChance)
        {
            int   lane   = Random.Range(-1, 2);
            float startZ = chunkStartZ + 2f;
            float coinSpacing = 1.5f;
            int   coinCount   = Random.Range(3, 8);

            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = Instantiate(coinPrefab);              // ← raw Instantiate
                coin.transform.position = new Vector3(lane * laneOffset, 0.5f, startZ + i * coinSpacing);
                coin.transform.SetParent(chunk.transform);
            }
        }

        // ── Power-Up ────────────────────────────────────────────────────────
        if (Random.value < powerUpChance)
        {
            int   lane   = Random.Range(-1, 2);
            float spawnZ = chunkStartZ + chunkLength * 0.5f;

            bool       useShield = Random.value < 0.5f;
            GameObject prefab    = useShield ? shieldPrefab : magnetPrefab;

            GameObject pu = Instantiate(prefab);                        // ← raw Instantiate
            pu.transform.position = new Vector3(lane * laneOffset, 0.5f, spawnZ);
            pu.transform.SetParent(chunk.transform);
        }
    }

    // ─── Chunk Destruction (Naive — Destroy) ────────────────────────────────

    private void DestroyChunk(GameObject chunk)
    {
        // Destroy all children (obstacles, coins, power-ups)
        foreach (Transform child in chunk.transform)
        {
            Destroy(child.gameObject);              // ← raw Destroy
        }

        Destroy(chunk);                             // ← raw Destroy
    }

    // ─── Game State ─────────────────────────────────────────────────────────

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Menu || state == GameManager.GameState.GameOver)
        {
            while (_activeChunks.Count > 0)
                DestroyChunk(_activeChunks.Dequeue());   // ← raw Destroy

            _spawnZ = 0f;
        }

        if (state == GameManager.GameState.Playing)
        {
            for (int i = 0; i < chunksAhead; i++)
                SpawnChunk();
        }
    }
}
