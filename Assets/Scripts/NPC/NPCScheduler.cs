using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NPCScheduler : MonoBehaviour
{

    private AudioSource DayAmbianceSource;
    private AudioSource NightAmbianceSource;

    private AudioSource DayMusicSource;
    private AudioSource NightMusicSource;

    private TimeManager _timeManager;
    private NPCManager _npcManager;
    public void Initialize(NPCManager npcManager, TimeManager timeManager)
    {
        _npcManager = npcManager;
        _timeManager = timeManager;

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
        SoundManager.Instance.PlaySound("DayNightCycleTransition", transform.position);

        if (currentCycle == DayCycle.Day)
        {
            
            if (DayAmbianceSource == null)
                DayAmbianceSource = SoundManager.Instance.PlaySound("DayAmbience", transform.position);
            else if (!DayAmbianceSource.isPlaying)
                DayAmbianceSource.Play();
            
            if (DayMusicSource == null)
                DayMusicSource = SoundManager.Instance.PlaySound("DayMusic", transform.position);
            else if (!DayMusicSource.isPlaying)
                DayMusicSource.Play();

            SoundManager.Instance.StopSound(NightAmbianceSource, 2f);
            SoundManager.Instance.StopSound(NightMusicSource, 2f);

            ScheduleNightTimeCalculations(); 
        }
        else
        {
            if (NightAmbianceSource == null)
                NightAmbianceSource = SoundManager.Instance.PlaySound("NightAmbience", transform.position);
            else if (!NightAmbianceSource.isPlaying)
                NightAmbianceSource.Play();

            if (NightMusicSource == null)
                NightMusicSource = SoundManager.Instance.PlaySound("NightMusic", transform.position);
            else if (!NightMusicSource.isPlaying)
                NightMusicSource.Play();

            SoundManager.Instance.StopSound(DayAmbianceSource, 2f);
            SoundManager.Instance.StopSound(DayMusicSource, 2f);

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
                SoundManager.Instance.PlaySound("VilligerDeath", peasant.transform.position);
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
