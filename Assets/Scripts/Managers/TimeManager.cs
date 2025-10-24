using System;
using UnityEngine;

public class TimeManager : Manager
{
    public event Action OnCycleCalculation;
    public event Action<DayCycle> OnCyclePassage;
    public DayCycle CurrentDayCycle { get; private set; } = DayCycle.Day;

    private SunTransitioner _sunTransitioner;

    public int DayNumber { get; private set; }
    public float LevelTime { get; private set; }

    private const float CYCLEDURATION = 60f;

    private float _cycleTimer = 0;
    private bool _isCalculatingCycle = false;

    public override void Initialize()
    {
        _cycleTimer = 0;
        LevelTime = 0;
        DayNumber = 0;
        CurrentDayCycle = DayCycle.Day;

        _sunTransitioner = GetComponent<SunTransitioner>();
        if(_sunTransitioner != null)
        {
            _sunTransitioner.InitializeLighting(CurrentDayCycle);
        }
    }

    private void OnEnable()
    {
        PlayingState.OnPlayingStateUpdate += UpdateComponent;
    }

    private void OnDisable()
    {
        PlayingState.OnPlayingStateUpdate -= UpdateComponent;
    }

    private void UpdateComponent()
    {
        LevelTime += Time.deltaTime;
        _cycleTimer += Time.deltaTime;

        if (_cycleTimer >= CYCLEDURATION)
        {
            PassTime();
        }
    }

    [ContextMenu("PassTime")]
    public void PassTime()
    {
        _isCalculatingCycle = true;

        OnCycleCalculation?.Invoke();

        CurrentDayCycle = (CurrentDayCycle == DayCycle.Day) ? DayCycle.Night : DayCycle.Day;
        if (CurrentDayCycle == DayCycle.Day) DayNumber++;

        Debug.Log($"[TimeManager] Cycle changed to {CurrentDayCycle}, Day: {DayNumber}");
        OnCyclePassage?.Invoke(CurrentDayCycle);

        if (_sunTransitioner != null)
        {
            _sunTransitioner.TransitionToCycle(CurrentDayCycle);
        }

        _cycleTimer = 0;
        _isCalculatingCycle = false;
    }

    public DayCycle GetCurrentCycle()
    {
        return CurrentDayCycle;
    }

    public bool IsCalculatingCycle()
    {
        return _isCalculatingCycle;
    }
}

public enum DayCycle
{
    Day,
    Night
}