using UnityEngine.Pool;
using UnityEngine;

public abstract class ObjectPool : MonoBehaviour
{
    private ObjectPool<IPoolable> _pool;
    protected GameObject _prefab;

    protected int _defaultCapacity;
    protected int _maxCapacity;
    protected int _maxActiveObjects = -1; // -1 means unlimited

    public int ActiveCount => _activeCount;
    private int _activeCount = 0;

    protected Vector3 _poolPosition = new Vector3(10000, 10000, 0);

    public virtual void Initialize()
    {
        _pool = new ObjectPool<IPoolable>(
            CreatePoolableObject,
            OnGetFromPool,
            OnReturnToPool,
            OnDestroyObject,
            true,
            _defaultCapacity,
            _maxCapacity
        );
    }
    public IPoolable Get()
    {
        if (_maxActiveObjects > 0 && _activeCount >= _maxActiveObjects)
        {
            return null;
        }

        return _pool.Get();
    }
    public IPoolable CreatePoolableObject()
    {
        GameObject poolableObject = Instantiate(_prefab, _poolPosition, Quaternion.identity);
        poolableObject.transform.SetParent(transform);
        return poolableObject.GetComponent<IPoolable>();
    }
    public void Release(IPoolable poolObject)
    {
        _pool.Release(poolObject);
    }
    protected virtual void OnGetFromPool(IPoolable poolObject)
    {
        _activeCount++;
        poolObject.PoolableComponent.SetActive(true);
    }
    private void OnReturnToPool(IPoolable poolObject)
    {
        _activeCount--;
        poolObject.PoolableComponent.SetActive(false);
        poolObject.PoolableComponent.transform.position = _poolPosition;
    }
    private void OnDestroyObject(IPoolable poolObject)
    {
        if (poolObject != null && poolObject.PoolableComponent != null) Destroy(poolObject.PoolableComponent);
    }
}
public interface IPoolable
{
    public GameObject PoolableComponent { get; }
    public void ReturnToPool(ObjectPool pool);
}