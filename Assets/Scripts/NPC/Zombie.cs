using UnityEngine;

public class Zombie : Undead
{
    public override int GetPurrCost()
    {
        return 1;
    }
    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        base.Initialize("Zombie", 3, movementSpeed, occupation, restZoneType, activeCycle);
    }
}
