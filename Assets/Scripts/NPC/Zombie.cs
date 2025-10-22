using System.Resources;
using UnityEngine;

public class Zombie : Undead
{
    public override int GetPurrCost()
    {
        if (_resourceManager.GetValue(Resources.Graves) >= GetGraveCost())
        {
            _resourceManager.UpdateValue(Resources.Graves, -GetGraveCost());
            return 70;
        }
        else
            return 210;
    }
    public override int GetGraveCost()
    {
        return 3;
    }
    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        base.Initialize(startZone, "Zombie", 3, movementSpeed, occupation, restZoneType, activeCycle);
    }
}
