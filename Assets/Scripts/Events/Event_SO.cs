using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_SO", menuName = "Scriptable Objects/Event_SO")]
public class Event_SO : ScriptableObject
{
    public enum EventTier
    {
        Easy,
        Medium,
        Severe
    }

    public string Title;
    public string Description;
    public EventTier Tier;
    public List<EventChoices_SO> Choices;

    public List<EventChoices_SO> GetChoices() 
    {
        foreach (var choice in Choices)
        {
            switch (choice.Outcome)
            {
                case EventChoices_SO.OutcomeElement.Peasant:
                    switch (Tier)
                    {
                        case EventTier.Easy:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 1, choice.OutcomeValue + 1);
                            break;
                        case EventTier.Medium:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 3, choice.OutcomeValue + 3);
                            break;
                        case EventTier.Severe:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 6, choice.OutcomeValue + 6);
                            break;
                    }
                    break;
                case EventChoices_SO.OutcomeElement.Materials:
                case EventChoices_SO.OutcomeElement.FoodStock:
                    switch (Tier)
                    {
                        case EventTier.Easy:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 5, choice.OutcomeValue + 5);
                            break;
                        case EventTier.Medium:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 10, choice.OutcomeValue + 10);
                            break;
                        case EventTier.Severe:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 30, choice.OutcomeValue + 30);
                            break;
                    }
                    break;
                case EventChoices_SO.OutcomeElement.Purr:
                    switch (Tier)
                    {
                        case EventTier.Easy:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 10, choice.OutcomeValue + 10);
                            break;
                        case EventTier.Medium:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 30, choice.OutcomeValue + 30);
                            break;
                        case EventTier.Severe:
                            choice.OutcomeValue = Random.Range(choice.OutcomeValue - 50, choice.OutcomeValue + 50);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        return Choices;
    }
}
