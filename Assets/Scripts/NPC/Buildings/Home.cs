using System.Collections.Generic;
using UnityEngine;

public class Home : Building
{
    protected override Occupation AssociatedOccupation => new UnemployedOccupation();

    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    private TimeManager _timeManager;

    public override void Initialize()
    {
        TileType = TileType.House;

        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
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
        if (PlayerInputManager.TryGetPreviousWorkerSelection(out IWorker prevWorker))
        {
            if (prevWorker is Undead) return;
            prevWorker.TravelToZone(AssociatedZone);
            return;
        }

        if (UIManager != null)
        {
            UIManager.DisplayBuildingInfo(this, AssociatedZone.GetNPCsInZone());
        }
    }
    private void UpdateProduction()
    {
        if (_timeManager.GetCurrentCycle() != DayCycle.Night) return;

        int npcProcreated = AssociatedZone.CurrentOccupancy;
        for(int i = 0; i < npcProcreated; i++)
        {
            _npcManager.SpawnPeasant(AssociatedZone);
            
        }
    }
    [ContextMenu("GatherPurr")]
    public void GatherPurr()
    {
        if(_timeManager.CurrentDayCycle != DayCycle.Night) return;
        IEnumerable<NPC> npcs = AssociatedZone.GetNPCsInZone();

        foreach (NPC npc in npcs)
        {
            npc.DecreaseLifeSpan(1);
            _resourceManager.UpdateValue(Resources.Purr, 1);
        }
    }
}
