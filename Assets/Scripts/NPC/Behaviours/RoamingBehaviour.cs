using UnityEngine;
using UnityEngine.AI;

public class RoamingBehaviour
{
    private NPC _npc;
    private NavMeshAgent _agent;

    private Vector3 _roamCenter;

    public int MovementSpeed { get; }

    private float _roamRadius;

    private float _timeSinceLastDestination;

    private const float DESTINATIONTIMEOUT = 1f;
    private const float MINVELOCITYTHRESHOLD = 0.5f;
    private const float RADIUSPERCENTAGE = 0.9f;

    public RoamingBehaviour(NPC npc, int movementSpeed)
    {
        _npc = npc;
        MovementSpeed = movementSpeed;

        _agent = npc.GetComponent<NavMeshAgent>();
        _agent.speed = movementSpeed;

        _roamCenter = CalculateNavMeshCenter();
        _roamRadius = CalculateRoamRadius(RADIUSPERCENTAGE);
        _timeSinceLastDestination = 0f;
    }
    private Vector3 CalculateNavMeshCenter()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        if (triangulation.vertices.Length == 0)
        {
            Debug.LogWarning("No NavMesh found");
            return _agent.transform.position;
        }

        Bounds bounds = new Bounds(triangulation.vertices[0], Vector3.zero);
        foreach (Vector3 vertex in triangulation.vertices)
        {
            bounds.Encapsulate(vertex);
        }

        return bounds.center;
    }
    private float CalculateRoamRadius(float percent)
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            return 10f;
        }

        Bounds bounds = new Bounds(triangulation.vertices[0], Vector3.zero);
        foreach (Vector3 vertex in triangulation.vertices)
        {
            bounds.Encapsulate(vertex);
        }

        float minExtent = Mathf.Min(bounds.extents.x, bounds.extents.z);
        return minExtent * percent;
    }
    public void Roam()
    {
        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            HandleStuckNPC();
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            Vector3 randomPoint = GetRandomNavMeshPoint();
            _agent.SetDestination(randomPoint);
            _timeSinceLastDestination = 0f;
        }
        else
        {
            if (_agent.velocity.magnitude < MINVELOCITYTHRESHOLD)
            {
                _timeSinceLastDestination += Time.deltaTime;

                if (_timeSinceLastDestination > DESTINATIONTIMEOUT)
                {
                    Debug.LogWarning($"NPC {_npc.name} stuck (zero velocity), finding new path");
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

        Vector3 randomPoint = GetRandomNavMeshPoint();
        _agent.SetDestination(randomPoint);
        _timeSinceLastDestination = 0f;
    }
    private Vector3 GetRandomNavMeshPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * _roamRadius;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
        randomDirection += _roamCenter;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, _roamRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return _roamCenter;
    }
}
