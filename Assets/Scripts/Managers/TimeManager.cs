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

    public override void Initialize()
    {
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

        if (Mathf.FloorToInt(LevelTime / CYCLEDURATION) > Mathf.FloorToInt((LevelTime - Time.deltaTime) / CYCLEDURATION))
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
    }
}

public enum DayCycle
{
    Day,
    Night
}