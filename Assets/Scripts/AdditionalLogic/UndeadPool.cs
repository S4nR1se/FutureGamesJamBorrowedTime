using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class UndeadPool : ObjectPool
{
    [System.Serializable]
    private struct UndeadPrefab
    {
        public UndeadTypes type;
        public GameObject prefab;
    }

    [SerializeField] private List<UndeadPrefab> _undeadPrefabs;
    private const int DEFAULTCAPACITY = 100;
    private const int MAXCAPACITY = 100;
    private const int MAXACTIVE = -1;

    private Dictionary<UndeadTypes, ObjectPool<IPoolable>> _pools;
    private Dictionary<UndeadTypes, int> _activeCounts;

    public override void Initialize()
    {
        _defaultCapacity = DEFAULTCAPACITY;
        _maxCapacity = MAXCAPACITY;
        _maxActiveObjects = MAXACTIVE;

        _pools = new Dictionary<UndeadTypes, ObjectPool<IPoolable>>();
        _activeCounts = new Dictionary<UndeadTypes, int>();

        foreach (var undeadPrefab in _undeadPrefabs)
        {
            _pools[undeadPrefab.type] = new ObjectPool<IPoolable>(
                () => CreatePoolableObject(undeadPrefab.prefab, undeadPrefab.type),
                poolObject => OnGetFromPool(poolObject, undeadPrefab.type),
                poolObject => OnReturnToPool(poolObject, undeadPrefab.type),
                poolObject => OnDestroyObject(poolObject),
                true,
                _defaultCapacity,
                _maxCapacity
            );
            _activeCounts[undeadPrefab.type] = 0;
        }
    }

    public IPoolable Get(UndeadTypes type)
    {
        if (_maxActiveObjects > 0 && _activeCounts[type] >= _maxActiveObjects)
        {
            return null;
        }
        return _pools[type].Get();
    }

    public void Release(IPoolable poolObject, UndeadTypes type)
    {
        _pools[type].Release(poolObject);
    }

    private IPoolable CreatePoolableObject(GameObject prefab, UndeadTypes type)
    {
        GameObject poolableObject = Instantiate(prefab, _poolPosition, Quaternion.identity);
        poolableObject.transform.SetParent(transform);
        var poolable = poolableObject.GetComponent<IPoolable>();
        return poolable;
    }

    protected override void OnGetFromPool(IPoolable poolObject)
    {
    }

    private void OnGetFromPool(IPoolable poolObject, UndeadTypes type)
    {
        _activeCounts[type]++;
        poolObject.PoolableComponent.SetActive(true);
    }
    private void OnReturnToPool(IPoolable poolObject, UndeadTypes type)
    {
        _activeCounts[type]--;
        poolObject.PoolableComponent.SetActive(false);
    }
    private void OnDestroyObject(IPoolable poolObject)
    {
        if (poolObject != null && poolObject.PoolableComponent != null)
        {
            Destroy(poolObject.PoolableComponent);
        }
    }
}

public enum UndeadTypes
{
    Skeleton,
    Zombie
}