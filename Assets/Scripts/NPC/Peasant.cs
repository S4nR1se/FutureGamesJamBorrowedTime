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
    private const int STARVING = 0;
    private const int ZERO = 0;

    private bool _isTraveling = false;

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
            _occupiedZone = null;
        }
    }

    public override void Initialize(Zone startZone, string name = "NPC", int lifeSpan = 11, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day)
    {
        Name = name;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        MarkedForDeath = false;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);

        _playerInteractionBehaviour = new PlayerInteractionBehaviour(_meshRenderer);

        SetCurrentZone(startZone);
        if (startZone != null && startZone.TryEnter(this))
        {
            _occupiedZone = startZone;
        }
        else if (startZone != null)
        {
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
            return;
        }

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
        }

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
            _travelPurpose = TravelPurpose.None;
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
            _travelPurpose = TravelPurpose.None;
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
                _travelPurpose = TravelPurpose.None;
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
        if (currentCycle != _activeCycle) return;
        GoToZone(Occupation.WorkZoneType);
        _travelPurpose = TravelPurpose.Work;
    }
    public void TravelToZone(Zone travelZone)
    {
        GoToZone(travelZone);
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
            CancelTravel();
            return;
        }

        if (_occupiedZone != null && _occupiedZone != zone)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
        }

        if (_reservedZone != zone)
        {
            CancelTravel();
            return;
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
            if (_goToZoneBehaviour != null)
            {
                _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
                _goToZoneBehaviour = null;
            }
            ReleaseReservation();
        }
    }

    private void OnDestroy()
    {
        ReleaseReservation();

        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
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

    public void RunNightChecklist()
    {
        DailyIntake();
        CheckStarvation();
        CheckHomelessness();
        CheckMortality();
        Debug.Log(MarkedForDeath);
    }
    
    private void DailyIntake()
    {
        if (_resourceManager.GetValue(Resources.FoodStock) > ZERO)
            _resourceManager.UpdateValue(Resources.FoodStock, DAILYMEAL);
        else
            _starvationValue++;
    }
    private void CheckStarvation()
    {
        if (_starvationValue > STARVING)
            MarkedForDeath = true;
    }
    private void CheckHomelessness()
    {
        if (_currentZone.Type != _restZoneType)
            _dreadFactor++;
    }
    private void CheckMortality()
    {
        if (CheckDayRemaining() <= ZERO) MarkedForDeath = true;
    }

    public int CheckDayRemaining()
    {
        return LifeSpan - _dreadFactor;
    }
}
