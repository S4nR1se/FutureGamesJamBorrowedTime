using UnityEngine;

public class ChurchOccupation : Occupation
{
    public override string Title { get; protected set; } = "Priest";
    public ChurchOccupation(ZoneType workZoneType = ZoneType.Church) : base(workZoneType)
    {
    }
}
