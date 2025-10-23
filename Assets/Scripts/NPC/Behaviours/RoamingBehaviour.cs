using UnityEngine;
using UnityEngine.AI;

public class RoamingBehaviour
{
    private NPC _npc;
    private NavMeshAgent _agent;
    private Zone _targetZone;
    public float MovementSpeed { get; }
    private float _timeSinceLastDestination;
    private const float DESTINATIONTIMEOUT = 1f;
    private const float MINVELOCITYTHRESHOLD = 0.5f;

    public RoamingBehaviour(NPC npc, float movementSpeed, Zone targetZone)
    {
        _npc = npc;
        _targetZone = targetZone;

        MovementSpeed = movementSpeed;
        _agent = npc.GetComponent<NavMeshAgent>();
        _agent.speed = movementSpeed;

        _timeSinceLastDestination = 0f;
    }
    public void SetTargetZone(Zone newZone)
    {
        if (newZone != null)
        {
            _targetZone = newZone;
        }
    }
    public void Roam()
    {
        if (_targetZone == null)
        {
            Debug.LogWarning($"{_npc.name} has no target zone to roam in!");
            return;
        }

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            HandleStuckNPC();
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            Vector3 destinationPoint = _targetZone.Type == ZoneType.ConstructionSite ? GetRandomPointInZone() : GetCenterZone();
            _agent.SetDestination(destinationPoint);
            _timeSinceLastDestination = 0f;
        }
        else
        {
            if (_agent.velocity.magnitude < MINVELOCITYTHRESHOLD)
            {
                _timeSinceLastDestination += Time.deltaTime;
                if (_timeSinceLastDestination > DESTINATIONTIMEOUT)
                {
                    HandleStuckNPC();
                }
            }
            else
            {
                _timeSinceLastDestination = 0f;
            }
        }
    }
    private void HandleStuckNPC()
    {
        _agent.ResetPath();
        Vector3 destinationPoint = _targetZone.Type == ZoneType.ConstructionSite ? GetRandomPointInZone() : GetCenterZone();
        _agent.SetDestination(destinationPoint);
        _timeSinceLastDestination = 0f;
    }
    private Vector3 GetRandomPointInZone()
    {
        if (_targetZone == null)
        {
            return _agent.transform.position;
        }
        return _targetZone.GetRandomPointInZone();
    }
    private Vector3 GetCenterZone()
    {
        if( _targetZone == null)
        {
            return _agent.transform.position;
        }
        return _targetZone.GetCenter();
    }
}
