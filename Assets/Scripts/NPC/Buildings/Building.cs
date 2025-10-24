using System.Collections.Generic;
using UnityEngine;

public abstract class Building : MonoBehaviour, IInteractable
{
    [SerializeField] private BuildingData_SO _buildingData;
    public Vector2Int Size { get; private set; } = new Vector2Int(1, 1);
    public BuildingData_SO BuildData {get; private set;}
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

    public virtual void Initialize()
    {
        BuildData = _buildingData;
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

    public IEnumerable<NPC> GetNPCsInBuilding()
    {
        return AssociatedZone?.GetNPCsInZone() ?? new List<NPC>();
    }

    public virtual void OnSelect(PlayerInputManager playerInputManager)
    {
        if (PlayerInputManager.TryGetPreviousWorkerSelection(out IWorker prevWorker))
        {
            prevWorker.AssignOccupation(AssociatedOccupation);
            prevWorker.TravelToZone(AssociatedZone);
            return;
        }

        if(UIManager != null)
        {
            UIManager.DisplayBuildingInfo(this, AssociatedZone.GetNPCsInZone());
        }
    }

    public virtual void OnDeselect()
    {

    }

    public virtual void OnHover()
    {
    }

    public virtual void OnHoverExit()
    {

    }
    public int GetOccupantsNumber()
    {
        return AssociatedZone.CurrentOccupancy;
    }
}

public enum Tier
{
    One,
    Two,
    Three
}
