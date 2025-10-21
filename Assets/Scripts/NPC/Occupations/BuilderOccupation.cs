using UnityEngine;

public class BuilderOccupation : Occupation
{
    public override string Title { get; protected set; } = "Builder";
    public BuilderOccupation(ZoneType workZoneType = ZoneType.ConstructionSite) : base(workZoneType)
    {
    }
}
