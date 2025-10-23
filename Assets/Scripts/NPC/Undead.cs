using System.Linq;
using UnityEngine;

public abstract class Undead : NPC, IWorker, IPoolable, IInteractable
{
    public GameObject PoolableComponent => gameObject;
    public GameObject Component => gameObject;

    private Renderer _meshRenderer;

    public Occupation Occupation => _occupation;

    private Occupation _occupation;
    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;
    private PlayerInteractionBehaviour _playerInteractionBehaviour;
    private LoiteringBehaviour _loiteringBehaviour;

    internal const int ZERO = 0;

    private bool _isTraveling = false;

    private Zone _reservedZone;
    private Zone _occupiedZone;
    private Zone _preferredZone;

    private TimeManager _timeManager;
    internal ResourceManager _resourceManager;
    private ZoneManager _zoneManager;

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<Renderer>();
    }
    private void OnEnable()
    {
        PlayingState.OnPlayingStateUpdate += UpdateComponent;
        _resourceManager = GameManager.Instance?.GetManager<ResourceManager>();
        _zoneManager = GameManager.Instance.GetManager<ZoneManager>();
        _timeManager = GameManager.Instance.GetManager<TimeManager>();
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
    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.Graveyard, DayCycle activeCycle = DayCycle.Night)
    {
        Name = name;
        Age = 0;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        MarkedForDeath = false;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);
        _playerInteractionBehaviour = new PlayerInteractionBehaviour(_meshRenderer);
        _loiteringBehaviour = new LoiteringBehaviour(this, MovementSpeed);
    }
    private void UpdateComponent()
    {
        if (_isTraveling)
        {
            Travel();
        }
        else if(_occupiedZone != null)
        {
            Roam();
        }
        else
        {
            Loiter();
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
    private void Loiter()
    {
        _loiteringBehaviour?.Loiter();
    }

    public void Travel()
    {
        _goToZoneBehaviour?.GoToZone();
    }
    private void GoToZone(ZoneType zoneType)
    {
        CancelTravel();
        Zone travelZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(zoneType);

        if (travelZone == null)
        {
            return;
        }

        if (travelZone.TryEnter(this))
        {
            _reservedZone = travelZone;

            SetGoToZoneBehaviour(travelZone);

            _isTraveling = true;
        }
    }
    private void GoToZone(Zone travelZone)
    {
        if (_isTraveling)
        {
            return;
        }

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
        }

        if (travelZone.TryEnter(this))
        {
            _reservedZone = travelZone;
            SetGoToZoneBehaviour(travelZone);
            _isTraveling = true;
        }
        else
        {
            ZoneType zoneType = travelZone.Type;
            travelZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(zoneType);

            if (travelZone == null)
            {
                return;
            }

            if (travelZone.TryEnter(this))
            {
                _reservedZone = travelZone;
                SetGoToZoneBehaviour(travelZone);
                _isTraveling = true;
            }
        }
    }
    private void SetGoToZoneBehaviour(Zone targetZone)
    {
        if (_goToZoneBehaviour != null)
        {
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
        }

        _goToZoneBehaviour = new GoToZoneBehaviour(this, MovementSpeed, targetZone);
        _goToZoneBehaviour.OnArrived += OnArrivedAtDestination;
    }

    public void GoToWork(DayCycle currentCycle)
    {
        if (currentCycle == DayCycle.Day) Age++;
        //Ignores Daycycle Simply works
        ValidatePreferredZone();

        Zone targetZone = _preferredZone;

        if (targetZone == null || targetZone.IsFull() || targetZone.Type != Occupation.WorkZoneType)
        {
            targetZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(Occupation.WorkZoneType);
        }

        if (targetZone != null)
        {
            GoToZone(targetZone);
        }
        else
        {
            _occupiedZone = null;
            Loiter();
        }
    }
    private void ValidatePreferredZone()
    {
        if (_preferredZone == null || _preferredZone.IsFull())
            _preferredZone = null;
    }
    public void TravelToZone(Zone travelZone)
    {
        if (_timeManager.CurrentDayCycle != _activeCycle)
        {
            return;
        }

        if (_isTraveling) CancelTravel();

        _preferredZone = travelZone;

        GoToZone(travelZone);
    }
    private void OnArrivedAtDestination(Zone zone)
    {
        if (_occupiedZone != null && _occupiedZone != zone)
        {
            if (_occupiedZone.GetNPCsInZone().Contains(this))
            {
                _occupiedZone.Exit(this);
            }
            _occupiedZone = null;
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
            _reservedZone.Exit(this);
            _reservedZone = null;
        }
    }
    public void CancelTravel()
    {
        if (_isTraveling)
        {
            _isTraveling = false;

            if (_goToZoneBehaviour != null)
            {
                _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
                _goToZoneBehaviour = null;
            }
        }

        ReleaseReservation();
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

    public void OnSelect(PlayerInputManager playerInputManager)
    {
        _playerInteractionBehaviour.OnSelect(playerInputManager);
    }

    public void OnDeselect()
    {
        _playerInteractionBehaviour.OnDeselect();
    }

    public void OnHover()
    {
        _playerInteractionBehaviour.OnHover();
    }

    public void OnHoverExit()
    {
        _playerInteractionBehaviour?.OnHoverExit();
    }
    public override void ResetOccupiedZone()
    {
        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
        }
    }
    public override Zone GetOccupiedZone()
    {
        return _occupiedZone;
    }
}
