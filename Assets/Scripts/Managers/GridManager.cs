using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GridManager : Manager
{
    public GameObject TilePrefab => _tilePrefab;
    [SerializeField] private GameObject _tilePrefab;

    private NavMeshSurface _navMeshSurface;

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
        SetupNavMeshSurface();
        BakeNavMesh();
    }
    private void SetupNavMeshSurface()
    {
        _navMeshSurface = GetComponent<NavMeshSurface>();
        if (_navMeshSurface == null)
        {
            _navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        }
        _navMeshSurface.collectObjects = CollectObjects.Children;

        _navMeshSurface.overrideVoxelSize = true;
        _navMeshSurface.voxelSize = 0.2f;

        _navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        _navMeshSurface.layerMask = ~0;
    }
    public void BakeNavMesh()
    {
        if (_navMeshSurface != null)
        {
            _navMeshSurface.BuildNavMesh();
        }
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
                _occupancyGrid[gridPos.x, gridPos.y] = (tile.tileType == TileType.BaseTile);
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
                    tileComponent.tileType = TileType.BaseTile;
                    _tileObjects[x, y] = tileComponent;
                    _tileTypes[x, y] = TileType.BaseTile;
                    _occupancyGrid[x, y] = false;
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
        Vector3 relativePos = worldPos - gridOrigin;

        float relX = (relativePos.x + CellSize * 0.5f) / CellSize;
        float relZ = (relativePos.z + CellSize * 0.5f) / CellSize;

        int x = Mathf.FloorToInt(relX);
        int y = Mathf.FloorToInt(relZ);

        x = Mathf.Clamp(x, 0, GridSize - 1);
        y = Mathf.Clamp(y, 0, GridSize - 1);

        return new Vector2Int(x, y);
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
    public void ReplaceTile(Vector2Int gridPos, TileType newTileType, GameObject tilePrefab = null)
    {
        if (gridPos.x < 0 || gridPos.x >= GridSize || gridPos.y < 0 || gridPos.y >= GridSize)
            return;

        Tile oldTile = _tileObjects[gridPos.x, gridPos.y];
        if (oldTile != null)
        {
            ZoneMarker oldZoneMarker = oldTile.GetComponent<ZoneMarker>();
            if (oldZoneMarker != null)
            {
                Zone oldZone = oldZoneMarker.GetZone();
                if (oldZone != null)
                {
                    ZoneManager zoneManager = GameManager.Instance.GetManager<ZoneManager>();
                    if (zoneManager != null)
                    {
                        zoneManager.UnregisterZone(oldZone);
                    }
                }
            }

            Destroy(oldTile.gameObject);
        }

        Vector3 worldPos = GridToWorld(gridPos);
        GameObject prefabToUse = tilePrefab ?? _tilePrefab;
        GameObject newTileObj = Instantiate(prefabToUse, worldPos, Quaternion.identity, transform);

        Tile newTile = newTileObj.GetComponent<Tile>();
        if (newTile == null)
            newTile = newTileObj.AddComponent<Tile>();

        newTile.tileType = newTileType;
        _tileObjects[gridPos.x, gridPos.y] = newTile;
        _tileTypes[gridPos.x, gridPos.y] = newTileType;
        _occupancyGrid[gridPos.x, gridPos.y] = (newTileType == TileType.BaseTile);

        ZoneMarker zoneMarker = newTileObj.GetComponent<ZoneMarker>();
        if (zoneMarker != null)
        {
            zoneMarker.InitializeZone();
        }
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
    public Tile GetTileAt(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= GridSize || gridPos.y < 0 || gridPos.y >= GridSize)
            return null;

        return _tileObjects[gridPos.x, gridPos.y];
    }
}

[System.Serializable]
public struct Vector2Int
{
    public int x, y;
    public Vector2Int(int x, int y) { this.x = x; this.y = y; }
}