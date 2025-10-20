using UnityEngine;

public abstract class Undead : NPC, IWorker, IPoolable
{
    public GameObject PoolableComponent => gameObject;

    public Occupation Occupation => _occupation;

    private Occupation _occupation;
    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;

    private bool _isTraveling = false;

    private Zone _reservedZone;
    private Zone _occupiedZone;

    private TravelPurpose _travelPurpose = TravelPurpose.None;

    private void OnEnable()
    {
        PlayingState.OnPlayingStateUpdate += UpdateComponent;
    }
    private void OnDisable()
    {
        PlayingState.OnPlayingStateUpdate -= UpdateComponent;

        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
        }
    }
    public abstract int GetPurrCost();
    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        Name = name;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        Zone startZone = GetCurrentZone() ?? GameManager.Instance.GetManager<ZoneManager>().GetClosestZone(transform.position);
        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);

        if (startZone != null && startZone.TryEnter(this))
        {
            _occupiedZone = startZone;
        }
    }
    private void UpdateComponent()
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

        if (_isTraveling && _travelPurpose == TravelPurpose.Work)
        {
            CancelTravel(); 
        }
        GoToWork(_activeCycle);
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
            if (travelZone.TryEnter(this))
            {
                _reservedZone = travelZone;
                _goToZoneBehaviour = new GoToZoneBehaviour(this, MovementSpeed, travelZone);
                _goToZoneBehaviour.OnArrived += OnArrivedAtDestination;
                _isTraveling = true;
            }
        }
    }

    [ContextMenu("Work")]
    public void GoToWork(DayCycle currentCycle)
    {
        //Ignores Cycles Simply works
        GoToZone(Occupation.WorkZoneType);
        _travelPurpose = TravelPurpose.Work;
    }

    private void OnArrivedAtDestination(Zone zone)
    {
        if (_occupiedZone != null && _occupiedZone != zone)
        {
            _occupiedZone.Exit(this);
        }

        _occupiedZone = _reservedZone;
        _reservedZone = null;

        _isTraveling = false;
        _roamingBehaviour.SetTargetZone(zone);
        _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;

        _travelPurpose = TravelPurpose.None;
    }
    private void ReleaseReservation()
    {
        if (_reservedZone != null)
        {
            _reservedZone.Exit(this);
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
            _occupiedZone.Exit(this);
        }

        if (_goToZoneBehaviour != null)
        {
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
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
            _occupiedZone.Exit(this);
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
