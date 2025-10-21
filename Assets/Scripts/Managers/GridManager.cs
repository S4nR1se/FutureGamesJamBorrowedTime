using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class GridManager : Manager
{
    [Header("Tile Prefabs")]
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _startingTilePrefab;

    [Header("Grid Settings")]
    public int GridSize { get; private set; } = 16;
    public float CellSize { get; private set; } = 5f;
    public Vector3 gridOrigin { get; private set; } = Vector3.zero;

    private Tile[,] _tileObjects;
    private TileType[,] _tileTypes;
    private bool[,] _occupancyGrid;

    private NavMeshSurface _navMeshSurface;

    public GameObject TilePrefab => _tilePrefab;
    public GameObject StartingTilePrefab => _startingTilePrefab;

    public override void Initialize()
    {
        _tileObjects = new Tile[GridSize, GridSize];
        _tileTypes = new TileType[GridSize, GridSize];
        _occupancyGrid = new bool[GridSize, GridSize];

        InitializeGridFromScene();
        SetupNavMeshSurface();
        BakeNavMesh();
    }

    #region Grid Setup
    private void InitializeGridFromScene()
    {
        // Register tiles already in the scene
        foreach (Tile tile in GetComponentsInChildren<Tile>())
        {
            Vector2Int gridPos = WorldToGrid(tile.transform.position);
            if (IsValidGridPos(gridPos))
            {
                _tileObjects[gridPos.x, gridPos.y] = tile;
                _tileTypes[gridPos.x, gridPos.y] = tile.tileType;
                _occupancyGrid[gridPos.x, gridPos.y] = (tile.tileType != TileType.BaseTile);
            }
        }

        // Fill missing tiles
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tileObjects[x, y] == null)
                {
                    Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                    GameObject prefabToUse = (x == 0 && y == 0 && _startingTilePrefab != null) ? _startingTilePrefab : _tilePrefab;

                    GameObject tileObj = Instantiate(prefabToUse, worldPos, Quaternion.identity, transform);
                    Tile tileComponent = tileObj.GetComponent<Tile>();
                    if (tileComponent == null)
                        tileComponent = tileObj.AddComponent<Tile>();

                    tileComponent.tileType = TileType.BaseTile;
                    _tileObjects[x, y] = tileComponent;
                    _tileTypes[x, y] = tileComponent.tileType;
                    _occupancyGrid[x, y] = false; // BaseTile is free
                }
            }
        }
    }

    private void SetupNavMeshSurface()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        if (_navMeshSurface == null)
            _navMeshSurface = gameObject.AddComponent<NavMeshSurface>();

        _navMeshSurface.collectObjects = CollectObjects.Children;
        _navMeshSurface.overrideVoxelSize = true;
        _navMeshSurface.voxelSize = 0.2f;
        _navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        _navMeshSurface.layerMask = ~0;
    }

    public void BakeNavMesh()
    {
        _navMeshSurface?.BuildNavMesh();
    }
    #endregion

    #region Grid Conversion
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3(gridPos.x * CellSize, 0, gridPos.y * CellSize);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - gridOrigin;
        int x = Mathf.Clamp(Mathf.FloorToInt((relativePos.x + CellSize * 0.5f) / CellSize), 0, GridSize - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((relativePos.z + CellSize * 0.5f) / CellSize), 0, GridSize - 1);
        return new Vector2Int(x, y);
    }

    private bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GridSize && pos.y >= 0 && pos.y < GridSize;
    }
    #endregion

    #region Tile Access
    public Tile GetTileAt(Vector2Int gridPos)
    {
        if (!IsValidGridPos(gridPos)) return null;
        return _tileObjects[gridPos.x, gridPos.y];
    }

    public bool IsAreaFree(Vector2Int startPos, Vector2Int size)
    {
        for (int x = startPos.x; x < startPos.x + size.x; x++)
        {
            for (int y = startPos.y; y < startPos.y + size.y; y++)
            {
                if (!IsValidGridPos(new Vector2Int(x, y)) || _occupancyGrid[x, y])
                    return false;
            }
        }
        return true;
    }
    #endregion

    #region Tile Replacement
    public void ReplaceTile(Vector2Int gridPos, TileType newTileType, GameObject tilePrefab = null)
    {
        if (!IsValidGridPos(gridPos)) return;

        Tile oldTile = _tileObjects[gridPos.x, gridPos.y];
        if (oldTile != null)
            Destroy(oldTile.gameObject);

        GameObject prefabToUse = tilePrefab ?? _tilePrefab;
        GameObject newTileObj = Instantiate(prefabToUse, GridToWorld(gridPos), Quaternion.identity, transform);

        Tile newTile = newTileObj.GetComponent<Tile>();
        if (newTile == null)
            newTile = newTileObj.AddComponent<Tile>();

        newTile.tileType = newTileType;
        _tileObjects[gridPos.x, gridPos.y] = newTile;
        _tileTypes[gridPos.x, gridPos.y] = newTileType;

        _occupancyGrid[gridPos.x, gridPos.y] = (newTileType != TileType.BaseTile);
    }

    public void ReplaceTileArea(Vector2Int start, Vector2Int size, TileType newTileType, GameObject tilePrefab = null)
    {
        for (int x = start.x; x < start.x + size.x; x++)
        {
            for (int y = start.y; y < start.y + size.y; y++)
            {
                ReplaceTile(new Vector2Int(x, y), newTileType, tilePrefab);
            }
        }
    }
    #endregion
}

[System.Serializable]
public struct Vector2Int
{
    public int x, y;
    public Vector2Int(int x, int y) { this.x = x; this.y = y; }
}