using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCScheduler : MonoBehaviour
{
    public AudioClip DeathPeasantSFX;
    public AudioClip DayAmbianceSFX;
    public AudioClip NightAmbianceSFX;
    public AudioSource DayAmbianceSource;
    public AudioSource NightAmbianceSource;

    private TimeManager _timeManager;
    private NPCManager _npcManager;
    private SoundManager _soundManager;
    public void Initialize(NPCManager npcManager, TimeManager timeManager)
    {
        _npcManager = npcManager;
        _timeManager = timeManager;
        _soundManager = GameManager.Instance.GetManager<SoundManager>();

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
            if (DayAmbianceSource == null)
                DayAmbianceSource = _soundManager.PlayLoopingSound(DayAmbianceSFX, this.transform.position);
            else
                DayAmbianceSource.Play();

            _soundManager.StopSound(NightAmbianceSource, 1);
            ScheduleNightTimeCalculations(); 
        }
        else
        {
            if (NightAmbianceSource == null)
                NightAmbianceSource = _soundManager.PlaySoundEffect(NightAmbianceSFX, this.transform.position);
            else
                NightAmbianceSource.Play();

            _soundManager.StopSound(DayAmbianceSource, 1);
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
    private void ScheduleDeath(NPC npc)
    {
        if (npc.MarkedForDeath)
        {
            if(npc is Peasant peasant)
            {
                _soundManager.PlaySoundEffect(DeathPeasantSFX, peasant.transform.position);
                _npcManager.DespawnPeasant(peasant);
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
