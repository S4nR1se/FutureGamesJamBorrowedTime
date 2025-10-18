using UnityEngine;

public class Peasant : NPC, IWorker
{
    public Occupation Occupation => _occupation;
    private Occupation _occupation;

    private RoamingBehaviour _roamingBehaviour;

    private void Awake()
    {
        _roamingBehaviour = new RoamingBehaviour(this, 3);
    }
    private void Update()
    {
        Roam();
    }
    public void Roam()
    {
        _roamingBehaviour.Roam();
    }
    public void AssignOccupation(Occupation occupation)
    {
        _occupation = occupation;
    }
}
