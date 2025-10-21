using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : Manager
{
    private TimeManager _timeManager;
    private List<ScriptableObject> _easyEvents;
    private List<ScriptableObject> _mediumEvents;
    private List<ScriptableObject> _severeEvents;

    private int _currentDay = 0;
    private int _eventCount = 0;
    private int _tierOfGame = 1;

    //Initialize gets called by GameManager on Awake
    public override void Initialize()
    {
        _currentDay = 0;
        _eventCount = 0;

        _timeManager = GameManager.Instance.GetManager<TimeManager>();
        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage += OnCyclePassed;
        }

        //Event to update whenever player upgrades castle to lvl2;
        //_tierOfGame = GameManager.Instance.    ;
        //if (_tierOfGame != null)
        //{
        //    _tierOfGame.OnTierUpgrade += OnTierUpgrade;
        //}
    }
    private void OnDisable()
    {
        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage -= OnCyclePassed;
        }
        //if (_tierOfGame != null)
        //{
        //    _timeManager.OnTierUpgrade -= OnTierUpgrade();
        //}
    }
    private void OnCyclePassed(DayCycle currentCycle)
    {
        _currentDay = _timeManager.DayNumber;
        if (_currentDay > 0 && _currentDay % 5 == 0)
            GetNewEvent();
    }
    private void OnTierUpgrade()
    {
        _tierOfGame = 2;
        _eventCount = 0;
    }
    private void GetNewEvent()
    {
        ScriptableObject nextEvent;
        if (_tierOfGame == 1)
        {
            if (_easyEvents != null && !_easyEvents.Any() && _mediumEvents != null && !_mediumEvents.Any())
            {
                GenerateEasyEvents();
                GenerateMediumEvents();
            }
            if (_eventCount > 0)
            {
                int rnd = Random.Range(0, 99);

                if (_easyEvents.Any() && rnd > 30)
                    nextEvent = GetRandomEvent(_easyEvents);
                else
                    nextEvent = GetRandomEvent(_mediumEvents);
            }
            else
            {
                GenerateEasyEvents();
                GenerateMediumEvents();
                nextEvent = GetRandomEvent(_easyEvents);
            }
        }
        else
        {
            if (_easyEvents != null && !_easyEvents.Any() && _mediumEvents != null && !_mediumEvents.Any() && _severeEvents != null && !_severeEvents.Any())
            {
                GenerateEasyEvents();
                GenerateMediumEvents();
                GenerateSevereEvents();
            }
            if (_eventCount > 0)
            {
                int rnd = Random.Range(0, 99);

                if (_easyEvents.Any() && rnd < 10)
                    nextEvent = GetRandomEvent(_easyEvents);
                else if (_mediumEvents.Any() && rnd > 55)
                    nextEvent = GetRandomEvent(_mediumEvents);
                else
                    nextEvent = GetRandomEvent(_severeEvents);
            }
            else
            {
                GenerateEasyEvents();
                GenerateMediumEvents();
                GenerateSevereEvents();
                nextEvent = GetRandomEvent(_mediumEvents);
            }
        }
        _eventCount++;

        //Update resources and changes based on nextEvent
    }

    private ScriptableObject GetRandomEvent(List<ScriptableObject> list)
    {
        int rnd = Random.Range(0, list.Count - 1);
        var nextEvent = list[rnd];
        list.RemoveAt(rnd);
        return nextEvent;
    }
    private void GenerateEasyEvents()
    {
        _easyEvents = new List<ScriptableObject>();

        _easyEvents.Add(ScriptableObject.CreateInstance("EasyEvent0"));
        _easyEvents.Add(ScriptableObject.CreateInstance("EasyEvent1"));
        _easyEvents.Add(ScriptableObject.CreateInstance("EasyEvent2"));
        _easyEvents.Add(ScriptableObject.CreateInstance("EasyEvent3"));
        _easyEvents.Add(ScriptableObject.CreateInstance("EasyEvent4"));
    }
    private void GenerateMediumEvents()
    {
        if (_mediumEvents == null)
            _mediumEvents = new List<ScriptableObject>();

        _mediumEvents.Add(ScriptableObject.CreateInstance("MediumEvent0"));
        _mediumEvents.Add(ScriptableObject.CreateInstance("MediumEvent1"));
        _mediumEvents.Add(ScriptableObject.CreateInstance("MediumEvent2"));
        _mediumEvents.Add(ScriptableObject.CreateInstance("MediumEvent3"));
        _mediumEvents.Add(ScriptableObject.CreateInstance("MediumEvent4"));
    }
    private void GenerateSevereEvents()
    {
        if (_severeEvents == null)
            _severeEvents = new List<ScriptableObject>();

        _severeEvents.Add(ScriptableObject.CreateInstance("SevereEvent0"));
        _severeEvents.Add(ScriptableObject.CreateInstance("SevereEvent1"));
        _severeEvents.Add(ScriptableObject.CreateInstance("SevereEvent2"));
        _severeEvents.Add(ScriptableObject.CreateInstance("SevereEvent3"));
        _severeEvents.Add(ScriptableObject.CreateInstance("SevereEvent4"));
    }
}
