using UnityEngine;

public class Peasant : NPC, IWorker
{
    public Occupation Occupation => _occupation;
    private Occupation _occupation;

    private RoamingBehaviour _roamingBehaviour;
    private GoToZoneBehaviour _goToZoneBehaviour;

    private bool _isTraveling = false;

    public override void Initialize(string name = "NPC", int lifeSpan = 10, float movementSpeed = 10)
    {
        Name = name;
        LifeSpan = lifeSpan;
        MovementSpeed = movementSpeed;
    }
    private void Start()
    {
        Initialize();
        Zone startZone = GetCurrentZone() ?? GameManager.Instance.GetManager<ZoneManager>().GetClosestZone(transform.position);
        _roamingBehaviour = new RoamingBehaviour(this, MovementSpeed, startZone);
        InvokeRepeating("StartTravel", 0, 10);
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
    public void StartTravel()
    {
        Zone randomZone = GameManager.Instance.GetManager<ZoneManager>().GetRandomAvailableZone(ZoneType.House);
        if (randomZone != null)
        {
            _goToZoneBehaviour = new GoToZoneBehaviour(this, MovementSpeed, randomZone);
            _goToZoneBehaviour.OnArrived += OnArrivedAtDestination;
            _isTraveling = true;
        }
    }

    private void OnArrivedAtDestination(Zone zone)
    {
        Debug.Log($"{Name} arrived at: {zone.Name}");
        _isTraveling = false;
        _roamingBehaviour.SetTargetZone(zone);

        _goToZoneBehaviour.OnArrived -= OnArrivedAtDestination;
    }
}
