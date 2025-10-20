using UnityEngine;

public class Workshop : Building
{
    private int _materialGenerated = 0;
    private const int MATERIALPERPEASANT = 3;
    public override void Initialize()
    {
        _materialGenerated = 0;

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
        _materialGenerated += MATERIALPERPEASANT;
    }

    protected override void OnNPCExit(NPC npc)
    {
        _materialGenerated = Mathf.Max(0, _materialGenerated - MATERIALPERPEASANT);
    }

    private void UpdateProduction()
    {
        ResourceManager.UpdateValue(Resources.Materials, _materialGenerated);

        _materialGenerated = 0;
    }
}
