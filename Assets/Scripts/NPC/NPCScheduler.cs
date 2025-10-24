using System.Collections.Generic;
using UnityEngine;

public class NPCScheduler : MonoBehaviour
{
    public static NPCScheduler Instance { get; private set; }

    private TimeManager _timeManager;
    private NPCManager _npcManager;
    private ZoneManager _zoneManager;
    public void Initialize(NPCManager npcManager, TimeManager timeManager)
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        _npcManager = npcManager;
        _timeManager = timeManager;
        _zoneManager = GameManager.Instance.GetManager<ZoneManager>();

        _timeManager.OnCyclePassage += OnTimePassage;
    }
    private void OnDisable()
    {
        if(_timeManager != null)
        {
            _timeManager.OnCyclePassage -= OnTimePassage;
        }
    }
    private void OnTimePassage(DayCycle currentCycle)
    {
        if(currentCycle == DayCycle.Day)
        {
            ScheduleNightTimeCalculations(); 
        }
        else
        {
            ScheduleRest();
        }
        ScheduleWorkers(currentCycle);
    }
    private void ScheduleNightTimeCalculations()
    {
        List<NPC> activeNPCS = new List<NPC>(_npcManager.GetAllActiveNPC());
        foreach (NPC npc in activeNPCS)
        {
            npc.DecreaseLifeSpan(1);
            if(npc is Peasant peasant)
            {
                peasant.RunNightChecklist();
            }
            ScheduleDeath(npc);
        }
    }
    public void ScheduleDeath(NPC npc)
    {
        if (npc.MarkedForDeath)
        {
            npc.ResetOccupiedZone();
            if (npc is Peasant peasant)
            {
                _npcManager.DespawnPeasant(peasant);
                ParticleSystemManager.Instance.Spawn("CatDie", transform.position);
            }
            else if(npc is Undead undead)
            {
                _npcManager.DespawnUndead(undead);
            }
        }
    }
    private void ScheduleWorkers(DayCycle currentCycle)
    {
        List<Peasant> peasants = _npcManager.GetNPCsOfType<Peasant>();
        foreach(Peasant p in peasants)
        {
            if (p is IWorker worker)
            {
                worker.GoToWork(currentCycle);
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
