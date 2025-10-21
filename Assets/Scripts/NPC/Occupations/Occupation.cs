using UnityEngine;

public abstract class Occupation
{
    public abstract string Title { get; protected set; }
    public ZoneType WorkZoneType { get; protected set; }
    protected Occupation(ZoneType workZoneType)
    {
        WorkZoneType = workZoneType;
    }
}
