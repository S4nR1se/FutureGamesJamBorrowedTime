using UnityEngine;

public class TimeManager : Manager
{
    public DayCycle CurrentDayCycle { get; private set; } = DayCycle.Day;
    public float LevelTime { get; private set; }
    private const float CYCLEDURATION = 60f;

    public override void Initialize()
    {
        LevelTime = 0;
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
            CurrentDayCycle = (CurrentDayCycle == DayCycle.Day) ? DayCycle.Night : DayCycle.Day;
        }
    }
}

public enum DayCycle
{
    Day,
    Night
}