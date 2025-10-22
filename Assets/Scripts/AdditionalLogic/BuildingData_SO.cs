using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData_SO", menuName = "Scriptable Objects/BuildingData_SO")]
public class BuildingData_SO : ScriptableObject
{
    public int MaterialCost;
    public int BuildTime;

    public float PlacementYOffset = 2f;
    public float DefaultRotationY = 0f;
    public GameObject PreviewPrefab;
}
