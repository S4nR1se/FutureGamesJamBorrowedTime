using UnityEngine;

public class Farm : Building
{
    private int _foodStockGenerated = 0;
    private const int FOODSTOCKPERPEASANT = 5;

    protected override Occupation AssociatedOccupation => new FarmerOccupation();

    public override void Initialize()
    {
        _foodStockGenerated = 0;

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
        _foodStockGenerated += FOODSTOCKPERPEASANT;
    }

    protected override void OnNPCExit(NPC npc)
    {
        _foodStockGenerated = Mathf.Max(0, _foodStockGenerated - FOODSTOCKPERPEASANT);
    }

    private void UpdateProduction()
    {
        int foodStockGenerated = AssociatedZone.CurrentOccupancy * FOODSTOCKPERPEASANT;
        ResourceManager.UpdateValue(Resources.FoodStock, foodStockGenerated);

        _foodStockGenerated = 0;
    }
    public override void OnSelect(PlayerInputManager playerInputManager)
    {
        base.OnSelect(playerInputManager);
    }
}
