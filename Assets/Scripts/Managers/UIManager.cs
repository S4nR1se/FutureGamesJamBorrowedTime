using Assets.Scripts.Managers;
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
    [SerializeField] private GameObject      _buildingWindow;
    [SerializeField] private Transform posP;
    [SerializeField] private GameObject _eventUIPrefab;
    [SerializeField] private GameObject _gameEnderUIPrefab;
    [SerializeField] private Button _gameOver;

    private Dictionary<string, HudComponent> _hudComponentsDic;
    private List<GameObject> _buildingNPCInfoList = new();
    private List<TextMeshProUGUI> _borrowTimeText;
    private int _borrowCount = 5;
    private Event_SO _eventToSolve;
    private GameObject _event;
    private GameObject _gameEnder;

    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    private TilePlacementManager _buildingsManager;
    private EventManager _eventManager;
    private GameEnderManager _gameEnderManager;

    public override void Initialize()
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _npcManager = GameManager.Instance.GetManager<NPCManager>();
        _buildingsManager = GameManager.Instance.GetManager<TilePlacementManager>();
        _eventManager = GameManager.Instance.GetManager<EventManager>();
        _gameEnderManager = GameManager.Instance.GetManager<GameEnderManager>();

        if ( _resourceManager != null)
        {
            _resourceManager.OnResourceChange += OnResourceChange;
        }
        if(_npcManager != null)
        {
            _npcManager.OnNPCAmountChange += OnNPCAmountChange;
        }
        if(_eventManager != null)
        {
            _eventManager.OnNewEvent += OnNewEvent;
        }
        if(_gameEnderManager != null)
        {
            _gameEnderManager.GameOver += GameOver;
        }

        _hudComponentsDic = new();
        InitializeHudComponentsCounter();
        _npcInfo.SetActive(false);
        _buildingWindow.SetActive(false);
        _borrowTimeText = new();
        _eventUIPrefab.SetActive(false);
        _gameOver.onClick.AddListener(delegate { TestGameOver(GameResult.GameWon, 2); });
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
        _buildingWindow.SetActive(true);
 
        for(int i = 0; i < 20; ++i)
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
        _buildingWindow.SetActive(false);
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
            _borrowTimeText[i].text = _borrowCount.ToString();
        }

    }

    public void TestOpenEventUI()
    {
        _eventUIPrefab.SetActive(true);
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
        if(!_eventToSolve)
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

    public void buttonClick()
    {
        Debug.Log("HI");
    }
}
