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
        if (Capacity < 0) return false; // Unlimited capacity
        return CurrentOccupancy >= Capacity;
    }

    public bool TryEnter()
    {
        if (IsFull()) return false;
        CurrentOccupancy++;
        return true;
    }

    public void Exit()
    {
        CurrentOccupancy = Mathf.Max(0, CurrentOccupancy - 1);
    }

    public Vector3 GetRandomPointInZone()
    {
        Vector2 randomCircle = Random.insideUnitCircle * Radius;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
        randomDirection += Center;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, Radius, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Center; // Fallback
    }

    public float GetDistanceTo(Vector3 position)
    {
        return Vector3.Distance(Center, position);
    }
}

public enum ZoneType
{
    House,
    Farm,
    Church,
    ConstructionSite
}