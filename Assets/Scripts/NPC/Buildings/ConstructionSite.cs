using UnityEngine;
using UnityEngine.UIElements;

public class ConstructionSite : Building
{
    public AudioClip ConstructionSFX;

    protected override Occupation AssociatedOccupation => new BuilderOccupation();

    private Tile _targetTile;
    private int _buildTime;
    private TileType _finalTileType;
    private TileDatabase_SO _tileDatabase;

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
        TimeManager timeManager = GameManager.Instance.GetManager<TimeManager>();
        if (timeManager != null)
            timeManager.OnCycleCalculation += UpdateConstruction;

        base.Initialize();

        //SoundManager.PlaySoundEffect(ConstructionSFX, this.transform.position);
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
            //SoundManager.PlaySoundEffect(ConstructionSFX, this.transform.position);
            Instantiate(finalPrefab, transform.position, Quaternion.identity);
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
}
