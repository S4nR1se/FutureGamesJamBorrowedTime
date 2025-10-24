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
    private ParticleSystem ParticlesSpawned;

    private Zone _zone;
    private Collider _collider;
    private NavMeshSurface _parentSurface;
    private bool _isZoneRegistered; // Track registration state

    private void Start()
    {
        if (_zone == null && !_isZoneRegistered)
        {
            InitializeZone();
        }
    }

    public void InitializeZone()
    {
        if (_isZoneRegistered)
        {
            Debug.LogWarning($"Zone {zoneName} is already registered!", this);
            return;
        }

        ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
        if (zoneManager == null)
        {
            Debug.LogError("ZoneManager not found!", this);
            return;
        }

        _parentSurface = zoneManager.ParentSurface;

        _zone = CreateZone();
        zoneManager.RegisterZone(_zone);
        _isZoneRegistered = true;

        if (zoneType == ZoneType.ConstructionSite)
        {
            ParticlesSpawned = ParticleSystemManager.Instance.Spawn("Construction", transform.position);
        }
        else
        {
            Destroy(ParticlesSpawned);
        }

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
            ZoneType.House => 10,
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

        float calculatedRadius = extents.magnitude;
        float shrinkFactor = 0.8f;
        return calculatedRadius * shrinkFactor;
    }

    public Zone GetZone() => _zone;

    public void UpdateZone()
    {
        if (_zone == null)
        {
            Debug.LogWarning($"No zone to update for {zoneName}!", this);
            return;
        }

        ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
        if (zoneManager == null)
        {
            Debug.LogError("ZoneManager not found!", this);
            return;
        }

        // Unregister the old zone
        if (_isZoneRegistered)
        {
            zoneManager.UnregisterZone(_zone);
            _isZoneRegistered = false;
        }

        // Create and register new zone
        _zone = CreateZone();
        zoneManager.RegisterZone(_zone);
        _isZoneRegistered = true;
    }

    private void OnDestroy()
    {
        if (_isZoneRegistered && _zone != null)
        {
            ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
            if (zoneManager != null)
            {
                zoneManager.UnregisterZone(_zone);
                _isZoneRegistered = false;
            }
        }
    }
}