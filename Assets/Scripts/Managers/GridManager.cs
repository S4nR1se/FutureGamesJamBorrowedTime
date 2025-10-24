using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class GridManager : Manager
{
    [Header("Tile Prefabs")]
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private GameObject _houseTilePrefab;
    [SerializeField] private GameObject _castleTilePrefab;
    [SerializeField] private GameObject _graveyardTilePrefab;

    [Header("Grid Settings")]
    public int GridSize { get; private set; } = 16;
    public float CellSize { get; private set; } = 5f;
    public Vector3 gridOrigin { get; private set; } = Vector3.zero;

    private Tile[,] _tileObjects;
    private TileType[,] _tileTypes;
    private bool[,] _occupancyGrid;

    private NavMeshSurface _navMeshSurface;

    public GameObject TilePrefab => _tilePrefab;
    public GameObject HouseTilePrefab => _houseTilePrefab;

    public override void Initialize()
    {
        _tileObjects = new Tile[GridSize, GridSize];
        _tileTypes = new TileType[GridSize, GridSize];
        _occupancyGrid = new bool[GridSize, GridSize];

        InitializeGridFromScene();
        PlaceSpecialTiles();
        SetupNavMeshSurface();
        BakeNavMesh();
    }

    private void InitializeGridFromScene()
    {
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

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tileObjects[x, y] == null)
                {
                    Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                    GameObject tileObj = Instantiate(_tilePrefab, worldPos, Quaternion.identity, transform);

                    Tile tileComponent = tileObj.GetComponent<Tile>();
                    if (tileComponent == null)
                        tileComponent = tileObj.AddComponent<Tile>();

                    tileComponent.tileType = TileType.BaseTile;
                    _tileObjects[x, y] = tileComponent;
                    _tileTypes[x, y] = tileComponent.tileType;
                    _occupancyGrid[x, y] = false;
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
        _navMeshSurface.layerMask = ~(1 << LayerMask.NameToLayer("NavMeshIgnore"));
        _navMeshSurface?.BuildNavMesh();
    }

    public Vector3 GridToWorld(Vector2Int gridPos) =>
        gridOrigin + new Vector3(gridPos.x * CellSize, 0, gridPos.y * CellSize);

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 relativePos = worldPos - gridOrigin;
        int x = Mathf.Clamp(Mathf.FloorToInt((relativePos.x + CellSize * 0.5f) / CellSize), 0, GridSize - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((relativePos.z + CellSize * 0.5f) / CellSize), 0, GridSize - 1);
        return new Vector2Int(x, y);
    }

    private bool IsValidGridPos(Vector2Int pos) =>
        pos.x >= 0 && pos.x < GridSize && pos.y >= 0 && pos.y < GridSize;

    public Tile GetTileAt(Vector2Int gridPos) =>
        IsValidGridPos(gridPos) ? _tileObjects[gridPos.x, gridPos.y] : null;

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

    public void SetTileOccupied(Vector2Int pos, bool occupied)
    {
        if (!IsValidGridPos(pos)) return;
        _occupancyGrid[pos.x, pos.y] = occupied;
    }

    public void PlaceSpecialTiles()
    {
        Vector2Int center = new Vector2Int(GridSize / 2, GridSize / 2);
        int radius = GridSize / 8;
        int minDistance = Mathf.Max(2, GridSize / 6); 

        Vector2Int castlePos = GetRandomPositionNear(center, radius);

        Vector2Int graveyardPos;
        do
        {
            graveyardPos = GetRandomPositionNear(center, radius);
        } while (Vector2Int.Distance(graveyardPos, castlePos) < minDistance);

        Vector2Int housePos;
        do
        {
            housePos = GetRandomPositionNear(center, radius);
        } while (
            Vector2Int.Distance(housePos, castlePos) < minDistance ||
            Vector2Int.Distance(housePos, graveyardPos) < minDistance
        );

        ReplaceTile(castlePos, TileType.Castle, _castleTilePrefab);
        ReplaceTile(graveyardPos, TileType.Graveyard, _graveyardTilePrefab);
        ReplaceTile(housePos, TileType.House, _houseTilePrefab);
    }

    private Vector2Int GetRandomPositionNear(Vector2Int center, int radius)
    {
        int x = Mathf.Clamp(center.x + Random.Range(-radius, radius + 1), 0, GridSize - 1);
        int y = Mathf.Clamp(center.y + Random.Range(-radius, radius + 1), 0, GridSize - 1);
        return new Vector2Int(x, y);
    }
}