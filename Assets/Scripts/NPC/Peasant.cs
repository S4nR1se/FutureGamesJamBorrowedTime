using System.Collections;
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
    public bool IsTraveling() => _isTraveling;

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
            _timeManager.OnCyclePassage -= HandleCyclePassage;

        ReleaseReservation();
        LeaveCurrentZone();
    }

    private void LeaveCurrentZone()
    {
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

    public void Initialize(Zone startZone, NPCManager.CatIdentity identity, string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day)
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
            _occupiedZone = (_timeManager != null && _timeManager.CurrentDayCycle == _activeCycle && startZone.Type == _restZoneType) ? null : startZone;
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
        if (_timeManager.IsCalculatingCycle()) return;

        StartCoroutine(WaitForCalculation(newCycle));
    }
    private void AssignRestZoneIfAvailable()
    {
        if (_occupiedZone == null && !_isTraveling)
        {
            Zone restZone = _zoneManager.GetRandomAvailableZone(_restZoneType);
            if (restZone != null)
            {
                GoToZone(restZone);
            }
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
        CheckForAvailableRestZone();
    }
    private void CheckForAvailableRestZone()
    {
        if (_timeManager.CurrentDayCycle == _activeCycle) return;

        Zone restZone = _zoneManager.GetRandomAvailableZone(_restZoneType);
        if (restZone != null && _occupiedZone != restZone)
        {
            GoToZone(restZone);
        }
    }
    private void CheckForAvailableWorkZone()
    {
        if (_timeManager.CurrentDayCycle != _activeCycle) return;

        _timeSinceLastZoneCheck += Time.deltaTime;
        if (_timeSinceLastZoneCheck < ZONE_CHECK_INTERVAL) return;

        _timeSinceLastZoneCheck = 0f;

        if (_isTraveling || _occupiedZone != null) return;

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
        if (_isTraveling || travelZone == null) return;

        if (_timeManager.CurrentDayCycle == _activeCycle && travelZone.Type == _restZoneType) return;

        LeaveCurrentZone();

        if (travelZone.TryEnter(this))
        {
            _reservedZone = travelZone;
            SetGoToZoneBehaviour(travelZone);
            _isTraveling = true;
        }
        else
        {
            ZoneType zoneType = travelZone.Type;
            Zone fallbackZone = _zoneManager.GetRandomAvailableZone(zoneType);

            if (fallbackZone != null && (zoneType != _restZoneType || _timeManager.CurrentDayCycle != DayCycle.Day))
            {
                if (fallbackZone.TryEnter(this))
                {
                    _reservedZone = fallbackZone;
                    SetGoToZoneBehaviour(fallbackZone);
                    _isTraveling = true;
                }
            }
        }
    }

    private void SetGoToZoneBehaviour(Zone targetZone)
    {
        if (_goToZoneBehaviour != null)
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;

        _goToZoneBehaviour = new GoToZoneBehaviour(this, MovementSpeed, targetZone);
        _goToZoneBehaviour.OnArrived += OnArrivedAtDestination;
    }

    public void GoToWork(DayCycle currentCycle)
    {
        if (currentCycle != _activeCycle)
        {
            LeaveCurrentZone();
            return;
        }

        if (_occupiedZone != null && _occupiedZone.Type == _restZoneType)
        {
            LeaveCurrentZone();
        }

        ValidatePreferredZone();

        Zone targetZone = _preferredZone ?? _zoneManager.GetRandomAvailableZone(Occupation.WorkZoneType);

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
        if (_timeManager.CurrentDayCycle != _activeCycle) return;

        if (_isTraveling) CancelTravel();

        _preferredZone = travelZone;
        GoToZone(travelZone);
    }

    [ContextMenu("Rest")]
    public void GoToRest()
    {
        if (_timeManager.CurrentDayCycle == _activeCycle) return;

        Zone restZone = _zoneManager.GetRandomAvailableZone(_restZoneType);

        if (restZone != null)
        {
            if (restZone.TryEnter(this))
                _occupiedZone = restZone;

            GoToZone(restZone);
        }
        else
        {
            LeaveCurrentZone();
            _isTraveling = false;
            Loiter();
        }
    }

    private void OnArrivedAtDestination(Zone zone)
    {
        if (zone == null)
        {
            CancelTravel();
            return;
        }

        if (_occupiedZone != null && _occupiedZone != zone)
            LeaveCurrentZone();

        if (_reservedZone != zone)
        {
            CancelTravel();
            return;
        }

        _occupiedZone = _reservedZone;
        _reservedZone = null;
        _isTraveling = false;
        _roamingBehaviour.SetTargetZone(zone);

        if (_goToZoneBehaviour != null)
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
        LeaveCurrentZone();

        if (_goToZoneBehaviour != null)
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;

        if (_timeManager != null)
            _timeManager.OnCyclePassage -= HandleCyclePassage;
    }

    public void ReturnToPool(ObjectPool pool)
    {
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        ReleaseReservation();
        LeaveCurrentZone();

        ClearCurrentZone();
        _isTraveling = false;

        if (_goToZoneBehaviour != null)
        {
            _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
            _goToZoneBehaviour = null;
        }

        pool.Release(this);
    }

    public void OnSelect(PlayerInputManager playerInputManager) => _playerInteractionBehaviour.OnSelect(playerInputManager);
    public void OnDeselect() => _playerInteractionBehaviour.OnDeselect();
    public void OnHover() => _playerInteractionBehaviour.OnHover();
    public void OnHoverExit() => _playerInteractionBehaviour?.OnHoverExit();
    public void UndeadContact() => IncreaseDread();
    public void IncreaseDread(int amount = 1) => _dreadFactor += amount;

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
        if (_currentZone == null || _currentZone.Type != _restZoneType)
        {
            IncreaseDread();
        }
    }

    private void CheckMortality()
    {
        if (CheckDayRemaining() <= ZERO)
            MarkedForDeath = true;
    }

    public int CheckDayRemaining() => LifeSpan - _dreadFactor;

    public Mood GetMood()
    {
        Zone restZone = _zoneManager.GetRandomAvailableZone(_restZoneType);

        if (_dreadFactor > 0 || _starvationValue > 0 || restZone == null) return Mood.Bad;
        return Mood.Neutral;
    }

    public override void ResetOccupiedZone() => LeaveCurrentZone();
    public override Zone GetOccupiedZone() => _occupiedZone;
    public void GatherPurr()
    {
        ResourceManager resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        if (resourceManager == null) return;
        resourceManager.UpdateValue(Resources.Purr, 9);
        DecreaseLifeSpan(1);
        ParticleSystemManager.Instance.Spawn("GetPurr", transform.position);
        if (LifeSpan == 0) NPCScheduler.Instance.ScheduleDeath(this);
    }
    private IEnumerator WaitForCalculation(DayCycle newCycle)
    {
        yield return new WaitForEndOfFrame();

        if (newCycle == _activeCycle)
        {
            GoToWork(newCycle);
        }
        else
        {
            AssignRestZoneIfAvailable();
            GoToRest();
        }
    }
}
