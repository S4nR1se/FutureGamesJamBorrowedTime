using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LoiteringBehaviour : Behaviour
{
    private NPC _npc;
    private NavMeshAgent _agent;

    private TimeManager _timeManager;

    public float MovementSpeed { get; }

    private const float MINVELOCITYTHRESHOLD = 0.2f;
    private const int MAX_RANDOM_POINT_ATTEMPTS = 20;
    private const float ROAM_RADIUS = 20f;

    public LoiteringBehaviour(NPC npc, float movementSpeed)
    {
        _npc = npc;
        MovementSpeed = movementSpeed;

        _timeManager = GameManager.Instance.GetManager<TimeManager>();

        _agent = npc.GetComponent<NavMeshAgent>();
        _agent.speed = movementSpeed;
        _agent.acceleration = 10f;
        _agent.angularSpeed = 360f;
    }

    public void Loiter()
    {
        if (_npc is Peasant peasant && _timeManager != null && _timeManager.CurrentDayCycle != _npc.GetActiveCycle())
        {
            Zone currentZone = peasant.GetOccupiedZone();
            ZoneType restType = peasant.GetRestZoneType();
            Zone restZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(restType);

            if (restZone != null && currentZone != restZone)
            {
                peasant.TravelToZone(restZone);

                if (_agent.hasPath)
                    _agent.ResetPath();
                return;
            }
        }

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid ||
            (!_agent.pathPending && (_agent.remainingDistance <= _agent.stoppingDistance || _agent.velocity.magnitude < MINVELOCITYTHRESHOLD)))
        {
            HandleStuckNPC();
        }
    }

    private void HandleStuckNPC()
    {
        _agent.ResetPath();
        Vector3 randomPoint = GetRandomPoint();
        _agent.SetDestination(randomPoint);

        _agent.speed = MovementSpeed * Random.Range(0.7f, 1.2f);
    }

    private Vector3 GetRandomPoint()
    {
        Vector3 basePosition = _npc.transform.position;

        for (int i = 0; i < MAX_RANDOM_POINT_ATTEMPTS; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * ROAM_RADIUS;
            randomDirection += basePosition;
            randomDirection.y = basePosition.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, ROAM_RADIUS, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return _npc.transform.position;
    }
}
