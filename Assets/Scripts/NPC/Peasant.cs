using System.Linq;
using UnityEditor.Overlays;
using UnityEngine;

public class Peasant : NPC, IWorker, IPeasant, IPoolable, IInteractable
{
    public GameObject PoolableComponent => gameObject;
    public GameObject Component => gameObject;

    [SerializeField] private Renderer _meshRenderer;

    public Occupation Occupation => _occupation;
    public int StarvationValue => _starvationValue;
    public int DreadFactor => _dreadFactor;

    private Occupation _occupation;

    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;
    private PlayerInteractionBehaviour _playerInteractionBehaviour;
    private LoiteringBehaviour _loiteringBehaviour;

    private const int DAILYMEAL = -1;
    private const int STARVING = 0;
    private const int ZERO = 0;

    private float _timeSinceLastZoneCheck = 0f;
    private const float ZONE_CHECK_INTERVAL = 2f;

    private bool _isTraveling = false;

    private int _starvationValue = 0;
    private int _dreadFactor = 0;

    private Zone _reservedZone;
    private Zone _occupiedZone;
    private Zone _preferredZone;

    private TimeManager _timeManager;
    private ResourceManager _resourceManager;
    private ZoneManager _zoneManager;

    private void OnEnable()
    {
        PlayingState.OnPlayingStateUpdate += UpdateComponent;
        _zoneManager = GameManager.Instance.GetManager<ZoneManager>();
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
        Age = 0;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        MarkedForDeath = false;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);
        _playerInteractionBehaviour = new PlayerInteractionBehaviour(_meshRenderer);
        _loiteringBehaviour = new LoiteringBehaviour(this, MovementSpeed);

        SetCurrentZone(startZone);
        if (startZone != null && startZone.TryEnter(this))
        {
            _occupiedZone = startZone;
        }
    }
    public void Initialize(Zone startZone,NPCManager.CatIdentity identity, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day)
    {
        Name = name;
        LifeSpan = lifeSpan;
        Age = 0;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        MarkedForDeath = false;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);
        _playerInteractionBehaviour = new PlayerInteractionBehaviour(_meshRenderer);
        _loiteringBehaviour = new LoiteringBehaviour(this, MovementSpeed);

        if (_meshRenderer != null && identity.Material != null)
        {
            Material[] mats = _meshRenderer.materials;
            mats[0] = identity.Material;
            _meshRenderer.materials = mats;
        }

        if (identity.Photo != null)
        {
            PassportPhoto = Sprite.Create(
                identity.Photo,
                new Rect(0, 0, identity.Photo.width, identity.Photo.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        SetCurrentZone(startZone);
        if (startZone != null && startZone.TryEnter(this))
        {
            if (_timeManager.CurrentDayCycle == _activeCycle && startZone.Type == _restZoneType)
            {
                _occupiedZone = null;
            }
            else
            {
                _occupiedZone = startZone;
            }
        }
    }
    private void UpdateComponent()
    {
        if (_isTraveling)
        {
            Travel();
        }
        else if (_occupiedZone != null)
        {
            Roam();
        }
        else
        {
            Loiter();
        }
    }

    private void HandleCyclePassage(DayCycle newCycle)
    {
        if (_timeManager.IsCalculatingCycle())
            return;
        if (_preferredZone != null && _preferredZone.Type == _restZoneType && _timeManager.CurrentDayCycle == _activeCycle)
        {
            _preferredZone = null;
        }

        if (_isTraveling && _reservedZone != null && _reservedZone.Type == _restZoneType)
        {
            if (newCycle == _activeCycle)
            {
                CancelTravel();
            }
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
    private void Loiter()
    {
        _loiteringBehaviour?.Loiter();

        CheckForAvailableWorkZone();
    }
    private void CheckForAvailableWorkZone()
    {
        if (_timeManager.CurrentDayCycle != _activeCycle)
            return;

        _timeSinceLastZoneCheck += Time.deltaTime;
        if (_timeSinceLastZoneCheck < ZONE_CHECK_INTERVAL)
            return;

        _timeSinceLastZoneCheck = 0f;

        if (_isTraveling || _occupiedZone != null)
            return;

        Zone availableWorkZone = _zoneManager.GetRandomAvailableZone(_occupation.WorkZoneType);

        if (availableWorkZone != null)
        {
            GoToZone(availableWorkZone);
        }
    }
    public void Travel()
    {
        _goToZoneBehaviour?.GoToZone();
    }
    private void GoToZone(Zone travelZone)
    {
        if (_isTraveling)
            return;

        if (_timeManager.CurrentDayCycle == DayCycle.Day && travelZone.Type == _restZoneType)
            return;

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
                return;

            if (_timeManager.CurrentDayCycle == DayCycle.Day && travelZone.Type == _restZoneType)
                return;

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
        if (currentCycle != _activeCycle)
        {
            ResetOccupiedZone();
            return;
        }

        if (_occupiedZone != null && _occupiedZone.Type == _restZoneType)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
        }

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

    [ContextMenu("Rest")]
    public void GoToRest()
    {
        if (_timeManager.CurrentDayCycle == _activeCycle)
            return;

        Zone restZone = _zoneManager.GetRandomAvailableZone(_restZoneType);
        if (restZone != null)
            GoToZone(restZone);
        else
        {
            _occupiedZone = null;
            _isTraveling = false;
            Loiter();
        }
    }

    [ContextMenu("Gather Purr")]
    public void GatherPurr()
    {
        ResourceManager resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        if (resourceManager == null) return;
        resourceManager.UpdateValue(Resources.Purr, 9);
        DecreaseLifeSpan(1);
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
        Age++;
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
    public Mood GetMood()
    {
        Zone restZone = _zoneManager.GetRandomAvailableZone(_restZoneType);

        if (_dreadFactor > 0 || _starvationValue > 0 || restZone == null) return Mood.Bad;
        else return Mood.Neutral;
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
