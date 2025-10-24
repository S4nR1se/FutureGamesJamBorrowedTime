using UnityEngine;

public class Workshop : Building
{
    protected override Occupation AssociatedOccupation => new LaborerOccupation();
    private int _materialGenerated = 0;

    private TimeManager timeManager;
    //private const int MATERIALPERPEASANT = 3;
    public override void Initialize()
    {
        _materialGenerated = 0;

        TileType = TileType.Workshop;

        timeManager = GameManager.Instance.GetManager<TimeManager>();
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
        _materialGenerated += OutputPerWorker;
    }

    protected override void OnNPCExit(NPC npc)
    {
        _materialGenerated = Mathf.Max(0, _materialGenerated - OutputPerWorker);
    }

    private void UpdateProduction()
    {
        int generatedMaterials = OutputPerWorker * AssociatedZone.CurrentOccupancy;
        ResourceManager.UpdateValue(Resources.Materials, generatedMaterials);

        _materialGenerated = 0;
    }
}
