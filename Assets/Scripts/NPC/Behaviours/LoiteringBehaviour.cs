using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class LoiteringBehaviour : Behaviour
{
    private NPC _npc;
    private NavMeshAgent _agent;

    private TimeManager _timeManager;

    public float MovementSpeed { get; }
    private float _timeSinceLastDestination;

    private const float DESTINATIONTIMEOUT = 0.5f;
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

        _timeSinceLastDestination = 0f;
    }

    public void Loiter()
    {
        if (_npc is Peasant && _timeManager != null && _timeManager.CurrentDayCycle != _npc.GetActiveCycle())
        {
            Zone currentZone = _npc.GetCurrentZone();
            ZoneType restType = _npc.GetRestZoneType();

            if ((currentZone == null || currentZone.Type != restType) && _npc is IWorker worker)
            {
                if (!worker.IsTraveling())
                {
                    Zone restZone = GameManager.Instance.GetManager<ZoneManager>()
                                        .GetRandomAvailableZone(restType);

                    if (restZone != null)
                    {
                        worker.TravelToZone(restZone);
                        return;
                    }
                }
            }
        }

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            HandleStuckNPC();
            return;
        }

        if (!_agent.pathPending && (_agent.remainingDistance <= _agent.stoppingDistance || _agent.velocity.magnitude < MINVELOCITYTHRESHOLD))
        {
            _timeSinceLastDestination += Time.deltaTime;
            if (_timeSinceLastDestination > DESTINATIONTIMEOUT)
            {
                HandleStuckNPC();
            }
        }
    }

    private void HandleStuckNPC()
    {
        _agent.ResetPath();
        Vector3 randomPoint = GetRandomPoint();
        _agent.SetDestination(randomPoint);

        _agent.speed = MovementSpeed * Random.Range(0.7f, 1.2f);

        _timeSinceLastDestination = 0f;
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
