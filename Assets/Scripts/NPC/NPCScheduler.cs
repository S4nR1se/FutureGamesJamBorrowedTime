using System.Collections.Generic;
using UnityEngine;

public class NPCScheduler : MonoBehaviour
{
    private TimeManager _timeManager;
    private NPCManager _npcManager;
    public void Initialize(NPCManager npcManager, TimeManager timeManager)
    {
        _npcManager = npcManager;
        _timeManager = timeManager;

        _timeManager.OnTimePassage += OnTimePassage;
    }
    private void OnEnable()
    {
        if(_timeManager != null)
        {
            _timeManager.OnTimePassage += OnTimePassage;   
        }
    }
    private void OnDisable()
    {
        if(_timeManager != null)
        {
            _timeManager.OnTimePassage -= OnTimePassage;
        }
    }
    private void OnTimePassage(DayCycle currentCycle)
    {
        if(currentCycle == DayCycle.Day)
        {
            ScheduleWorkers();
        }
        else
        {
            ScheduleRest();
        }
    }
    private void ScheduleWorkers()
    {
        List<Peasant> peasants = _npcManager.GetNPCsOfType<Peasant>();
        foreach(Peasant p in peasants)
        {
            if (p is IWorker worker)
            {
                worker.GoToWork();
            }
        }
    }
    private void ScheduleRest()
    {
        List<Peasant> peasants = _npcManager.GetNPCsOfType<Peasant>();
        foreach (Peasant p in peasants)
        {
            p.GoToRest();
        }
    }
}
