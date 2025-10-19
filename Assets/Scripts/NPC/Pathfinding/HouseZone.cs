using Unity.AI.Navigation;
using UnityEngine;

public class HouseZone : Zone
{
    public HouseZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.House, center, radius, parentSurface, capacity)
    {
    }
}
