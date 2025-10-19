using System;
using UnityEngine;
using UnityEngine.AI;

public class GoToZoneBehaviour
{
    private NPC _npc;
    private NavMeshAgent _agent;
    private Zone _targetZone;
    private Vector3 _targetPoint;
    public float MovementSpeed { get; }
    private float _timeSinceLastDestination;
    private const float DESTINATIONTIMEOUT = 1f;
    private const float MINVELOCITYTHRESHOLD = 0.5f;
    private const float ARRIVAL_THRESHOLD = 1f;

    public event Action<Zone> OnArrived;

    private bool _hasSetDestination;

    public GoToZoneBehaviour(NPC npc, float movementSpeed, Zone targetZone)
    {
        _npc = npc;
        _targetZone = targetZone;
        MovementSpeed = movementSpeed;
        _agent = npc.GetComponent<NavMeshAgent>();
        _agent.speed = movementSpeed;
        _timeSinceLastDestination = 0f;
        _hasSetDestination = false;
    }
    public void SetTargetZone(Zone newZone)
    {
        if (newZone != null)
        {
            _targetZone = newZone;
            _hasSetDestination = false;
            Debug.Log($"{_npc.name} now heading to zone: {newZone.Name}");
        }
    }
    public void GoToZone()
    {
        if (!_hasSetDestination)
        {
            _targetPoint = _targetZone.GetRandomPointInZone();
            _agent.SetDestination(_targetPoint);
            _hasSetDestination = true;
            _timeSinceLastDestination = 0f;
            Debug.Log($"{_npc.name} traveling to {_targetZone.Name}");
        }
        if (HasArrivedAtZone())
        {
            Debug.Log($"{_npc.name} arrived at {_targetZone.Name}");
            OnArrived?.Invoke(_targetZone);
            return;
        }
        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            HandleStuckNPC();
            return;
        }
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
    private bool HasArrivedAtZone()
    {
        if (_targetZone.ContainsPosition(_agent.transform.position))
        {
            return true;
        }
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + ARRIVAL_THRESHOLD)
        {
            return true;
        }

        return false;
    }
    private void HandleStuckNPC()
    {
        Debug.LogWarning($"{_npc.name} stuck while traveling to {_targetZone.Name}, retrying...");
        _agent.ResetPath();

        _targetPoint = _targetZone.GetRandomPointInZone();
        _agent.SetDestination(_targetPoint);
        _timeSinceLastDestination = 0f;
    }
}
