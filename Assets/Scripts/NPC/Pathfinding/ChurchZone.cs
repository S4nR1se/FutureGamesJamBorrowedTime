using Unity.AI.Navigation;
using UnityEngine;

public class ChurchZone : Zone
{
    public ChurchZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.Church, center, radius, parentSurface, capacity)
    {
    }
}
