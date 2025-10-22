using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public string Name {  get; protected set; }
    public int LifeSpan { get; protected set; }
    public float MovementSpeed { get; protected set; }
    public bool MarkedForDeath { get; protected set; } = false;

    protected Zone _currentZone;

    protected ZoneType _restZoneType;

    protected DayCycle _activeCycle;

    public abstract void Initialize(Zone startingZone,string name, int lifeSpan, float movementSpeed, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day);
    public void DecreaseLifeSpan(int amount)
    {
        if (LifeSpan <= 0) return;

        LifeSpan -= amount;
        if (LifeSpan <= 0) MarkedForDeath = true;
    }
    protected Occupation CreateDefaultOccupation()
    {
        return new FarmerOccupation();
    }
    public void SetCurrentZone(Zone zone)
    {
        _currentZone = zone;
    }
    public void SetRestZoneType(ZoneType restZoneType)
    {
        _restZoneType = restZoneType;
    }
    public void ClearCurrentZone()
    {
        _currentZone = null;
    }
    public Zone GetCurrentZone() => _currentZone;
}
public enum TravelPurpose
{
    None,
    Work,
    Rest,
    Other
}
public interface IWorker
{
    Occupation Occupation { get;}
    void AssignOccupation(Occupation occupation);
    void GoToWork(DayCycle currentCycle);
    void TravelToZone(Zone travelZone);
}
public interface IPeasant
{
    int Age { get; }
    int StarvationValue { get; }
    int DreadFactor { get; }
    void GoToRest();
    void GatherPurr();
    
}
public enum Mood
{
    Neutral,
    Bad
}