using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public string Name {  get; protected set; }
    public int LifeSpan { get; protected set; }
    public float MovementSpeed { get; protected set; }

    private Zone _currentZone;

    public abstract void Initialize(string name, int lifeSpan, float movementSpeed);
    public void SetCurrentZone(Zone zone)
    {
        _currentZone = zone;
    }
    public void ClearCurrentZone()
    {
        _currentZone = null;
    }
    public Zone GetCurrentZone() => _currentZone;
}

public interface IWorker
{
    abstract Occupation Occupation { get;}
    void AssignOccupation(Occupation occupation);
}