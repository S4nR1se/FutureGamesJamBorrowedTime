using Unity.AI.Navigation;
using UnityEngine;

public class Zone
{
    NavMeshSurface ParentSurface;
    string Name;
    ZoneType Type;
    Vector3 Center;
    int Capacity;
    int CurrentOccupancy;
    float Radius;
}

public enum ZoneType
{
    House,
    Farm,
    Church,
    ConstructionSite
}