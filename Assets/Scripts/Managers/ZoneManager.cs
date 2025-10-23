using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class ZoneManager : Manager
{
    public NavMeshSurface ParentSurface {  get; private set; }

    private Dictionary<ZoneType, List<Zone>> _zonesByType = new();
    private List<Zone> _allZones = new();
    public override void Initialize()
    {
        if (ParentSurface == null)
        {
            ParentSurface = FindFirstObjectByType<NavMeshSurface>();
        }

        foreach (ZoneType type in System.Enum.GetValues(typeof(ZoneType)))
        {
            _zonesByType[type] = new List<Zone>();
        }

        ZoneMarker[] markers = FindObjectsByType<ZoneMarker>(FindObjectsSortMode.None);
        foreach (ZoneMarker marker in markers)
        {
            Zone zone = marker.CreateZone();
            RegisterZone(zone);
        }
    }
    public void RegisterZone(Zone zone)
    {
        if (zone == null)
        {
            return;
        }

        if (!_allZones.Contains(zone))
        {
            _allZones.Add(zone);
            _zonesByType[zone.Type].Add(zone);
        }
    }
    public void UnregisterZone(Zone zone)
    {
        if (zone == null) return;

        List<NPC> npcs = zone.GetNPCsInZone().ToList();
        foreach (NPC npc in npcs)
        {
            npc.ResetOccupiedZone();
        }

        if (_allZones.Remove(zone))
        {
            _zonesByType[zone.Type].Remove(zone);
        }
    }
    public List<Zone> GetZonesOfType(ZoneType zoneType)
    {
        if (_zonesByType.ContainsKey(zoneType))
        {
            return new List<Zone>(_zonesByType[zoneType]);
        }
        return new List<Zone>();
    }
    public Zone GetRandomZone(ZoneType zoneType)
    {
        List<Zone> zones = GetZonesOfType(zoneType);
        if (zones.Count == 0)
        {
            return null;
        }
        return zones[Random.Range(0, zones.Count)];
    }

    public Zone GetRandomAvailableZone(ZoneType zoneType)
    {
        List<Zone> zones = GetZonesOfType(zoneType);
        List<Zone> availableZones = zones.Where(z => !z.IsFull()).ToList();

        if (availableZones.Count == 0)
        {
            return null;
        }
        return availableZones[Random.Range(0, availableZones.Count)];
    }
    public Zone FindZoneAtPosition(Vector3 position)
    {
        foreach (Zone zone in _allZones)
        {
            if (zone.ContainsPosition(position))
            {
                return zone;
            }
        }
        return null;
    }

    public List<Zone> GetAllZones()
    {
        return new List<Zone>(_allZones);
    }

    public Zone GetClosestZone(Vector3 position, ZoneType? typeFilter = null)
    {
        List<Zone> zonesToSearch = typeFilter.HasValue
            ? GetZonesOfType(typeFilter.Value)
            : _allZones;

        if (zonesToSearch.Count == 0) return null;

        Zone closest = zonesToSearch[0];
        float closestDist = Vector3.Distance(position, closest.Center);

        foreach (Zone zone in zonesToSearch)
        {
            float dist = Vector3.Distance(position, zone.Center);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = zone;
            }
        }

        return closest;
    }
}
