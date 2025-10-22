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
        public int[] Value;
        public Image[] Icon;
        public TextMeshProUGUI[] Counter;
    }

    [SerializeField] private HudComponent[] _hudComponents;
    [SerializeField] private Dictionary<string, GameObject> _buildingPrefab;
    [SerializeField] private GameObject _npcInfo;
    [SerializeField] private TextMeshProUGUI _npcNameText;
    [SerializeField] private TextMeshProUGUI _npcOccupationText;
    [SerializeField] private TextMeshProUGUI _npcLifeSpanText;
    [SerializeField] private TextMeshProUGUI _npcMoodText;

    private Dictionary<string, HudComponent> _hudComponentsDic;

    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    private TilePlacementManager _buildingsManager;
    private IWorker _workerInterface;
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
            for (int j = 0; j < _hudComponents[i].Counter.Length; j++)
            {
                _hudComponents[i].Counter[j].text = _hudComponents[i].Value[j].ToString();
            }
            _hudComponentsDic.Add(_hudComponents[i].Title, _hudComponents[i]);
        }
    }

    private void Start()
    {
        _hudComponentsDic = new();
        InitializeHudComponentsCounter();
        _npcInfo.SetActive(false);
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

    public void DisplayNPCInfo(NPC npc)
    {
        _npcInfo.SetActive(true);
        _npcNameText.text = "Name: " + npc.Name;
       // npcOccupationText.text 
        _npcLifeSpanText.text = "LifeSpan: " + npc.LifeSpan.ToString();
        //_npcMoodText.text
    }

    public void HideNPCInfo()
    {
        _npcInfo.SetActive(false);
    }

    public void PickHouse()
    {
        _buildingsManager.SelectBuilding(TileType.House);
    }

    public void PickTemple()
    {
        _buildingsManager.SelectBuilding(TileType.Temple);
    }

    public void PickFarm()
    {
        _buildingsManager.SelectBuilding(TileType.Farm);
    }

    public void PickWorkshop()
    {
        _buildingsManager.SelectBuilding(TileType.Workshop);
    }
}
