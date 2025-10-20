using UnityEngine;

public class EventManager : Manager
{
    private TimeManager _timeManager;

    private int _currentDay = 0;

    //Initialize gets called by GameManager on Awake
    public override void Initialize()
    {
        _currentDay = 0;

        _timeManager = GameManager.Instance.GetManager<TimeManager>();
        if( _timeManager != null)
        {
            _timeManager.OnCyclePassage += OnCyclePassed;
        }
    }
    private void OnDisable()
    {
        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage -= OnCyclePassed;
        }
    }
    private void OnCyclePassed(DayCycle currentCycle)
    {
        _currentDay = _timeManager.DayNumber;
    }
}
