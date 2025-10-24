using UnityEngine;
using UnityEngine.UIElements;

public abstract class NPC : MonoBehaviour
{
    public string Name {  get; protected set; }
    public int Age { get; protected set; }
    public Sprite PassportPhoto { get; protected set; }
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
        if (LifeSpan <= 0)
        {
            MarkedForDeath = true;
            ParticleSystemManager.Instance.Spawn("CatDie", transform.position);
        }
        
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
    public abstract void ResetOccupiedZone();
    public abstract Zone GetOccupiedZone();
    public Zone GetCurrentZone() => _currentZone;
    public ZoneType GetRestZoneType() => _restZoneType;
    public DayCycle GetActiveCycle() => _activeCycle;
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
    bool IsTraveling();
}
public interface IPeasant
{
    int StarvationValue { get; }
    int DreadFactor { get; }
    void GoToRest();
    void GatherPurr();
    Mood GetMood();
}
public enum Mood
{
    Neutral,
    Bad
}