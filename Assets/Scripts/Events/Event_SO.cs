using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_SO", menuName = "Scriptable Objects/Event_SO")]
public class Event_SO : ScriptableObject
{
    public string Title;
    public string Description;
    public List<EventChoices_SO> Choices;
}
