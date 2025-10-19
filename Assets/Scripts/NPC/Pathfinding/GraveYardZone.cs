using Unity.AI.Navigation;
using UnityEngine;

public class GraveYardZone : Zone
{
    public GraveYardZone(string name, Vector3 center, float radius, NavMeshSurface parentSurface = null, int capacity = -1)
        : base(name, ZoneType.Graveyard, center, radius, parentSurface, capacity)
    {
    }
}
