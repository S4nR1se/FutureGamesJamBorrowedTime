using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/EventCatalog_SO")]
public class EventCatalog_SO : ScriptableObject
{
    public List<Event_SO> EasyEvents = new List<Event_SO>();
    public List<Event_SO> MediumEvents = new List<Event_SO>();
    public List<Event_SO> SevereEvents = new List<Event_SO>();

    private int _easyCount { get { return EasyEvents.Count; } }
    private int _lastEasyEvent;

    private int _mediumCount { get { return MediumEvents.Count; } }
    private int _lastMediumEvent;

    private int _severeCount { get { return SevereEvents.Count; } }
    private int _lastSevereEvent;

    public Event_SO GetRandomEasyEvent()
    {
        int nextIndex;
        do
        {
            nextIndex = UnityEngine.Random.Range(0, _easyCount - 1);
        } while (_lastEasyEvent == nextIndex);
        
        _lastEasyEvent = nextIndex;
        return EasyEvents[nextIndex];
    }

    public Event_SO GetRandomMediumEvent()
    {
        int nextIndex;
        do
        {
            nextIndex = UnityEngine.Random.Range(0, _mediumCount - 1);
        } while (_lastMediumEvent == nextIndex);

        _lastMediumEvent = nextIndex;
        return MediumEvents[nextIndex];
    }

    public Event_SO GetRandomSevereEvent()
    {
        int nextIndex;
        do
        {
            nextIndex = UnityEngine.Random.Range(0, _severeCount - 1);
        } while (_lastSevereEvent == nextIndex);

        _lastSevereEvent = nextIndex;
        return SevereEvents[nextIndex];
    }
}
