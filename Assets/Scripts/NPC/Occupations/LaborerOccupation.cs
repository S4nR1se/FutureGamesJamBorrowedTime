using UnityEngine;

public class LaborerOccupation : Occupation
{
    public override string Title { get; protected set; } = "Laborer";
    public LaborerOccupation(ZoneType workZoneType = ZoneType.Workshop) : base(workZoneType)
    {
    }
}
