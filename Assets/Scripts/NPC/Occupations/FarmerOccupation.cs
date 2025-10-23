using UnityEngine;

public class FarmerOccupation : Occupation
{
    public override string Title { get; protected set; } = "Farmer";
    public FarmerOccupation(ZoneType workZoneType = ZoneType.Farm) : base(workZoneType)
    {
    }
}
