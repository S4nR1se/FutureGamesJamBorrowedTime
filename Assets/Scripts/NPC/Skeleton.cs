using UnityEngine;

public class Skeleton : Undead
{
    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        base.Initialize(startZone, "Skeleton", 1, movementSpeed, occupation, restZoneType, activeCycle);
    }
}
