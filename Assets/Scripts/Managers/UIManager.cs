using System;
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
        public Image[] Icon;
        public TextMeshProUGUI[] Counter;
    }

    [SerializeField] private HudComponent[] _hudComponents;

    private Dictionary<string, HudComponent> _hudComponentsDic;

    //[SerializeField] private int _day = 0;
    //[SerializeField] private int _borrowTime = 0;
    //[SerializeField] private int _summoning = 0;
    //[SerializeField] private int _graves = 0;
    //[SerializeField] private int _peasants = 0;
    //[SerializeField] private int _resources = 0;
    //[SerializeField] private int _buildings = 0;
    //[SerializeField] private int _purr = 0;


    private ResourceManager _resourceManager;
    private NPCManager _npcManager;
    public override void Initialize()
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _npcManager = GameManager.Instance.GetManager<NPCManager>();

        if( _resourceManager != null)
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
                _hudComponents[i].Counter[j].text = "0";
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
        
        foreach (var resource in resources)
        {
            //hudComponentsDic["Resources"].Counter[i] = resource.Value;
        }
        for (int i = 0; i < resources.Count; ++i)
        {
           //resources.Keys

        }

    }
    private void OnNPCAmountChange(Dictionary<System.Type, List<NPC>> npcsByType)
    {

    }

    void SetHUDComponent(string Title, Dictionary<int, int> Counter1)
    {

    }
}
