using Assets.Scripts.Managers;
using UnityEngine;

public class Castle : Building
{
    protected override Occupation AssociatedOccupation => throw new System.NotImplementedException();

    public override void Initialize()
    {
        if (BuildingTier == Tier.Three)
            GameManager.Instance.GetManager<GameEnderManager>().GameWon();

        base.Initialize();
    }

    protected override void OnNPCEnter(NPC npc)
    {
    }

    protected override void OnNPCExit(NPC npc)
    {
    }
}
