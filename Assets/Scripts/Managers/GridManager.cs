using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : Manager
{
    private Tilemap _baseTilemap;

    private Dictionary<Vector2Int, bool> _occupiedCells = new();
    private float _gridSize = 2f;

    public override void Initialize()
    {
        _baseTilemap = FindFirstObjectByType<Tilemap>();   
    }
    public Vector2Int GetGridPos(Vector3 worldPos)
    {
        Vector3Int tilePos = _baseTilemap.WorldToCell(worldPos);
        return new Vector2Int(tilePos.x, tilePos.y);
    }
    public Vector3 GetSnappedPosition(Vector3 worldPos)
    {
        Vector3Int tilePos = _baseTilemap.WorldToCell(worldPos);
        Vector3 snappedPos = _baseTilemap.CellToWorld(tilePos);
        return new Vector3(snappedPos.x, 0, snappedPos.z);
    }
}
