using System.Collections.Generic;
using UnityEngine;

public class BuildingsManagers : Manager
{
    private Dictionary<System.Type, List<Building>> _buildings = new();
    public override void Initialize()
    {
        Building[] existingBuildings = FindObjectsByType<Building>(FindObjectsSortMode.None);
        foreach (Building building in existingBuildings)
        {
            RegisterBuilding(building);
        }
    }
    public void RegisterBuilding(Building building)
    {
        System.Type buildingType = building.GetType();
        if (!_buildings.ContainsKey(buildingType))
        {
            _buildings[buildingType] = new List<Building>();
        }
        _buildings[buildingType].Add(building);
    }
    public void UnRegisterBuilding(Building building)
    {
        System.Type buildingType = building.GetType();
        if (_buildings.ContainsKey(buildingType))
        {
            _buildings[buildingType].Remove(building);
        }
    }
}
