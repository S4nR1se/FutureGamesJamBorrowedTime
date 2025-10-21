using UnityEngine;

public class ConstructionSite : Building
{
    protected override Occupation AssociatedOccupation => new BuilderOccupation();

    private int _buildTimeDecrease = 0;
    private const int DECREASEPERPEASANT = 1;
    public override void Initialize()
    {
        //Logic for future building NEEDS GRID
        _buildTimeDecrease = 0;

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
        _buildTimeDecrease += DECREASEPERPEASANT;
    }

    protected override void OnNPCExit(NPC npc)
    {
        _buildTimeDecrease = Mathf.Max(0, _buildTimeDecrease - DECREASEPERPEASANT);
    }

    private void UpdateProduction()
    {
        //Needs logic to decrease the building time of the projected building
        _buildTimeDecrease = 0;
    }
}
