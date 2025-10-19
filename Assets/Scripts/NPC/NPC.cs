using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public string Name {  get; protected set; }
    public int LifeSpan { get; protected set; }
    public float MovementSpeed { get; protected set; }

    private Zone _currentZone;

    protected ZoneType _restZoneType;

    public abstract void Initialize(string name, int lifeSpan, float movementSpeed, Occupation occupation = null, ZoneType restZoneType = ZoneType.House);
    protected Occupation CreateDefaultOccupation()
    {
        return new FarmerOccupation(ZoneType.Farm);
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
    public abstract void GoToRest();
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
}