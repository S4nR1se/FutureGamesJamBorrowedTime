using UnityEngine;

[CreateAssetMenu(fileName = "EventChoices_SO", menuName = "Scriptable Objects/EventChoices_SO")]
public class EventChoices_SO : ScriptableObject
{
    public enum OutcomeElement
    {
        Peasant, 
        Building,
        Materials,
        FoodStock,
        Graves,
        Purr,
        Dread
    }

    public string Description;
    public OutcomeElement Outcome;
    public int OutcomeValue;
}
