using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public string Name {  get; protected set; }
    public int LifeSpan { get; protected set; }
    public float MovementSpeed { get; protected set; }

    private Zone _currentZone;

    protected ZoneType _restZoneType;

    protected DayCycle _activeCycle;

    public abstract void Initialize(string name, int lifeSpan, float movementSpeed, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day);
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
    void GoToWork();
}
public interface IPeasant
{
    int Age { get; }
    int StarvationValue { get; }
    int DreadFactor { get; }
    void GoToRest();
}