using System.Linq;
using UnityEditor.Overlays;
using UnityEngine;

public class Peasant : NPC, IWorker, IPeasant, IPoolable, IInteractable
{
    public GameObject PoolableComponent => gameObject;
    public GameObject Component => gameObject;

    private Renderer _meshRenderer;

    public Occupation Occupation => _occupation;
    public int Age => _age;
    public int StarvationValue => _starvationValue;
    public int DreadFactor => _dreadFactor;

    private Occupation _occupation;

    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;
    private PlayerInteractionBehaviour _playerInteractionBehaviour;

    private const int DAILYMEAL = -1;
    private const int STARVING = 1;
    private const int ZERO = 0;

    private bool _isTraveling = false;
    private bool _markedForDeath = false;

    private int _age = 1;
    private int _starvationValue = 0;
    private int _dreadFactor = 0;

    private Zone _reservedZone;
    private Zone _occupiedZone;

    private TravelPurpose _travelPurpose = TravelPurpose.None;

    private TimeManager _timeManager;
    private ResourceManager _resourceManager;

    private void Awake()
    {
        _meshRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnEnable()
    {
        PlayingState.OnPlayingStateUpdate += UpdateComponent;
        _timeManager = GameManager.Instance.GetManager<TimeManager>();
        _resourceManager = GameManager.Instance?.GetManager<ResourceManager>();
        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage += HandleCyclePassage;
        }
    }

    private void OnDisable()
    {
        PlayingState.OnPlayingStateUpdate -= UpdateComponent;
        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage -= HandleCyclePassage;
        }
        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            Debug.Log($"[Peasant] {Name} exited {_occupiedZone.Name} on disable");
            _occupiedZone = null;
        }
    }

    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 11, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day)
    {
        Name = name;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);

        _playerInteractionBehaviour = new PlayerInteractionBehaviour(_meshRenderer);

        SetCurrentZone(startZone);
        if (startZone != null && startZone.TryEnter(this))
        {
            _occupiedZone = startZone;
            Debug.Log($"[Peasant] {Name} initialized and registered in {startZone.Name}");
        }
        else if (startZone != null)
        {
            Debug.LogWarning($"[Peasant] {Name} failed to register in initial zone {startZone.Name}");
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

    private void HandleCyclePassage(DayCycle newCycle)
    {
        if (_timeManager.IsCalculatingCycle())
        {
            Debug.Log($"[Peasant] {Name} delaying cycle passage handling due to ongoing cycle calculation");
            return;
        }

        if (newCycle == _activeCycle)
        {
            GoToWork(newCycle);
        }
        else
        {
            GoToRest();
        }
    }

    public void AssignOccupation(Occupation occupation)
    {
        _occupation = occupation;

        if (_isTraveling && _travelPurpose == TravelPurpose.Work)
        {
            CancelTravel();
            GoToWork(_activeCycle);
        }
        else if (!_isTraveling && _occupiedZone != null && _occupiedZone.Type == Occupation.WorkZoneType)
        {
            GoToWork(_activeCycle);
        }
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
        if (_isTraveling)
        {
            Debug.LogWarning($"[Peasant] {Name} cannot go to {zoneType} zone: already traveling");
            return;
        }

        // Ensure NPC is removed from current zone before reserving a new one
        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            Debug.Log($"[Peasant] {Name} exited {_occupiedZone.Name} before going to new zone");
            _occupiedZone = null;
        }

        Zone travelZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(zoneType);
        if (travelZone == null)
        {
            Debug.LogWarning($"[Peasant] {Name} found no available {zoneType} zone");
            return;
        }

        if (travelZone.TryEnter(this))
        {
            _reservedZone = travelZone;
            SetGoToZoneBehaviour(travelZone);
            _isTraveling = true;
            _travelPurpose = TravelPurpose.None;
            Debug.Log($"[Peasant] {Name} reserved and traveling to {travelZone.Name}");
        }
        else
        {
            Debug.LogWarning($"[Peasant] {Name} failed to reserve {travelZone.Name}: zone full or already contains NPC");
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

    [ContextMenu("Work")]
    public void GoToWork(DayCycle currentCycle)
    {
        if (currentCycle != _activeCycle) return;
        GoToZone(Occupation.WorkZoneType);
        _travelPurpose = TravelPurpose.Work;
    }

    [ContextMenu("Rest")]
    public void GoToRest()
    {
        GoToZone(_restZoneType);
        _travelPurpose = TravelPurpose.Rest;
    }

    private void OnArrivedAtDestination(Zone zone)
    {
        if (zone == null)
        {
            Debug.LogWarning($"[Peasant] {Name} arrived at null zone");
            CancelTravel();
            return;
        }

        // Ensure NPC is removed from previous zone
        if (_occupiedZone != null && _occupiedZone != zone)
        {
            _occupiedZone.Exit(this);
            Debug.Log($"[Peasant] {Name} exited {_occupiedZone.Name} to enter {zone.Name}");
            _occupiedZone = null;
        }

        if (_reservedZone != zone)
        {
            Debug.LogWarning($"[Peasant] {Name} arrived at {zone.Name}, but reserved zone was {_reservedZone?.Name ?? "null"}");
            CancelTravel();
            return;
        }

        _occupiedZone = _reservedZone;
        _reservedZone = null;
        _isTraveling = false;
        _roamingBehaviour.SetTargetZone(zone);
        _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
        _travelPurpose = TravelPurpose.None;

        Debug.Log($"[Peasant] {Name} successfully registered in {zone.Name}");
    }

    private void ReleaseReservation()
    {
        if (_reservedZone != null)
        {
            _reservedZone.Exit(this);
            Debug.Log($"[Peasant] {Name} released reservation for {_reservedZone.Name}");
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
            ReleaseReservation();
            Debug.Log($"[Peasant] {Name} canceled travel");
        }
    }

    private void OnDestroy()
    {
        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            Debug.Log($"[Peasant] {Name} exited {_occupiedZone.Name} on destroy");
            _occupiedZone = null;
        }

        if (_goToZoneBehaviour != null)
        {
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
        }

        if (_timeManager != null)
        {
            _timeManager.OnCyclePassage -= HandleCyclePassage;
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
            Debug.Log($"[Peasant] {Name} exited {_occupiedZone.Name} on return to pool");
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

    public void UndeadContact()
    {
        _dreadFactor++;
    }

    public void NightChecklist()
    {
        _dailyIntake();
        _checkStarvation();
        _checkHomelessness();
        _checkMortality();
    }
    
    private void _dailyIntake()
    {
        if (_resourceManager.GetValue(Resources.FoodStock) > ZERO)
            _resourceManager.UpdateValue(Resources.FoodStock, DAILYMEAL);
        else
            _starvationValue++;
    }
    private void _checkStarvation()
    {
        if (_starvationValue > STARVING)
            _markedForDeath = true;
    }
    private void _checkHomelessness()
    {
        if (_restZoneType == ZoneType.Road)
            _dreadFactor++;
    }
    private void _checkMortality()
    {
        if (CheckDayRemaining() <= ZERO)
            _markedForDeath = true;
    }

    public int CheckDayRemaining()
    {
        return LifeSpan - _dreadFactor;
    }
    public void BorrowTime(int borrowedAmout = 1)
    {
        DecreaseLifeSpan(borrowedAmout);
    }
}
