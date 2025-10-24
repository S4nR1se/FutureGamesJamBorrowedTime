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
        Purr,
        Dread
    }

    public string Description;
    public OutcomeElement Outcome;
    public int OutcomeValue;

    public void SolveEncounter()
    {
        switch (Outcome)
        {
            case OutcomeElement.Peasant:
                GameManager.Instance?.GetManager<NPCManager>().DespawnPeasantAtRandom(OutcomeValue);
                break;
            case OutcomeElement.Building:
                //Except Castle and Graveyard
                GameManager.Instance?.GetManager<BuildingsManagers>().DestroyBuildsAtRandom(OutcomeValue); //<----
                break;
            case OutcomeElement.Materials:
                GameManager.Instance?.GetManager<ResourceManager>().UpdateValue(Resources.Materials , -OutcomeValue);
                break;
            case OutcomeElement.FoodStock:
                GameManager.Instance?.GetManager<ResourceManager>().UpdateValue(Resources.FoodStock, -OutcomeValue);
                break;
            case OutcomeElement.Purr:
                GameManager.Instance?.GetManager<ResourceManager>().UpdateValue(Resources.Purr, -OutcomeValue);
                break;
            case OutcomeElement.Dread:
                GameManager.Instance?.GetManager<NPCManager>().IncreaseGlobalDread(OutcomeValue);
                break;
            default:
                break;
        }
    }
}
