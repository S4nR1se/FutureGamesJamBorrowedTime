using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneMarker : MonoBehaviour
{
    [Header("Zone Configuration")]
    [SerializeField] private string zoneName = "New Zone";
    [SerializeField] private ZoneType zoneType = ZoneType.House;

    private bool _autoCalculateRadius = true;
    private const float MANUALRADIUS = 10f;

    private Zone _zone;
    private Collider _collider;

    private NavMeshSurface _parentSurface;

    private void Start()
    {
        if (_zone == null)
        {
            InitializeZone();
        }
    }

    public void InitializeZone()
    {
        ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
        _parentSurface = zoneManager.ParentSurface;

        _zone = CreateZone();
        zoneManager.RegisterZone(_zone);

        Building associatedBuilding = GetComponent<Building>();
        if (associatedBuilding != null)
        {
            associatedBuilding.Initialize();
        }
    }

    public Zone CreateZone()
    {
        Vector3 center = transform.position;
        float radius = CalculateRadius();

        int capacity = GetCapacityForType(zoneType);
        return new Zone(zoneName, zoneType, center, radius, _parentSurface, capacity);
    }

    private int GetCapacityForType(ZoneType type)
    {
        return type switch
        {
            ZoneType.House => 1,
            ZoneType.Farm => 5,
            ZoneType.Church => 10,
            ZoneType.Workshop => 5,
            ZoneType.ConstructionSite => -1,
            ZoneType.Graveyard => -1,
            _ => -1 // Default unlimited
        };
    }

    private float CalculateRadius()
    {
        if (!_autoCalculateRadius)
            return MANUALRADIUS;

        if (_collider == null)
            _collider = GetComponent<Collider>();

        Bounds bounds = _collider.bounds;

        Vector2 extents = new Vector2(bounds.extents.x, bounds.extents.z);
        return extents.magnitude;
    }

    public Zone GetZone() => _zone;

    public void UpdateZone()
    {
        if (_zone != null)
        {
            ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
            zoneManager.UnregisterZone(_zone);

            _zone = CreateZone();
            zoneManager.RegisterZone(_zone);
        }
    }
}