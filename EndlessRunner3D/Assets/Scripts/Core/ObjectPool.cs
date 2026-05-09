using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Object Pool — eliminates runtime Instantiate/Destroy overhead.
/// Usage: ObjectPool.Instance.GetFromPool("Coin", coinPrefab, transform)
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    // Dictionary of pools keyed by prefab name
    private Dictionary<string, Queue<GameObject>> _pools = new();
    private Dictionary<string, GameObject>        _prefabs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── Pre-warm a pool (call at scene start) ──────────────────────────────
    public void PrewarmPool(string key, GameObject prefab, int count)
    {
        _prefabs[key] = prefab;

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = CreateNew(key, prefab);
            obj.SetActive(false);
            _pools[key].Enqueue(obj);
        }
    }

    // ─── Get an object from the pool ────────────────────────────────────────
    public GameObject GetFromPool(string key, GameObject prefab = null, Transform parent = null)
    {
        if (!_pools.ContainsKey(key))
        {
            _pools[key] = new Queue<GameObject>();
            if (prefab != null) _prefabs[key] = prefab;
        }

        GameObject obj;

        if (_pools[key].Count > 0)
        {
            obj = _pools[key].Dequeue();
        }
        else
        {
            // Pool exhausted — create a new one (auto-expand)
            GameObject pref = prefab ?? (_prefabs.ContainsKey(key) ? _prefabs[key] : null);
            if (pref == null) { Debug.LogError($"[Pool] No prefab for key '{key}'"); return null; }
            obj = CreateNew(key, pref);
        }

        if (parent != null) obj.transform.SetParent(parent);
        obj.SetActive(true);
        return obj;
    }

    // ─── Return an object to the pool ───────────────────────────────────────
    public void ReturnToPool(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);    // re-parent to pool holder

        if (!_pools.ContainsKey(key))
            _pools[key] = new Queue<GameObject>();

        _pools[key].Enqueue(obj);
    }

    // ─── Private helpers ────────────────────────────────────────────────────
    private GameObject CreateNew(string key, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.name = key;

        // Let the object know its pool key via PooledObject component
        PooledObject po = obj.GetComponent<PooledObject>() ?? obj.AddComponent<PooledObject>();
        po.PoolKey = key;
        return obj;
    }
}

/// <summary>
/// Lightweight component on every pooled object.
/// Automatically returns itself to the pool when disabled, if AutoReturn is true.
/// </summary>
public class PooledObject : MonoBehaviour
{
    public string PoolKey;
    public bool   AutoReturn = false;   // set true for timed effects (e.g. particles)

    private void OnDisable()
    {
        if (AutoReturn && ObjectPool.Instance != null)
            ObjectPool.Instance.ReturnToPool(PoolKey, gameObject);
    }
}
