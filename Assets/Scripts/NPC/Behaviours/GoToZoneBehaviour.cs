using UnityEngine;
using UnityEngine.AI;

public class GoToZoneBehaviour
{
    private NPC _npc;
    private NavMeshAgent _agent;
    public int MovementSpeed {  get;}

    private const float DESTINATIONTIMEOUT = 1f;
    private const float MINVELOCITYTHRESHOLD = 0.5f;
}
