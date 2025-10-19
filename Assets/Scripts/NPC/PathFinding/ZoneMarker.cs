using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneMarker : MonoBehaviour
{
    [Header("Zone Configuration")]
    [SerializeField] private string zoneName = "New Zone";
    [SerializeField] private ZoneType zoneType = ZoneType.House;
    [SerializeField] private NavMeshSurface parentSurface;
    [SerializeField] private int capacity = -1; // -1 = unlimited

    [Header("Zone Size")]
    [SerializeField] private bool autoCalculateRadius = true;
    [SerializeField] private float manualRadius = 10f;

    private Zone _zone;
    private Collider _collider;
    private HashSet<NPC> _npcsInZone = new HashSet<NPC>();

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (_zone == null)
        {
            _zone = CreateZone();

            ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
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

        _zone = new Zone(
            zoneName,
            zoneType,
            center,
            radius,
            parentSurface,
            capacity
        );

        return _zone;
    }

    private float CalculateRadius()
    {
        if (!autoCalculateRadius)
        {
            return manualRadius;
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
        return manualRadius;
    }
    private void OnTriggerEnter(Collider other)
    {
        NPC npc = other.GetComponent<NPC>();
        if (npc != null && _zone != null)
        {
            if (_npcsInZone.Add(npc))
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
           if (_npcsInZone.Remove(npc))
           {
               npc.ClearCurrentZone();
               Debug.Log($"NPC {npc.name} exited zone {zoneName}");
           }
       }
    }
    public Zone GetZone() => _zone;
    public void UpdateZone()
    {
        if (_zone != null)
        {
            _zone = CreateZone();
        }
    }
}