using UnityEngine;

public class Peasant : NPC, IWorker
{
    private readonly Occupation _occupation;
    Occupation IWorker.Occupation => _occupation;

    public Peasant()
    {
        _occupation = new FarmerOccupation();
    }
}
