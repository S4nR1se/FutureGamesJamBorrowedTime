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

    [System.Serializable]
    private class NPCEntryData
    {
        public NPC NPC;
        public GameObject GameObject;
        public TextMeshProUGUI CounterText;
        public int CounterValue;
    }

    [SerializeField] private HudComponent[] _hudComponents;
    [SerializeField] private Dictionary<string, GameObject> _buildingPrefab;
    [SerializeField] private GameObject _npcInfo;

    [SerializeField] private TextMeshProUGUI _npcNameText;
    [SerializeField] private TextMeshProUGUI _npcOccupationText;
    [SerializeField] private TextMeshProUGUI _npcLifeSpanText;
    [SerializeField] private TextMeshProUGUI _npcMoodText;
    [SerializeField] private Image _npcPhoto;

    [SerializeField] private Button _npcArrowForward;
    [SerializeField] private Button _npcArrowBackward;

    [SerializeField] private GameObject _npcEntryTemplate;
    [SerializeField] private GameObject _buildingWindowParent;
    [SerializeField] private Transform _parentTransform;

    private CanvasGroup _npcInfoCanvasGroup;
    private CanvasGroup _buildingInfoCanvasGroup;
    private Dictionary<string, HudComponent> _hudComponentsDic;
    private List<NPCEntryData> _npcEntries = new();

    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    private TilePlacementManager _buildingsManager;
    private ZoneManager _zoneManager;

    private List<Occupation> _availableOccupations = new();
    private int _currentOccupationIndex = 0;
    private NPC _currentNPC;

    public override void Initialize()
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _npcManager = GameManager.Instance.GetManager<NPCManager>();
        _buildingsManager = GameManager.Instance.GetManager<TilePlacementManager>();
        _zoneManager = GameManager.Instance.GetManager<ZoneManager>();

        if (_resourceManager != null)
        {
            _resourceManager.OnResourceChange += OnResourceChange;
            OnResourceChange(_resourceManager.GetAllResources());
        }
        if (_npcManager != null)
        {
            _npcManager.OnNPCAmountChange += OnNPCAmountChange;
        }

        _npcInfoCanvasGroup = _npcInfo.GetComponent<CanvasGroup>();
        _buildingInfoCanvasGroup = _buildingWindowParent.GetComponent<CanvasGroup>();

        _hudComponentsDic = new();
        _npcEntries = new();

        HideBuildingInfo();
        HideNPCInfo();

        _npcArrowForward.onClick.AddListener(OnArrowForwardClicked);
        _npcArrowBackward.onClick.AddListener(OnArrowBackClicked);

        if (_npcEntryTemplate != null)
        {
            _npcEntryTemplate.SetActive(false);
        }

        _availableOccupations = new List<Occupation>()
        {
            new FarmerOccupation(),
            new BuilderOccupation(),
            new LaborerOccupation(),
            new ChurchOccupation(),
        };

        InitializeHudComponentsCounter();
    }

    private void InitializeHudComponentsCounter()
    {
        for (int i = 0; i < _hudComponents.Length; i++)
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
            if (resource.Key == Resources.Purr)
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
            else if (resource.Key == Resources.Dread)
            {
                _hudComponentsDic["Dread"].Counter[0].text = resource.Value.ToString();
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
        _currentNPC = npc;

        _npcInfoCanvasGroup.alpha = 1;
        _npcInfoCanvasGroup.interactable = true;
        _npcInfoCanvasGroup.blocksRaycasts = true;

        _npcPhoto.sprite = npc.PassportPhoto;

        _npcNameText.text = "Name: " + npc.Name;

        _npcLifeSpanText.text = "LifeSpan: " + npc.LifeSpan.ToString();

        if (npc is IWorker worker && worker.Occupation != null)
        {
            _npcOccupationText.text = worker.Occupation.Title;
            _currentOccupationIndex = _availableOccupations.FindIndex(o => o.Title == worker.Occupation.Title);
            if (_currentOccupationIndex < 0) _currentOccupationIndex = 0;
        }
        else
        {
            _npcOccupationText.text = "Unemployed";
            _currentOccupationIndex = 0;
        }

        if (npc is Peasant peasant)
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

    public void DisplayBuildingInfo(IEnumerable<NPC> npcs = null)
    {
        if (npcs == null)
        {
            Debug.LogWarning("No NPCs provided for DisplayBuildingInfo");
            return;
        }

        foreach (var entry in _npcEntries)
        {
            Destroy(entry.GameObject);
        }
        _npcEntries.Clear();

        _buildingInfoCanvasGroup.alpha = 1f;
        _buildingInfoCanvasGroup.interactable = true;
        _buildingInfoCanvasGroup.blocksRaycasts = true;

        List<NPC> npcList = new List<NPC>(npcs);
        int counter = 0;

        foreach (NPC npc in npcList)
        {
            counter++;

            if (_npcEntryTemplate == null)
            {
                Debug.LogError("NPCEntryTemplate is not assigned!");
                return;
            }

            GameObject buildingNPCInfo = Instantiate(_npcEntryTemplate, _parentTransform);
            buildingNPCInfo.GetComponent<RectTransform>().localScale = Vector3.one;
            buildingNPCInfo.SetActive(true);

            TextMeshProUGUI nameText = null;
            TextMeshProUGUI counterText = null;
            Button button1 = null, button2 = null, button3 = null, button4 = null;

            foreach (Transform child in buildingNPCInfo.transform)
            {
                if (child.name == "Info")
                {
                    button1 = child.GetComponentInChildren<Button>();
                    nameText = child.GetComponent<TextMeshProUGUI>();
                    nameText.text = $"{counter}. {npc.Name} {npc.Age}";
                }
                else if (child.name == "Purr")
                {
                    button2 = child.GetComponentInChildren<Button>();
                }
                else if (child.name == "Add")
                {
                    button3 = child.GetComponentInChildren<Button>();
                }
                else if (child.name == "Counter")
                {
                    counterText = child.GetComponent<TextMeshProUGUI>();
                    counterText.text = "0";
                }
                else if (child.name == "Detract")
                {
                    button4 = child.GetComponentInChildren<Button>();
                }
            }

            NPCEntryData entryData = new NPCEntryData
            {
                NPC = npc,
                GameObject = buildingNPCInfo,
                CounterText = counterText,
                CounterValue = 0 
            };

            if (button1 != null)
            {
                button1.onClick.AddListener(() => OnButton1Clicked(npc));
            }
            if (button2 != null)
            {
                button2.onClick.AddListener(() => OnButton2Clicked(npc, entryData));
            }
            if (button3 != null)
            {
                button3.onClick.AddListener(() => OnAddButtonClicked(entryData));
            }
            if (button4 != null)
            {
                button4.onClick.AddListener(() => OnDetractButtonClicked(entryData));
            }

            _npcEntries.Add(entryData);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_parentTransform.GetComponent<RectTransform>());

        ScrollRect scrollRect = _parentTransform.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
        }
    }

    public void HideBuildingInfo()
    {
        _buildingInfoCanvasGroup.alpha = 0f;
        _buildingInfoCanvasGroup.interactable = false;
        _buildingInfoCanvasGroup.blocksRaycasts = false;

        foreach (var entry in _npcEntries)
        {
            Destroy(entry.GameObject);
        }
        _npcEntries.Clear();
    }

    private void OnButton1Clicked(NPC npc)
    {
        DisplayNPCInfo(npc);
    }

    private void OnButton2Clicked(NPC npc, NPCEntryData entry)
    {
        if(npc is Peasant peasant)
        {
            for (int i = 0; i < entry.CounterValue; i++)
            {
                peasant.GatherPurr();
            }
        }
        entry.CounterValue = 0;
    }

    private void OnAddButtonClicked(NPCEntryData entry)
    {
        entry.CounterValue = Mathf.Min(entry.CounterValue + 1, entry.NPC.LifeSpan);
        if (entry.CounterText != null)
        {
            entry.CounterText.text = entry.CounterValue.ToString();
        }
    }

    private void OnDetractButtonClicked(NPCEntryData entry)
    {
        entry.CounterValue = Mathf.Max(0, entry.CounterValue - 1);
        if (entry.CounterText != null)
        {
            entry.CounterText.text = entry.CounterValue.ToString();
        }
    }
    private void OnArrowForwardClicked()
    {
        if (_availableOccupations.Count == 0) return;

        _currentOccupationIndex = (_currentOccupationIndex + 1) % _availableOccupations.Count;
        UpdateOccupationDisplay();
    }
    private void OnArrowBackClicked()
    {
        if (_availableOccupations.Count == 0) return;

        _currentOccupationIndex = (_currentOccupationIndex - 1 + _availableOccupations.Count) % _availableOccupations.Count;
        UpdateOccupationDisplay();
    }
    private void UpdateOccupationDisplay()
    {
        _npcArrowForward.interactable = _currentNPC != null;
        _npcArrowBackward.interactable = _currentNPC != null;

        if (_currentNPC == null) return;
        if (_availableOccupations.Count == 0) return;

        var occupation = _availableOccupations[_currentOccupationIndex];
        _npcOccupationText.text = occupation.Title;

        if (_currentNPC is IWorker worker)
        {
            worker.AssignOccupation(occupation);

            Zone workZone = _zoneManager.GetRandomAvailableZone(occupation.WorkZoneType);
            if(workZone != null)
            {
                worker.TravelToZone(workZone);
            }
        }
    }
}
