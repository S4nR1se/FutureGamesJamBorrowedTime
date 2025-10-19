using UnityEngine;

public class Undead : NPC, IWorker, IPoolable
{
    public GameObject PoolableComponent => gameObject;

    public Occupation Occupation => _occupation;

    private Occupation _occupation;
    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;

    private bool _isTraveling = false;

    private Zone _reservedZone;
    private Zone _occupiedZone;

    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard)
    {
        Name = name;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        Zone startZone = GetCurrentZone() ?? GameManager.Instance.GetManager<ZoneManager>().GetClosestZone(transform.position);
        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);

        if (startZone != null && startZone.TryEnter())
        {
            _occupiedZone = startZone;
        }
    }
    private void Update()
    {
        if (_isTraveling)
        {
            Travel();
        }
        else
        {
            Roam();
        }
    }

    public void AssignOccupation(Occupation occupation)
    {
        _occupation = occupation;
    }

    public void Roam()
    {
        _roamingBehaviour?.Roam();
    }

    public void Travel()
    {
        _goToZoneBehaviour?.GoToZone();
    }
    private void GoToZone(ZoneType zoneType)
    {
        ReleaseReservation();

        Zone travelZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(zoneType);

        if (travelZone != null)
        {
            if (travelZone.TryEnter())
            {
                _reservedZone = travelZone;
                _goToZoneBehaviour = new GoToZoneBehaviour(this, MovementSpeed, travelZone);
                _goToZoneBehaviour.OnArrived += OnArrivedAtDestination;
                _isTraveling = true;
            }
            else
            {
                Debug.LogWarning($"{Name} couldn't reserve {travelZone.Name} - zone is full");
            }
        }
        else
        {
            Debug.LogWarning($"{Name} couldn't find available zone of type {zoneType}");
        }
    }

    [ContextMenu("Work")]
    public void GoToWork()
    {
        GoToZone(Occupation.WorkZoneType);
    }

    [ContextMenu("Rest")]
    public override void GoToRest()
    {
        GoToZone(_restZoneType);
    }

    private void OnArrivedAtDestination(Zone zone)
    {
        if (_occupiedZone != null && _occupiedZone != zone)
        {
            _occupiedZone.Exit();
        }

        _occupiedZone = _reservedZone;
        _reservedZone = null;

        _isTraveling = false;
        _roamingBehaviour.SetTargetZone(zone);
        _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
    }
    private void ReleaseReservation()
    {
        if (_reservedZone != null)
        {
            _reservedZone.Exit();
            _reservedZone = null;
        }
    }
    public void CancelTravel()
    {
        if (_isTraveling)
        {
            _isTraveling = false;
            ReleaseReservation();

            if (_goToZoneBehaviour != null)
            {
                _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
                _goToZoneBehaviour = null;
            }
        }
    }

    private void OnDestroy()
    {
        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit();
        }

        if (_goToZoneBehaviour != null)
        {
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
        }
    }

    private void OnDisable()
    {
        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit();
            Debug.Log($"{Name} disabled, exited {_occupiedZone.Name}");
            _occupiedZone = null;
        }
    }

    public void ReturnToPool(ObjectPool pool)
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit();
            _occupiedZone = null;
        }

        ClearCurrentZone();

        _isTraveling = false;
        if (_goToZoneBehaviour != null)
        {
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
            _goToZoneBehaviour = null;
        }

        pool.Release(this);
    }
}
