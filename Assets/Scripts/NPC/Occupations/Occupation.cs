using UnityEngine;

public abstract class Occupation
{
    public ZoneType WorkZoneType { get; protected set; }
    protected Occupation(ZoneType workZoneType)
    {
        WorkZoneType = workZoneType;
    }
}
