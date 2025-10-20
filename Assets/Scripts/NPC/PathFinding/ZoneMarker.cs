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
            ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
            _parentSurface = zoneManager.ParentSurface;

            _zone = CreateZone();

            if (zoneManager != null)
            {
                zoneManager.RegisterZone(_zone);
            }

            Building associatedBuilding = GetComponent<Building>();
            if(associatedBuilding != null)
            {
                associatedBuilding.Initialize();
            }
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
        {
            return MANUALRADIUS;
        }

        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }

        if (_collider is SphereCollider sphere)
        {
            return sphere.radius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if (_collider is BoxCollider box)
        {
            Vector3 size = box.size;
            Vector3 scale = transform.localScale;
            float maxExtent = Mathf.Max(size.x * scale.x, size.z * scale.z) / 2f;
            return maxExtent;
        }
        else if (_collider is CapsuleCollider capsule)
        {
            return capsule.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        }
        return MANUALRADIUS;
    }

    private void OnTriggerEnter(Collider other)
    {
        NPC npc = other.GetComponent<NPC>();
        if (npc != null && _zone != null)
        {
            if (_zone.TryEnter(npc))
            {
                npc.SetCurrentZone(_zone);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NPC npc = other.GetComponent<NPC>();
        if (npc != null && _zone != null)
        {
            _zone.Exit(npc);
            npc.ClearCurrentZone();
        }
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