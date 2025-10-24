using System.Linq;
using System.Security.Principal;
using UnityEngine;

public abstract class Undead : NPC, IWorker, IPoolable, IInteractable
{
    [SerializeField] private Texture2D _passPortPhoto;
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

    private float _timeSinceLastZoneCheck = 0f;
    private const float ZONE_CHECK_INTERVAL = 2f;

    private bool _isTraveling = false;
    public bool IsTraveling() => _isTraveling;

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

        PassportPhoto = Sprite.Create(
                _passPortPhoto,
                new Rect(0, 0, _passPortPhoto.width, _passPortPhoto.height),
                new Vector2(0.5f, 0.5f)
            );

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
        CheckForAvailableWorkZone();
    }

    public void Travel()
    {
        _goToZoneBehaviour?.GoToZone();
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

    private void GoToZone(Zone travelZone)
    {
        if (_isTraveling || travelZone == null) return;

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
    private void LeaveCurrentZone()
    {
        if (_occupiedZone != null)
        {
            _occupiedZone.Exit(this);
            _occupiedZone = null;
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
        if (currentCycle == DayCycle.Day) Age++;

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
    public override void ResetOccupiedZone() => LeaveCurrentZone();
    public override Zone GetOccupiedZone() => _occupiedZone;
}
