using UnityEngine;

public class Graveyard : Building
{
    protected override Occupation AssociatedOccupation => new BuilderOccupation();

    private NPCManager _npcManager;
    private TimeManager _timeManager;

    public override void Initialize()
    {
        _npcManager = GameManager.Instance.GetManager<NPCManager>();
        _timeManager = GameManager.Instance.GetManager<TimeManager>();
        if (_timeManager != null)
        {
            _timeManager.OnCycleCalculation += UpdateProduction;
        }

        base.Initialize();
    }
    private void OnDisable()
    {
        if (_timeManager != null)
        {
            _timeManager.OnCycleCalculation -= UpdateProduction;
        }
    }
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
    private void UpdateProduction()
    {
    }
}
