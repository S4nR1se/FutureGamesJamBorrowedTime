using Unity.AI.Navigation;
using UnityEngine;

public class WorkshopZone : Zone
{
    public WorkshopZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.Workshop, center, radius, parentSurface, capacity)
    {
    }
}
