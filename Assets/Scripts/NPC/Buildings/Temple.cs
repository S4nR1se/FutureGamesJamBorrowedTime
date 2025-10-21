using UnityEngine;

public class Temple : Building
{
    protected override Occupation AssociatedOccupation => new ChurchOccupation();

    protected override void OnNPCEnter(NPC npc)
    {
    }

    protected override void OnNPCExit(NPC npc)
    {
    }
}
