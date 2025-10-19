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

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

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
        }
    }

    public Zone CreateZone()
    {
        Vector3 center = transform.position;
        float radius = CalculateRadius();

        Zone zone = null;
        switch (zoneType)
        {
            case ZoneType.House:
                zone = new HouseZone(zoneName, center, radius, _parentSurface, 10);
                break;
            case ZoneType.Farm:
                zone = new FarmZone(zoneName, center, radius, _parentSurface, 5);
                break;
            case ZoneType.Graveyard:
                zone = new GraveYardZone(zoneName, center, radius, _parentSurface, -1);
                break;
            case ZoneType.Workshop:
                zone = new WorkshopZone(zoneName, center, radius, _parentSurface, 5);
                break;
            case ZoneType.ConstructionSite:
                zone = new ConstructionZone(zoneName, center, radius, _parentSurface, -1);
                break;
            case ZoneType.Church:
                zone = new ChurchZone(zoneName, center, radius, _parentSurface, 10);
                break;
            default:
                zone = new RoadZone(zoneName, center, radius, _parentSurface, -1);
                break;
        }

        return zone;
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
                Debug.Log($"NPC {npc.name} entered zone {zoneName}");
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
            Debug.Log($"NPC {npc.name} exited zone {zoneName}");
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