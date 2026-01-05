using UnityEngine;

public class Farm : Building
{
    private int _foodStockGenerated = 0;
    //private const int FOODSTOCKPERPEASANT = 2;
    private AudioSource _musicSource;

    protected override Occupation AssociatedOccupation => new FarmerOccupation();

    public override void Initialize()
    {
        _foodStockGenerated = 0;

        TileType = TileType.Farm;

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
        _foodStockGenerated += OutputPerWorker;
    }

    protected override void OnNPCExit(NPC npc)
    {
        _foodStockGenerated = Mathf.Max(0, _foodStockGenerated - OutputPerWorker);
    }

    private void UpdateProduction()
    {
        int foodStockGenerated = AssociatedZone.CurrentOccupancy * OutputPerWorker;
        ResourceManager.UpdateValue(Resources.FoodStock, foodStockGenerated);
        //UndeadContact()
        _foodStockGenerated = 0;
    }
    public override void OnSelect(PlayerInputManager playerInputManager)
    {
        _musicSource = SoundManager.Instance.PlaySound("FarmingScythe");
        base.OnSelect(playerInputManager);
    }
}
