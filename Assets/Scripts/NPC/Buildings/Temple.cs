using UnityEngine;

public class Temple : Building
{
    protected override Occupation AssociatedOccupation => new ChurchOccupation();

    private int _dreadReduced = 0;
    public override void Initialize()
    {
        _dreadReduced = 0;

        TileType = TileType.Temple;

        TimeManager timeManager = GameManager.Instance.GetManager<TimeManager>();
        if (timeManager != null)
        {
            timeManager.OnCycleCalculation += UpdateProduction;
        }

        base.Initialize();
    }
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            TimeManager timeManager = GameManager.Instance.GetManager<TimeManager>();
            if (timeManager != null)
            {
                timeManager.OnCycleCalculation -= UpdateProduction;
            }
        }
    }
    protected override void OnNPCEnter(NPC npc)
    {
        _dreadReduced += OutputPerWorker;
    }

    protected override void OnNPCExit(NPC npc)
    {
        _dreadReduced = Mathf.Max(0, _dreadReduced - OutputPerWorker);
    }

    private void UpdateProduction()
    {
        int dreadReduceGenerated = AssociatedZone.CurrentOccupancy * OutputPerWorker;
        ResourceManager.UpdateValue(Resources.Dread, -dreadReduceGenerated);
        _dreadReduced = 0;
    }
}
