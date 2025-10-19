using UnityEngine;

public abstract class Occupation
{
    public Zone WorkZone { get; protected set; }
    protected Occupation(Zone workZone)
    {
        WorkZone = workZone;
    }
}
