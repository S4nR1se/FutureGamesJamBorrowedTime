using Unity.AI.Navigation;
using UnityEngine;

public class RoadZone : Zone
{
    public RoadZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.Road, center, radius, parentSurface, capacity)
    {
    }
}
