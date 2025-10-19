using UnityEngine;

public class PeasantPool : ObjectPool
{
    [SerializeField] private GameObject _poolPrefab;
    private const int DEFAULTCAPACITY = 100;
    private const int MAXCAPACITY = 100;
    private const int MAXACTIVE = -1;

    public override void Initialize()
    {
        _prefab = _poolPrefab;
        _defaultCapacity = DEFAULTCAPACITY;
        _maxCapacity = MAXCAPACITY;
        _maxActiveObjects = MAXACTIVE;

        base.Initialize();
    }
}
