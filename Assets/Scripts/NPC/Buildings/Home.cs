using UnityEngine;

public class Home : Building
{
    protected override Occupation AssociatedOccupation => new BuilderOccupation();

    protected override void OnNPCEnter(NPC npc)
    {
        
    }

    protected override void OnNPCExit(NPC npc)
    {
        
    }
    public override void OnSelect(PlayerInputManager playerInputManager)
    {
        //Skip
    }
}
