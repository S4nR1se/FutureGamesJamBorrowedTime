using System.Resources;
using UnityEngine;

public class TilePlacementManager : Manager
{
    [SerializeField] private TileDatabase_SO tileDatabase;

    private GridManager _gridManager;
    private ResourceManager _resourceManager;

    private TileType _selectedTileType = TileType.BaseTile;

    public TileType SelectedTileType => _selectedTileType;

    public override void Initialize()
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _gridManager = GameManager.Instance.GetManager<GridManager>();
    }
    public void SelectBuilding(TileType tileType)
    {
        _selectedTileType = tileType;
        Debug.Log($"Selected building: {_selectedTileType}");
    }

    public void TryPlaceBuilding(Tile targetTile)
    {
        if (_selectedTileType == TileType.BaseTile || targetTile == null)
            return;

        Vector2Int gridPos = _gridManager.WorldToGrid(targetTile.transform.position);
        Tile existingTile = _gridManager.GetTileAt(gridPos);

        if (existingTile?.tileType != TileType.BaseTile)
        {
            Debug.Log("Cannot place building: Tile is already occupied.");
            return;
        }

        BuildingData_SO data = tileDatabase.tiles.Find(x => x.tileType == _selectedTileType)?.buildingData;
        if (data == null)
        {
            Debug.LogWarning($"No building data found for tile type {_selectedTileType}");
            return;
        }

        int materialCost = data.MaterialCost;
        int currentMaterials = _resourceManager.GetValue(Resources.Materials);

        if (currentMaterials < materialCost)
        {
            Debug.Log($"Not enough materials. Needed: {materialCost}, Current: {currentMaterials}");
            return;
        }

        _resourceManager.UpdateValue(Resources.Materials, -materialCost);

        GameObject constructionPrefab = tileDatabase.GetPrefab(TileType.ConstructionSite);
        if (constructionPrefab == null)
            return;

        GameObject constructionGO = Instantiate(
            constructionPrefab,
            targetTile.transform.position,
            Quaternion.identity,
            _gridManager.transform
        );

        ConstructionSite constructionSite = constructionGO.GetComponent<ConstructionSite>();
        if (constructionSite != null)
        {
            if (data != null)
            {
                constructionSite.SetUpConstructionZone(targetTile, data.BuildTime, _selectedTileType, tileDatabase);
            }
        }

        Destroy(targetTile.gameObject);
    }

    public void ClearSelection()
    {
        _selectedTileType = TileType.BaseTile;
    }
}
