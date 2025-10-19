using UnityEditor.Overlays;
using UnityEngine;

public class Peasant : NPC, IWorker, IPeasant
{
    public Occupation Occupation => _occupation;
    public int Age => _age;
    public int StarvationValue => _starvationValue;
    public int DreadFactor => _dreadFactor;

    private Occupation _occupation;
    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;

    private bool _isTraveling = false;

    private int _age = 1;
    private int _starvationValue = 0;
    private int _dreadFactor = 0;

    private Zone _reservedZone;
    private Zone _occupiedZone;

    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 5, Occupation occupation = null, ZoneType restZoneType = ZoneType.House)
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

    private void Start()
    {
        Initialize();
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

    public override void GoToZone(ZoneType zoneType)
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
    public void GoRest()
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
}
