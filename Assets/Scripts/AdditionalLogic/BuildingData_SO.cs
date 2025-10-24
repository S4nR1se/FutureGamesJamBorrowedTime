using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData_SO", menuName = "Scriptable Objects/BuildingData_SO")]
public class BuildingData_SO : ScriptableObject
{
    public int BuildTime;
    public int OutputPerWorker;
    public Resources Resource;
    public int MaterialCost;
    public int PurrCost;
    public int WorkerSize;
    public BuildingData_SO Upgrade;
    public Tier BuildingTier;

    public float PlacementYOffset = 2f;
    public float DefaultRotationY = 0f;
    public GameObject PreviewPrefab;

    public string BuildingName = "";
    public string Description = "Building does stuff fr";
}
