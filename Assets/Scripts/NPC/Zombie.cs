using System.Resources;
using UnityEngine;

public class Zombie : Undead
{

    public AudioClip ZombieBiteSFX;

    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night, SoundManager soundManager = null)
    {
        _soundManager = soundManager;
        base.Initialize(startZone, "Zombie", 3, movementSpeed, occupation, restZoneType, activeCycle);
    }
}
