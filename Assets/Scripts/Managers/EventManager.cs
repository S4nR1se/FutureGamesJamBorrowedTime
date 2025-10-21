using System;
using UnityEngine;

public class EventManager : Manager
{
    private TimeManager _timeManager;
    private EventCatalog_SO _eventCatalog;

    private bool _firstEvent;
    private int _currentDay = 0;
    private int _tierOfGame = 1;

    public delegate void GetNewEventHandler(Event_SO newEvent);
    public event GetNewEventHandler OnNewEvent;

    //Initialize gets called by GameManager on Awake
    public override void Initialize()
    {
        _currentDay = 0;
        _firstEvent = true;

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
        {
            var nextEvent = GetNewEvent();
            OnNewEvent?.Invoke(nextEvent);
        }
    }
    private void OnTierUpgrade()
    {
        _tierOfGame = 2;
        _firstEvent = true;
    }
    private Event_SO GetNewEvent()
    {
        Event_SO nextEvent;
        if (_tierOfGame == 1)
        {
            if (_firstEvent)
                nextEvent = _eventCatalog.GetRandomEasyEvent();
            else
            {
                int rnd = UnityEngine.Random.Range(0, 99);

                if (rnd > 30)
                    nextEvent = _eventCatalog.GetRandomEasyEvent();
                else
                    nextEvent = _eventCatalog.GetRandomMediumEvent();
            }
        }
        else
        {
            if (_firstEvent)
                nextEvent = _eventCatalog.GetRandomMediumEvent();
            else
            {
                int rnd = UnityEngine.Random.Range(0, 99);

                if (rnd < 10)
                    nextEvent = _eventCatalog.GetRandomEasyEvent();
                else if (rnd > 55)
                    nextEvent = _eventCatalog.GetRandomMediumEvent();
                else
                    nextEvent = _eventCatalog.GetRandomSevereEvent();
            }
        }
        return nextEvent;
    }
}
