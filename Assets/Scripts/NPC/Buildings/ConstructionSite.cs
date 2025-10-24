using UnityEngine;

public class ConstructionSite : Building
{
    protected override Occupation AssociatedOccupation => new BuilderOccupation();

    private Tile _targetTile;
    public int RemainingBuildTime => _buildTime;
    private int _buildTime;

    private TileType _finalTileType;
    private TileDatabase_SO _tileDatabase;

    private TilePlacementManager _tileManager;

    private const int DECREASEPERPEASANT = 1;

    public void SetUpConstructionZone(Tile targetTile, int buildTime, TileType finalTileType, TileDatabase_SO tileDatabase)
    {
        _targetTile = targetTile;
        _buildTime = buildTime;
        _finalTileType = finalTileType;
        _tileDatabase = tileDatabase;
    }

    public override void Initialize()
    {
        TileType = TileType.ConstructionSite;

        _tileManager = GameManager.Instance.GetManager<TilePlacementManager>(); 
        TimeManager timeManager = GameManager.Instance.GetManager<TimeManager>();
        if (timeManager != null)
            timeManager.OnCycleCalculation += UpdateConstruction;

        base.Initialize();
    }

    private void OnDisable()
    {
        TimeManager timeManager = GameManager.Instance.GetManager<TimeManager>();
        if (timeManager != null)
            timeManager.OnCycleCalculation -= UpdateConstruction;
    }

    private void UpdateConstruction()
    {
        int workers = AssociatedZone.CurrentOccupancy;
        _buildTime -= workers * DECREASEPERPEASANT;
        _buildTime = Mathf.Max(0, _buildTime);

        if (_buildTime == 0)
            ConstructBuilding();
    }

    private void ConstructBuilding()
    {
        if (_tileDatabase == null)
            return;

        GameObject finalPrefab = _tileDatabase.GetPrefab(_finalTileType);
        if (finalPrefab != null)
        {
            Instantiate(finalPrefab, transform.position, Quaternion.identity);
            SoundManager.Instance.PlaySound("BuildingPlaced", transform.position);
            GameManager.Instance.GetManager<ZoneManager>().UnregisterZone(AssociatedZone);
            Destroy(gameObject);
        }
    }

    protected override void OnNPCEnter(NPC npc)
    {
    }

    protected override void OnNPCExit(NPC npc)
    {
    }
    public override void OnSelect(PlayerInputManager playerInputManager)
    {
        if (PlayerInputManager.TryGetPreviousWorkerSelection(out IWorker prevWorker))
        {
            prevWorker.AssignOccupation(AssociatedOccupation);
            prevWorker.TravelToZone(AssociatedZone);
            return;
        }
    }
    public override void OnHover()
    {
    }
    public override void OnHoverExit()
    {
        _tileManager.ClearConstructionPreview();
    }
}
