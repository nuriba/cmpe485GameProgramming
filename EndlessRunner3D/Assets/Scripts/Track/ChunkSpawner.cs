using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ChunkSpawner — procedurally generates the endless track using the Object Pool.
///
/// SETUP:
///   • Create chunk prefabs (flat track sections, ~20 units long each)
///   • Assign chunkPrefabs, obstaclePrefabs, coinPrefab, powerUpPrefab in Inspector
///   • Set chunkLength to match your prefab's Z length
///   • Place this script on a ChunkSpawner GameObject in the scene
/// </summary>
public class ChunkSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] chunkPrefabs;       // track section prefabs (1-3 variants)
    [SerializeField] private GameObject[] obstaclePrefabs;    // obstacle variants
    [SerializeField] private GameObject   coinPrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject magnetPrefab;

    [Header("Track Settings")]
    [SerializeField] private float chunkLength          = 20f;
    [SerializeField] private int   chunksAhead          = 5;   // how many chunks to keep spawned
    [SerializeField] private float laneOffset           = 2.5f;

    [Header("Spawn Rates (0–1)")]
    [SerializeField] private float obstacleChance       = 0.4f;
    [SerializeField] private float coinRowChance        = 0.6f;
    [SerializeField] private float powerUpChance        = 0.05f;

    // Pool keys
    private const string KEY_CHUNK    = "Chunk";
    private const string KEY_OBSTACLE = "Obstacle";
    private const string KEY_COIN     = "Coin";
    private const string KEY_POWERUP  = "PowerUp";

    private Queue<GameObject> _activeChunks = new();
    private float _spawnZ = 0f;    // Z position of next chunk spawn
    private float _playerZ = 0f;   // tracked to know when to spawn more

    private Transform _playerTransform;

    // ─── Unity Lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        // Pre-warm pools
        ObjectPool.Instance.PrewarmPool(KEY_CHUNK,    chunkPrefabs[0],    chunksAhead + 2);
        ObjectPool.Instance.PrewarmPool(KEY_OBSTACLE, obstaclePrefabs[0], 15);
        ObjectPool.Instance.PrewarmPool(KEY_COIN,     coinPrefab,         40);
        ObjectPool.Instance.PrewarmPool(KEY_POWERUP + "_Shield", shieldPrefab, 3);
        ObjectPool.Instance.PrewarmPool(KEY_POWERUP + "_Magnet", magnetPrefab, 3);

        // Spawn initial chunks
        for (int i = 0; i < chunksAhead; i++)
            SpawnChunk();

        // Cache player ref
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

        // Spawn new chunks when player is getting close to the end
        while (_spawnZ < _playerZ + chunksAhead * chunkLength)
            SpawnChunk();

        // Recycle chunks the player has passed
        while (_activeChunks.Count > 0)
        {
            GameObject oldest = _activeChunks.Peek();
            if (oldest.transform.position.z + chunkLength < _playerZ - chunkLength)
            {
                _activeChunks.Dequeue();
                RecycleChunk(oldest);
            }
            else break;
        }
    }

    // ─── Chunk Spawning ─────────────────────────────────────────────────────

    private void SpawnChunk()
    {
        // Pick a random chunk prefab variant
        GameObject prefab = chunkPrefabs[Random.Range(0, chunkPrefabs.Length)];
        GameObject chunk  = ObjectPool.Instance.GetFromPool(KEY_CHUNK, prefab);

        chunk.transform.position = new Vector3(0f, 0f, _spawnZ);
        chunk.transform.rotation = Quaternion.identity;
        _activeChunks.Enqueue(chunk);

        // Populate this chunk (skip the very first chunk so player has room)
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
            int   lane    = Random.Range(-1, 2);                       // -1, 0, 1
            float spawnX  = lane * laneOffset;
            float spawnZ  = chunkStartZ + chunkLength * Random.Range(0.3f, 0.7f);

            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            GameObject obstacle       = ObjectPool.Instance.GetFromPool(KEY_OBSTACLE, obstaclePrefab);
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
                GameObject coin = ObjectPool.Instance.GetFromPool(KEY_COIN, coinPrefab);
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
            string     poolKey   = useShield ? KEY_POWERUP + "_Shield" : KEY_POWERUP + "_Magnet";

            GameObject pu = ObjectPool.Instance.GetFromPool(poolKey, prefab);
            pu.transform.position = new Vector3(lane * laneOffset, 0.5f, spawnZ);
            pu.transform.SetParent(chunk.transform);
        }
    }

    // ─── Chunk Recycling ────────────────────────────────────────────────────

    private void RecycleChunk(GameObject chunk)
    {
        // Return all children (obstacles, coins, power-ups) to their pools
        foreach (Transform child in chunk.transform)
        {
            PooledObject po = child.GetComponent<PooledObject>();
            if (po != null)
                ObjectPool.Instance.ReturnToPool(po.PoolKey, child.gameObject);
            else
                child.gameObject.SetActive(false);
        }

        ObjectPool.Instance.ReturnToPool(KEY_CHUNK, chunk);
    }

    // ─── Game State ─────────────────────────────────────────────────────────

    private void OnGameStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Menu || state == GameManager.GameState.GameOver)
        {
            // Recycle everything on restart
            while (_activeChunks.Count > 0)
                RecycleChunk(_activeChunks.Dequeue());

            _spawnZ = 0f;
        }

        if (state == GameManager.GameState.Playing)
        {
            for (int i = 0; i < chunksAhead; i++)
                SpawnChunk();
        }
    }
}
