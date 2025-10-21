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

    public void TryPlaceBuilding(Vector3 worldPosition)
    {
        if (_selectedTileType == TileType.BaseTile)
        {
            return;
        }

        Vector2Int gridPos = _gridManager.WorldToGrid(worldPosition);

        if (gridPos.x < 0 || gridPos.x >= _gridManager.GridSize ||
            gridPos.y < 0 || gridPos.y >= _gridManager.GridSize)
        {
            return;
        }
        GameObject prefab = tileDatabase.GetPrefab(_selectedTileType);

        if (prefab == null)
        {
            return;
        }

        _gridManager.ReplaceTile(gridPos, _selectedTileType, prefab);
    }

    public void ClearSelection()
    {
        _selectedTileType = TileType.BaseTile;
    }
}
