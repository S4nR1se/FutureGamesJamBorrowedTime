using UnityEngine;

public class TilePlacementManager : Manager
{
    [SerializeField] private TileDatabase_SO tileDatabase;

    private GridManager _gridManager;
    private TileType _selectedTileType = TileType.BaseTile;

    public TileType SelectedTileType => _selectedTileType;

    public override void Initialize()
    {
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

        if (_gridManager.GetTileAt(gridPos)?.tileType != TileType.BaseTile)
        {
            Debug.Log("Cannot place building: Tile is already occupied.");
            return;
        }
        GameObject prefab = tileDatabase.GetPrefab(_selectedTileType);
        if (prefab == null)
            return;

        _gridManager.ReplaceTile(gridPos, _selectedTileType, prefab);
    }

    public void ClearSelection()
    {
        _selectedTileType = TileType.BaseTile;
    }
}
