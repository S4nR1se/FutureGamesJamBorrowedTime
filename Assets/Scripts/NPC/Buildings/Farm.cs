using UnityEngine;

public class Farm : Building
{
    private int _foodStockGenerated = 0;
    private const int FOODSTOCKPERPEASANT = 5;
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
        ResourceManager.UpdateValue(Resources.FoodStock, _foodStockGenerated);

        _foodStockGenerated = 0;
    }
}
