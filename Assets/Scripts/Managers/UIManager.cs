using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class UIManager : Manager
{
    [System.Serializable]
    public class HudComponent
    {
        public string Title;
        public Image[] Icon;
        public TextMeshProUGUI[] Counter;
    }

    [SerializeField] private HudComponent[] _hudComponents;
    [SerializeField] private Dictionary<string, GameObject> _buildingPrefab;

    private GameObject _selectedBuilding = null;

    private Dictionary<string, HudComponent> _hudComponentsDic;

    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    private TilePlacementManager _buildingsManager;
    public override void Initialize()
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _npcManager = GameManager.Instance.GetManager<NPCManager>();
        _buildingsManager = GameManager.Instance.GetManager<TilePlacementManager>();

        if ( _resourceManager != null)
        {
            _resourceManager.OnResourceChange += OnResourceChange;
        }
        if(_npcManager != null)
        {
            _npcManager.OnNPCAmountChange += OnNPCAmountChange;
        }
    }

    private void InitializeHudComponentsIcon()
    {

    }

    private void InitializeHudComponentsCounter()
    {
        for(int i = 0; i < _hudComponents.Length; i++)
        {
           _hudComponentsDic.Add(_hudComponents[i].Title, _hudComponents[i]);
            for (int j = 0; j < _hudComponents[i].Counter.Length; j++)
            {
                _hudComponents[i].Counter[j].text = "50";
            }
        }
    }

    private void Start()
    {
        _hudComponentsDic = new();
        InitializeHudComponentsCounter();
    }

    private void OnDisable()
    {
        if (_resourceManager != null)
        {
            _resourceManager.OnResourceChange -= OnResourceChange;
        }
        if (_npcManager != null)
        {
            _npcManager.OnNPCAmountChange -= OnNPCAmountChange;
        }
    }
    private void OnResourceChange(Dictionary<Resources, int> resources)
    {
        if (resources == null || _hudComponentsDic == null)
        {
            return;
        }

        foreach (var resource in resources)
        {
            if(resource.Key == Resources.Purr)
            {
                _hudComponentsDic["Purr"].Counter[0].text = resource.Value.ToString();
            }
            else if (resource.Key == Resources.Graves)
            {
                _hudComponentsDic["Graves"].Counter[0].text = resource.Value.ToString();
            }
            else if (resource.Key == Resources.FoodStock)
            {
                _hudComponentsDic["Resources"].Counter[0].text = resource.Value.ToString();
            }
            else if (resource.Key == Resources.Materials)
            {
                _hudComponentsDic["Resources"].Counter[1].text = resource.Value.ToString();
            }
        }
    }

    private void OnNPCAmountChange(Dictionary<System.Type, List<NPC>> npcsByType)
    {
        if (npcsByType == null || _hudComponentsDic == null)
        {
            return;
        }

        int NPCS = 0;
        foreach (var npc in npcsByType)
        {
            NPCS += npc.Value.Count;
        }

        _hudComponentsDic["Peasants"].Counter[0].text = NPCS.ToString();
    }

    private void SelectedBuildingCreation(GameObject Building)
    {
        if(_selectedBuilding == null)
        {
            //Instatiate building prefab at mouse position
            //_selectedBuilding = Instatiate(Building, mousePosition);
        }
        else
        {
            Destroy(_selectedBuilding);
            //_selectedBuilding = Instatiate(Building, mousePosition);
        }
    }

    public void PickHouse()
    {
        _buildingsManager.SelectBuilding(TileType.House);
        // SelectedBuildingCreation(_buildingPrefab["House"]);
    }

    public void PickTemple()
    {
        _buildingsManager.SelectBuilding(TileType.Temple);
        //SelectedBuildingCreation(_buildingPrefab["Temple"]);
    }

    public void PickFarm()
    {
        _buildingsManager.SelectBuilding(TileType.Farm);
        //SelectedBuildingCreation(_buildingPrefab["Farm"]);
    }

    public void PickWorkshop()
    {
        _buildingsManager.SelectBuilding(TileType.Workshop);
        //SelectedBuildingCreation(_buildingPrefab["Workshp"]);
    }
}
