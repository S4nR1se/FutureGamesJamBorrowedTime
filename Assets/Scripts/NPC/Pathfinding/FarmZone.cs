using Unity.AI.Navigation;
using UnityEngine;

public class FarmZone : Zone
{
    public FarmZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.Farm, center, radius, parentSurface, capacity)
    {
    }
}
