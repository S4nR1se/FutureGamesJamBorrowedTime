using System.Collections.Generic;
using UnityEngine;

public class PlacementData
{
    public List<Vector3Int> OccupiedPositions;
    public int ID { get; private set; }
    public int PlacedObjectIndex { get; private set; }

    public PlacementData(List<Vector3Int> OccupiedPositions, int ID, int PlacedObjectIndex)
    {
        this.OccupiedPositions = OccupiedPositions;
        this.ID = ID;
        this.PlacedObjectIndex = PlacedObjectIndex;
    }
}

public class GridData : MonoBehaviour
{
    Dictionary<Vector3Int, PlacementData> _placedObjects = new();

    public void AddObjectAt(Vector3Int GridPosition, Vector2Int ObjectSize, int ID, int PlacedObjectIndex)
    {
        List<Vector3Int> PositionToOccupy = CalculatePositions(GridPosition, ObjectSize);
        PlacementData data = new PlacementData(PositionToOccupy, ID, PlacedObjectIndex);
        foreach(var pos in PositionToOccupy)
        {
            if(_placedObjects.ContainsKey(pos))
            {
                throw new System.Exception($"Dictionary already contains this cell position{pos}");
            }
            _placedObjects[pos] = data;
        }
    }

    private List<Vector3Int> CalculatePositions(Vector3Int GridPosition,Vector2Int ObjectSize)
    {
        List<Vector3Int> ReturnValues = new();
        for(int x = 0; x < ObjectSize.x; x++)
        {
            for (int y = 0; y < ObjectSize.y; y++)
            {
                ReturnValues.Add(GridPosition + new Vector3Int(x, 0, y));
            }
        }

        return ReturnValues;
    }

    public bool CanPlaceObjectAt(Vector3Int GridPosition, Vector2Int ObjectSize)
    {
        List<Vector3Int> PositionToOccupy = CalculatePositions(GridPosition, ObjectSize);
        foreach (var pos in PositionToOccupy)
        {
            if(_placedObjects.ContainsKey(pos))
            {
                return false;
            }
        }

        return true;
    }
}
