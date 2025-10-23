using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
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

    internal void DestroyBuildsAtRandom(int outcomeValue)
    {
        List<Building> destructionList = new List<Building>();
        if (_buildings.Count > 0)
        {
            var destroyables = _buildings.Where(type => type.Key != typeof(Castle) || type.Key != typeof(Graveyard) || type.Key != typeof(ConstructionSite)).SelectMany(values => values.Value).ToList();
            if (destroyables.Any())
            {
                for (int i = 0; i < outcomeValue; i++)
                {
                    var toDestroy = destroyables[UnityEngine.Random.Range(0, destroyables.Count() - 1)];
                    destructionList.Add(toDestroy);
                }
            }
            foreach (var toDestroy in destructionList)
            {
                UnRegisterBuilding(toDestroy);
            }
        }
    }
}
