using UnityEngine;

public abstract class NPC : MonoBehaviour
{
    public string Name {  get; protected set; }
    public int LifeSpan { get; protected set; }
}

public interface IWorker
{
    abstract Occupation Occupation { get;}
}