using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class Zone
{
    public NavMeshSurface ParentSurface { get; private set; }
    public string Name { get; private set; }
    public ZoneType Type { get; private set; }
    public Vector3 Center { get; private set; }
    public int Capacity { get; private set; }
    public int CurrentOccupancy { get; private set; }
    public float Radius { get; private set; }
    private HashSet<NPC> _npcsInZone = new HashSet<NPC>();

    // Events for entry/exit (buildings can subscribe)
    public event Action<NPC> NPCEntered;
    public event Action<NPC> NPCExited;

    public Zone(string name, ZoneType type, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
    {
        Name = name;
        Type = type;
        Center = center;
        Radius = radius;
        ParentSurface = parentSurface;
        Capacity = capacity; // -1 means unlimited
        CurrentOccupancy = 0;
    }

    public bool ContainsPosition(Vector3 position)
    {
        return Vector3.Distance(position, Center) <= Radius;
    }

    public bool IsFull()
    {
        if (Capacity < 0) return false;
        return CurrentOccupancy >= Capacity;
    }

    public bool TryEnter(NPC npc)
    {
        if (npc == null)
        {
            Debug.LogWarning($"[Zone] {Name} cannot register null NPC");
            return false;
        }

        if (IsFull())
        {
            Debug.LogWarning($"[Zone] {Name} is full (Occupancy: {CurrentOccupancy}/{Capacity})");
            return false;
        }

        if (_npcsInZone.Add(npc))
        {
            CurrentOccupancy++;
            NPCEntered?.Invoke(npc);
            Debug.Log($"[Zone] {Name} registered NPC {npc.Name}, Occupancy: {CurrentOccupancy}/{Capacity}");
            ValidateState();
            return true;
        }

        Debug.LogWarning($"[Zone] {Name} failed to register NPC {npc.Name}: already in zone");
        return false;
    }

    public void Exit(NPC npc)
    {
        if (npc == null)
        {
            Debug.LogWarning($"[Zone] {Name} cannot remove null NPC");
            return;
        }

        if (_npcsInZone.Remove(npc))
        {
            CurrentOccupancy = Mathf.Max(0, CurrentOccupancy - 1);
            NPCExited?.Invoke(npc);
            Debug.Log($"[Zone] {Name} removed NPC {npc.Name}, Occupancy: {CurrentOccupancy}/{Capacity}");
            ValidateState();
        }
        else
        {
            Debug.LogWarning($"[Zone] {Name} could not remove NPC {npc.Name}: not in zone");
        }
    }

    public IEnumerable<NPC> GetNPCsInZone()
    {
        return _npcsInZone;
    }

    public Vector3 GetRandomPointInZone()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * Radius;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
        randomDirection += Center;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, Radius, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Center;
    }

    public float GetDistanceTo(Vector3 position)
    {
        return Vector3.Distance(Center, position);
    }

    public void ValidateState()
    {
        int actualCount = _npcsInZone.Count;
        if (CurrentOccupancy != actualCount)
        {
            Debug.LogWarning($"[Zone] {Name} occupancy mismatch: CurrentOccupancy={CurrentOccupancy}, Actual={actualCount}. Correcting...");
            CurrentOccupancy = actualCount;
        }
    }
    public Vector3 GetCenter()
    {
        return Center;
    }
}

public enum ZoneType
{
    House,
    Farm,
    Church,
    Zone,
    ConstructionSite,
    Road,
    Workshop,
    Graveyard
}