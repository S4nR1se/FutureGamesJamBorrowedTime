using System.Collections.Generic;
using UnityEngine;

public class GridManager : Manager
{
    public GameObject TilePrefab => _tilePrefab;
    [SerializeField] private GameObject _tilePrefab;

    private Dictionary<Vector2Int, Building> _buildings = new Dictionary<Vector2Int, Building>();
    public Vector3 gridOrigin { get; private set; } = Vector3.zero;

    public int GridSize { get; private set; } = 16;
    public float CellSize { get; private set; } = 5;

    private bool[,] _occupancyGrid;

    private Tile[,] _tileObjects;
    private TileType[,] _tileTypes;

    public override void Initialize()
    {
        _occupancyGrid = new bool[GridSize, GridSize];
        _tileObjects = new Tile[GridSize, GridSize];
        _tileTypes = new TileType[GridSize, GridSize];
        InitializeGridFromScene();
    }
   private void InitializeGridFromScene()
    {
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            Vector2Int gridPos = WorldToGrid(tile.transform.position);
            if (gridPos.x >= 0 && gridPos.x < GridSize && gridPos.y >= 0 && gridPos.y < GridSize)
            {
                _tileObjects[gridPos.x, gridPos.y] = tile;
                _tileTypes[gridPos.x, gridPos.y] = tile.tileType;
                _occupancyGrid[gridPos.x, gridPos.y] = (tile.tileType == TileType.NonWalkable);
                // Note: If tileType is Building, assume it's a pre-placed building (handle below)
            }
        }
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tileObjects[x, y] == null)
                {
                    Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                    GameObject tileObj = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);
                    Tile tileComponent = tileObj.GetComponent<Tile>();
                    if (tileComponent == null) tileComponent = tileObj.AddComponent<Tile>();
                    tileComponent.tileType = TileType.NonWalkable;
                    _tileObjects[x, y] = tileComponent;
                    _tileTypes[x, y] = TileType.NonWalkable;
                    _occupancyGrid[x, y] = false;
                }
                else if (_tileTypes[x, y] == TileType.Building)
                {
                    Building building = _tileObjects[x, y].GetComponent<Building>();
                    if (building != null)
                    {
                        _buildings.Add(new Vector2Int(x, y), building);
                        // Mark all cells in building's footprint as occupied
                        for (int bx = x; bx < x + building.Size.x; bx++)
                        {
                            for (int by = y; by < y + building.Size.y; by++)
                            {
                                if (bx < GridSize && by < GridSize)
                                {
                                    _occupancyGrid[bx, by] = true;
                                    _tileTypes[bx, by] = TileType.Building;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3(gridPos.x * CellSize, 0, gridPos.y * CellSize);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / CellSize);
        int y = Mathf.FloorToInt((worldPos.z - gridOrigin.z) / CellSize);
        return new Vector2Int(Mathf.Clamp(x, 0, GridSize - 1), Mathf.Clamp(y, 0, GridSize - 1));
    }

    public bool IsAreaFree(Vector2Int startPos, Vector2Int size)
    {
        for (int x = startPos.x; x < startPos.x + size.x; x++)
        {
            for (int y = startPos.y; y < startPos.y + size.y; y++)
            {
                if (x >= GridSize || y >= GridSize || _occupancyGrid[x, y])
                    return false;
            }
        }
        return true;
    }
}

[System.Serializable]
public struct Vector2Int
{
    public int x, y;
    public Vector2Int(int x, int y) { this.x = x; this.y = y; }
}