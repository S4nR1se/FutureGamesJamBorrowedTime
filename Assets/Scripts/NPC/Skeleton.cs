using UnityEngine;

public class Skeleton : Undead
{
    public override int GetPurrCost()
    {
        if (_resourceManager.GetValue(Resources.Graves) >= GetGraveCost())
        {
            _resourceManager.UpdateValue(Resources.Graves, -GetGraveCost());
            return 20;
        }
        else
            return 60;
    }
    public override int GetGraveCost()
    {
        return 1;
    }
    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        base.Initialize(startZone, "Skeleton", 1, movementSpeed, occupation, restZoneType, activeCycle);
    }
}
