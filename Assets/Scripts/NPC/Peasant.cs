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

    private bool _isTraveling = false;

    private int _age = 1;
    private int _starvationValue = 0;
    private int _dreadFactor = 0;

    private Zone _reservedZone;
    private Zone _occupiedZone;

    private TravelPurpose _travelPurpose = TravelPurpose.None;

    private void Awake()
    {
        _meshRenderer = GetComponent<Renderer>();
    }
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
            Debug.Log($"{Name} disabled, exited {_occupiedZone.Name}");
            _occupiedZone = null;
        }
    }
    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.House, DayCycle activeCycle = DayCycle.Day)
    {
        Name = name;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;

        _activeCycle = activeCycle;

        _occupation = occupation ?? CreateDefaultOccupation();
        SetRestZoneType(restZoneType);

        Zone startZone = GetCurrentZone() ?? GameManager.Instance.GetManager<ZoneManager>().GetClosestZone(transform.position);
        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);

        _playerInteractionBehaviour = new PlayerInteractionBehaviour(_meshRenderer);

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
            GoToWork(_activeCycle);
        } 
        else if (!_isTraveling && _occupiedZone.Type == Occupation.WorkZoneType)
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
        if (_occupiedZone != null && _occupiedZone != zone)
        {
            if (_occupiedZone.GetNPCsInZone().Contains(this))
            {
                _occupiedZone.Exit(this);
                Debug.Log($"{Name} exited {_occupiedZone.Name} to enter {zone.Name}");
            }
            _occupiedZone = null;
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

    public void OnSelect()
    {
        _playerInteractionBehaviour.OnSelect();
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
}
