using UnityEngine;

public class Skeleton : Undead
{
    public override int GetPurrCost()
    {
        return 1;
    }

    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        base.Initialize("Skeleton", 1, movementSpeed, occupation, restZoneType, activeCycle);
    }
}
