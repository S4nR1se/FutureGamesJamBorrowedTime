using Unity.AI.Navigation;
using UnityEngine;

public class ConstructionZone : Zone
{
    public ConstructionZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.ConstructionSite, center, radius, parentSurface, capacity)
    {
    }
}
