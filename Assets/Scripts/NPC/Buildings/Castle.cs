using Assets.Scripts.Managers;
using UnityEngine;

public class Castle : Building
{
    protected override Occupation AssociatedOccupation => new UnemployedOccupation();

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
    public override void OnSelect(PlayerInputManager playerInputManager)
    {
        if (PlayerInputManager.TryGetPreviousWorkerSelection(out IWorker prevWorker))
        {
            return;
        }
        UIManager.DisplayCastleInfo(this);
    }
}
