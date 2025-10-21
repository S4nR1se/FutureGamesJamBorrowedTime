using System;
using UnityEngine;

public class TimeManager : Manager
{
    public event Action OnCycleCalculation;
    public event Action<DayCycle> OnCyclePassage;
    public DayCycle CurrentDayCycle { get; private set; } = DayCycle.Night;

    public int DayNumber {  get; private set; }
    public float LevelTime { get; private set; }

    private const float CYCLEDURATION = 60f;
    private float _cycleTimer = 0;

    public override void Initialize()
    {
        _cycleTimer = 0;
        LevelTime = 0;
        DayNumber = 0;
        CurrentDayCycle = DayCycle.Night;
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
        OnCycleCalculation?.Invoke();

        CurrentDayCycle = (CurrentDayCycle == DayCycle.Day) ? DayCycle.Night : DayCycle.Day;
        if (CurrentDayCycle == DayCycle.Day) DayNumber++;
        OnCyclePassage?.Invoke(CurrentDayCycle);

        _cycleTimer = 0;
    }
}

public enum DayCycle
{
    Day,
    Night
}