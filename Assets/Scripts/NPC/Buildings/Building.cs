using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingData_SO _buildingData;
    public Vector2Int Size { get; private set; } = new Vector2Int(1, 1);
    public int BuildTime { get; protected set; }
    public int MaterialCost { get; protected set; }
    public int PurrCost { get; protected set; }
    public int OutputPerWorker { get; protected set; }
    public int WorkerSize { get; protected set; }
    public Tier BuildingTier { get; protected set; }
    protected Zone AssociatedZone { get; private set; }
    protected ResourceManager ResourceManager { get; private set; }
    protected PlayerInputManager PlayerInputManager { get; private set; }
    protected UIManager UIManager { get; private set; }
    protected abstract Occupation AssociatedOccupation { get;}

    public GameObject Component => gameObject;

    protected AudioSource _portalIdleSource;

    public virtual void Initialize()
    {
        BuildTime = _buildingData.BuildTime;
        MaterialCost = _buildingData.MaterialCost;
        PurrCost = _buildingData.PurrCost;
        OutputPerWorker = _buildingData.OutputPerWorker;
        WorkerSize = _buildingData.WorkerSize;
        BuildingTier = _buildingData.BuildingTier;

        ResourceManager = GameManager.Instance?.GetManager<ResourceManager>();

        PlayerInputManager = GameManager.Instance.GetManager<PlayerInputManager>();
        UIManager = GameManager.Instance.GetManager<UIManager>();

        ZoneMarker marker = GetComponent<ZoneMarker>();
        if (marker != null)
        {
            AssociatedZone = marker.GetZone();
            if (AssociatedZone != null)
            {
                AssociatedZone.NPCEntered += OnNPCEnter;
                AssociatedZone.NPCExited += OnNPCExit;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (AssociatedZone != null)
        {
            AssociatedZone.NPCEntered -= OnNPCEnter;
            AssociatedZone.NPCExited -= OnNPCExit;
        }
    }

    protected abstract void OnNPCEnter(NPC npc);

    protected abstract void OnNPCExit(NPC npc);

    protected IEnumerable<NPC> GetNPCsInBuilding()
    {
        return AssociatedZone?.GetNPCsInZone() ?? new List<NPC>();
    }

    public virtual void OnSelect(PlayerInputManager playerInputManager)
    {
        if (PlayerInputManager.TryGetPreviousWorkerSelection(out IWorker prevWorker))
        {
            prevWorker.AssignOccupation(AssociatedOccupation);
            prevWorker.TravelToZone(AssociatedZone);
        }

        if(UIManager != null)
        {
            //UIManager.DisplayBuildingInfo(GetNPCsInBuilding());
        }

        if (this is Castle)
        {
            if (_portalIdleSource == null)
                _portalIdleSource = SoundManager.Instance.PlaySound("Portal-Idle", transform.position);
        }
        if (this is Farm)
        {
            SoundManager.Instance.PlaySound("FarmingScythe", transform.position);
        }
    }

    public virtual void OnDeselect()
    {
        if (this is Castle && _portalIdleSource != null)
        {
            SoundManager.Instance.StopSound(_portalIdleSource, 1.5f); // fade-out 1.5s
            _portalIdleSource = null;
        }
    }

    public virtual void OnHover()
    {

    }

    public virtual void OnHoverExit()
    {

    }
}

public enum Tier
{
    One,
    Two,
    Three
}
