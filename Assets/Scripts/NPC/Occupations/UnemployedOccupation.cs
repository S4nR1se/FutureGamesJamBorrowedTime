using UnityEngine;

public class UnemployedOccupation : Occupation
{
    public UnemployedOccupation(ZoneType workZoneType = ZoneType.House) : base(workZoneType)
    {
    }

    public override string Title { get; protected set; } = "Unemployed";

}
