using System.Collections.Generic;
using UnityEngine;

public class NPCScheduler : MonoBehaviour
{
    public static NPCScheduler Instance { get; private set; }

    private AudioSource DayAmbianceSource;
    private AudioSource NightAmbianceSource;

    private AudioSource DayMusicSource;
    private AudioSource NightMusicSource;

    private TimeManager _timeManager;
    private NPCManager _npcManager;
    private ZoneManager _zoneManager;

    private AudioSource _musicSound;
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
            SoundManager.Instance.PlaySound("DayToNightShiftBell");
            ScheduleNightTimeCalculations(); 
        }
        else
        {
            SoundManager.Instance.StopSound(NightAmbianceSource, 2f);
            SoundManager.Instance.StopSound(NightMusicSource, 2f);
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
                _musicSound = SoundManager.Instance.PlaySound("VilligerDeath");

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
