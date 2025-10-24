using Assets.Scripts.Managers;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.Managers.GameEnderManager;

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
    [SerializeField] private TextMeshProUGUI _buildingTitle;
    [SerializeField] private TextMeshProUGUI _buildingTier;
    [SerializeField] private TextMeshProUGUI _buildingUpgradeText;
    [SerializeField] private Button _buildingUpgradeButton;

    [SerializeField] private GameObject _graveYardWindow;
    [SerializeField] private TextMeshProUGUI _graveYardUpgradeText;
    [SerializeField] private TextMeshProUGUI _graveYardTitle;
    [SerializeField] private TextMeshProUGUI _graveyardTier;
    [SerializeField] private Button _skeletonButton;
    [SerializeField] private Button _zombieButton;
    [SerializeField] private Button _graveyardUpgradeButton;

    [SerializeField] private GameObject _castleWindow;
    [SerializeField] private TextMeshProUGUI _castleTier;
    [SerializeField] private TextMeshProUGUI _castleRequirements;
    [SerializeField] private Button _castleUpgradeButton;

    [SerializeField] private Transform _parentTransform;
    [SerializeField] private Material PURRmat;

    [SerializeField] private GameObject _eventUIPrefab;
    [SerializeField] private GameObject _gameEnderUIPrefab;
    [SerializeField] private Button _gameOver;

    private CanvasGroup _npcInfoCanvasGroup;
    private CanvasGroup _buildingInfoCanvasGroup;
    private CanvasGroup _graveYardCanvasGroup;
    private CanvasGroup _castleCanvasGroup;

    private Building _currentBuilding;
    private IEnumerable<NPC> _currentNpcs;

    private Dictionary<string, HudComponent> _hudComponentsDic;
    private List<NPCEntryData> _npcEntries = new();

    private Event_SO _eventToSolve;
    private GameObject _event;
    private GameObject _gameEnder;

    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    private TilePlacementManager _buildingsManager;
    private ZoneManager _zoneManager;
    private TimeManager _timeManager;
    private EventManager _eventManager;
    private GameEnderManager _gameEnderManager;

    private List<Occupation> _availableOccupations = new();
    private int _currentOccupationIndex = 0;
    private NPC _currentNPC;

    public override void Initialize()
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _npcManager = GameManager.Instance.GetManager<NPCManager>();
        _buildingsManager = GameManager.Instance.GetManager<TilePlacementManager>();
        _zoneManager = GameManager.Instance.GetManager<ZoneManager>();
        _timeManager = GameManager.Instance.GetManager<TimeManager>();
        _eventManager = GameManager.Instance.GetManager<EventManager>();
        _gameEnderManager = GameManager.Instance.GetManager<GameEnderManager>();

        if (_resourceManager != null)
        {
            _resourceManager.OnResourceChange += OnResourceChange;
            OnResourceChange(_resourceManager.GetAllResources());
        }
        if (_npcManager != null)
        {
            _npcManager.OnNPCAmountChange += OnNPCAmountChange;
        }
        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage += OnTimePassage;
        }
        if (_eventManager != null)
        {
            _eventManager.OnNewEvent += OnNewEvent;
        }
        if (_gameEnderManager != null)
        {
            _gameEnderManager.GameOver += GameOver;
        }

        //_gameOver.onClick.AddListener(delegate { TestGameOver(GameResult.GameWon, 2); });

        _npcInfoCanvasGroup = _npcInfo.GetComponent<CanvasGroup>();
        _buildingInfoCanvasGroup = _buildingWindowParent.GetComponent<CanvasGroup>();
        _graveYardCanvasGroup = _graveYardWindow.GetComponent<CanvasGroup>();
        _castleCanvasGroup = _castleWindow.GetComponent<CanvasGroup>();

        _hudComponentsDic = new();
        _npcEntries = new();

        HideGraveyardInfo();
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
                if (PURRmat != null)
                {
                    PURRmat.SetFloat("_Fill",resource.Value);
                }
            }
            else if (resource.Key == Resources.Graves)
            {
                _hudComponentsDic["Graves"].Counter[0].text = resource.Value.ToString();
                UpdateUndeadCost(resource.Value);
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

    private void UpdateUndeadCost(int gravesRemaining)
    {
        if (gravesRemaining >= Graveyard.SKELETONGRAVECOST)
            _hudComponentsDic["Summoning"].Counter[0].text = Graveyard.SKELETONPURRCOSTWITHGRAVE.ToString();
        else
            _hudComponentsDic["Summoning"].Counter[0].text = Graveyard.SKELETONPURRCOSTWITHOUTGRAVE.ToString();

        if (gravesRemaining >= Graveyard.ZOMBIEGRAVECOST)
            _hudComponentsDic["Summoning"].Counter[1].text = Graveyard.ZOMBIEPURRCOSTWITHGRAVE.ToString();
        else
            _hudComponentsDic["Summoning"].Counter[1].text = Graveyard.ZOMBIEPURRCOSTWITHOUTGRAVE.ToString();
    }

    private void OnNPCAmountChange(Dictionary<System.Type, List<NPC>> npcsByType)
    {
        if (npcsByType == null || _hudComponentsDic == null)
            return;

        int peasants = 0;
        int skeletons = 0;
        int zombies = 0;

        foreach (var kvp in npcsByType)
        {
            Type npcType = kvp.Key;
            List<NPC> npcList = kvp.Value;

            if (typeof(Peasant).IsAssignableFrom(npcType))
            {
                peasants += npcList.Count;
            }
            else if (typeof(Skeleton).IsAssignableFrom(npcType))
            {
                skeletons += npcList.Count;
            }
            else if (typeof(Zombie).IsAssignableFrom(npcType))
            {
                zombies += npcList.Count;
            }
        }

        if (_hudComponentsDic.ContainsKey("Peasants"))
            _hudComponentsDic["Peasants"].Counter[0].text = peasants.ToString();

        if (_hudComponentsDic.ContainsKey("Summoning"))
        {
            _hudComponentsDic["Summoning"].Counter[0].text = skeletons.ToString();
            _hudComponentsDic["Summoning"].Counter[1].text = zombies.ToString();
        }
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

    private void OnTimePassage(DayCycle cycle)
    {
        if (cycle == DayCycle.Day)
        {
            _hudComponentsDic["Day"].Counter[0].text = _timeManager.DayNumber.ToString();
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

    public void DisplayBuildingInfo(Building building = null, IEnumerable<NPC> npcs = null)
    {
        _currentBuilding = building;
        _currentNpcs = npcs;

        HideAllInfo();
        if (npcs == null)
        {
            Debug.LogWarning("No NPCs provided for DisplayBuildingInfo");
            return;
        }

        _buildingTitle.text = $"{building.BuildData.BuildingName} - Occupants: {building.GetOccupantsNumber()} - Tier {building.BuildData.BuildingTier}";
        _buildingTier.text = $"Tier {building.BuildData.BuildingTier}";
        _buildingUpgradeText.text = $"Upgrade? Cost: {building.BuildData.MaterialCost} Materials & {building.BuildData.PurrCost} Purr";
        _buildingUpgradeButton.onClick.AddListener(() => OnUpgradeButtonClick(building));

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

        _buildingUpgradeButton.onClick.RemoveAllListeners();

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
        DisplayBuildingInfo(_currentBuilding, _currentBuilding.GetNPCsInBuilding());
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
    private void OnSpawnSkeleton(Building building)
    {
        if (building is Graveyard graveyard)
        {
            graveyard.SpawnSkeleton();
        }
    }
    private void OnSpawnZombie(Building building)
    {
        if (building is Graveyard graveyard)
        {
            graveyard.SpawnZombie();
        }
    }
    public void DisplayGraveyardInfo(Building building)
    {
        HideAllInfo();
        _graveYardCanvasGroup.alpha = 1;
        _graveYardCanvasGroup.interactable = true;
        _graveYardCanvasGroup.blocksRaycasts = true;

        //_graveYardCanvasGroup.onClick.AddListener(() => OnUpgradeButtonClick(building));

        _graveYardTitle.text = $"Graveyard";
        _graveyardTier.text = $"Tier {building.BuildData.BuildingTier}";
        _graveYardUpgradeText.text = $"Upgrade? Requirements: {building.BuildData.MaterialCost} Materials & {building.BuildData.PurrCost} Purr";
        _graveyardUpgradeButton.onClick.AddListener(() => OnUpgradeButtonClick(building));


        _skeletonButton.onClick.AddListener(() => OnSpawnSkeleton(building));
        _zombieButton.onClick.AddListener(() => OnSpawnZombie(building));
    }
    public void HideGraveyardInfo()
    {
        _graveYardCanvasGroup.alpha = 0;
        _graveYardCanvasGroup.interactable = false;
        _graveYardCanvasGroup.blocksRaycasts = false;

        _graveyardUpgradeButton.onClick.RemoveAllListeners();
    }
    public void DisplayCastleInfo(Building building)
    {
        HideAllInfo();
        _castleCanvasGroup.alpha = 1;
        _castleCanvasGroup.interactable = true;
        _castleCanvasGroup.blocksRaycasts = true;

        //_castleCanvasGroup.onClick.AddListener(() => OnUpgradeButtonClick(building));

        _castleTier.text = $"Tier {building.BuildData.BuildingTier}";
        _castleRequirements.text = $"Requirements {building.BuildData.MaterialCost} Materials & {building.BuildData.PurrCost} Purr";
        _castleUpgradeButton.onClick.AddListener(() => OnUpgradeButtonClick(building));
    }
    public void HideCastleInfo()
    {
        _castleCanvasGroup.alpha = 0;
        _castleCanvasGroup.interactable = false;
        _castleCanvasGroup.blocksRaycasts = false;

        _castleUpgradeButton.onClick.RemoveAllListeners();
    }
    public void HideAllInfo()
    {
        HideGraveyardInfo();
        HideBuildingInfo();
        HideCastleInfo();
    }
    private void OnUpgradeButtonClick(Building building)
    {
        _buildingsManager.TryUpgradeBuilding(building);
    }
    public void TestNextEvent()
    {
        _eventManager.TestNextEvent();
    }

    public void TestStage2()
    {
        _eventManager.TestStage2();
    }

    private void OnNewEvent(Event_SO newEvent)
    {
        _eventToSolve = newEvent;
        var textElements = _eventUIPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in textElements)
        {
            if (text.name == "Title")
            {
                text.text = newEvent.Title;
            }
            else if (text.name == "Description")
            {
                text.text = newEvent.Description;
            }
            else if (text.name == "ChoiceText0")
            {
                text.text = newEvent.Choices[0].Description;
            }
            else if (text.name == "ChoiceText1")
            {
                text.text = newEvent.Choices[1].Description;
            }
            else if (text.name == "ChoiceText2")
            {
                text.text = newEvent.Choices[2].Description;
            }
        }
    }

    public void SolveEventOutcome(int choice)
    {
        if (!_eventToSolve)
        {
            Debug.Log("There is no triggered event");
            return;
        }

        var chosenEvent = _eventToSolve.Choices[choice];
        Debug.Log($"{chosenEvent.Outcome} {chosenEvent.OutcomeValue}");
        chosenEvent.SolveEncounter();
        _eventUIPrefab.SetActive(false);
    }

    public void TestGameOver(GameResult result, int lostCondition = 0)
    {
        switch (result)
        {
            case GameResult.GameWon:
                _gameEnderUIPrefab.SetActive(true);
                _gameEnderManager.GameWon();
                break;
            case GameResult.GameLost:
                _gameEnderUIPrefab.SetActive(true);
                _gameEnderManager.TestGameLost(lostCondition);
                break;
        }
    }

    private void GameOver(GameResult result, string message)
    {
        _gameEnderUIPrefab.SetActive(true);
        var textElements = _gameEnderUIPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in textElements)
        {
            if (text.name == "Title")
            {
                if (result == GameResult.GameWon)
                {
                    text.text = "YOU WIN";
                }
                else
                {
                    text.text = "YOU LOSE";
                }
            }
            else if (text.name == "Description")
            {
                text.text = message;
            }
        }
    }
}
