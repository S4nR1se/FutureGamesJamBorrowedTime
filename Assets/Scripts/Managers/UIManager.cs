using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private GameObject _buildingNPCInfoPrefab;
    [SerializeField] private GameObject _buildingWindow;
    [SerializeField] private Transform posP;

    private CanvasGroup _npcInfoCanvasGroup;
    private CanvasGroup _buildingInfoCanvasGroup;

    private Dictionary<string, HudComponent> _hudComponentsDic;
    private List<GameObject> _buildingNPCInfoList = new();
    private List<TextMeshProUGUI> _borrowTimeText;
    private int _borrowCount = 5;

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

        _npcInfoCanvasGroup = _npcInfo.GetComponent<CanvasGroup>();
        _buildingInfoCanvasGroup = _buildingWindow.GetComponent<CanvasGroup>();

        _borrowTimeText = new();
        _hudComponentsDic = new();

        HideBuildingInfo();
        HideNPCInfo();

        InitializeHudComponentsCounter();
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
        _npcInfoCanvasGroup.alpha = 1;
        _npcInfoCanvasGroup.interactable = true;
        _npcInfoCanvasGroup.blocksRaycasts = true;

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
        _npcInfoCanvasGroup.alpha = 0;
        _npcInfoCanvasGroup.interactable = false;
        _npcInfoCanvasGroup.blocksRaycasts = false;
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

        _buildingInfoCanvasGroup.alpha = 1f;
        _buildingInfoCanvasGroup.interactable = true;
        _buildingInfoCanvasGroup.blocksRaycasts = true;

        //foreach (NPC npc in enumerable)
        //{
        //    GameObject _buildingNPCInfo = Instantiate(_buildingNPCInfoPrefab);
        //    _buildingNPCInfo.transform.SetParent(posP);
        //    Vector3 NewPos = new Vector3(pos.transform.position.x, pos.transform.position.y + new_height, pos.transform.position.z);
        //    _buildingNPCInfo.transform.position = NewPos;
        //    new_height -= 30;
        //    for (int j = 0; j < _buildingNPCInfo.transform.childCount; ++j)
        //    {
        //        Transform child = _buildingNPCInfo.transform.GetChild(j);
        //        if(child.name == "Name")
        //        {
        //            child.gameObject.GetComponent<TextMeshProUGUI>().text = npc.name;
        //        }
        //        else if(child.name == "BTCounter")
        //        {
        //            child.gameObject.GetComponent<TextMeshProUGUI>().text = npc.LifeSpan.ToString();
        //        }
        //    }

        //    _buildingNPCInfoList.Add(_buildingNPCInfo);
        //}

        for (int i = 0; i < 20; ++i)
        {
            GameObject _buildingNPCInfo = Instantiate(_buildingNPCInfoPrefab);
            _buildingNPCInfo.transform.SetParent(posP);
            for (int j = 0; j < _buildingNPCInfo.transform.childCount; ++j)
            {
                Transform child = _buildingNPCInfo.transform.GetChild(j);
                if (child.name == "Name")
                {
                    child.gameObject.GetComponent<TextMeshProUGUI>().text = "Hi";
                }
                else if (child.name == "BTCounter")
                {
                    child.gameObject.GetComponent<TextMeshProUGUI>().text = _borrowCount.ToString();
                    _borrowTimeText.Add(child.gameObject.GetComponent<TextMeshProUGUI>());
                }
            }

            _buildingNPCInfoList.Add(_buildingNPCInfo);
        }
    }

    public void HideBuildingInfo()
    {
        _buildingInfoCanvasGroup.alpha = 0f;
        _buildingInfoCanvasGroup.interactable = false;
        _buildingInfoCanvasGroup.blocksRaycasts = false;
        for(int i = 0; i < _buildingNPCInfoList.Count; ++i)
        {
            Destroy(_buildingNPCInfoList[i]);
        }
    }

    public void BorrowDayPlusButton()
    {
        _borrowCount++;
        UpdateBorrowTimeText();
    }

    public void BorrowDayMinusButton()
    {
        _borrowCount--;
        UpdateBorrowTimeText();
    }

    public void UpdateBorrowTimeText()
    {
        for(int i = 0; i < _borrowTimeText.Count; ++i)
        {
            Debug.Log("hi");
            _borrowTimeText[i].text = _borrowCount.ToString();
        }
        
    }    
}
