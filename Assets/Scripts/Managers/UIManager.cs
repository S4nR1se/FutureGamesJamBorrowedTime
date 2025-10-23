using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.UI.Image;

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
    [SerializeField] private GameObject      _buildingNPCInfoPrefab;
    [SerializeField] private GameObject[]      _buildingWindow;
    [SerializeField] private Transform pos;
    [SerializeField] private Transform posP;

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

        _hudComponentsDic = new();
        InitializeHudComponentsCounter();
        _npcInfo.SetActive(false);
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
        if (npc is IWorker worker)
        {
            _npcOccupationText.text = "Occupation: " + worker.Occupation.Title;
        }
        else
        {
            _npcOccupationText.text = "None";
        }
        _npcLifeSpanText.text = "LifeSpan: " + npc.LifeSpan.ToString();

        if(npc is Peasant peasant)
        {
            _npcMoodText.text = "Mood: " + peasant.GetMood();
        }
        else
        {
            _npcMoodText.text = " ";
        }

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

    public void DisplayBuildingInfo()//IEnumerable<NPC> enumerable)
    {
        int new_height = 0;
        for(int i = 0; i < 1; i++)
        {
            GameObject _buildingNPCInfo = Instantiate(_buildingNPCInfoPrefab);
            for (int j = 0; j < _buildingNPCInfo.transform.childCount; ++j)
            {
                Transform child = _buildingNPCInfo.transform.GetChild(j);
               
            }

            _buildingNPCInfo.transform.SetParent(posP);
            Vector3 NewPos = new Vector3(pos.transform.position.x, pos.transform.position.y + new_height, pos.transform.position.z);
            _buildingNPCInfo.transform.position = NewPos;
            new_height -= 30;
        }
      
    }

    public void HideBuildingInfo()
    {

    }
}
