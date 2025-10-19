using System;
using UnityEngine;

public class RemovingState : iBuildingState
{
    private int _gameObjectIndex = -1;
    private Grid _grid;
    private PreviewSystem _previewSystem;
    private GridData _groundData;
    private GridData _objectsData;
    private ObjectPlacer _objectPlacer;

    public RemovingState(Grid grid, 
                         PreviewSystem previewSystem, 
                         GridData groundData, 
                         GridData objectsData, 
                         ObjectPlacer objectPlacer)
    {
        _grid = grid;
        _previewSystem = previewSystem;
        _groundData = groundData;
        _objectsData = objectsData;
        _objectPlacer = objectPlacer;

        _previewSystem.StartShowingRemovePreview();
    }

    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int GridPosition)
    {
        GridData selectedData = null;
        if(_objectsData.CanPlaceObjectAt(GridPosition, Vector2Int.one) == false)
        {
            selectedData = _objectsData;
        }
        else if(_groundData.CanPlaceObjectAt(GridPosition, Vector2Int.one) == false)
        {
            selectedData = _groundData;
        }

        if(selectedData == null)
        {

        }
        else
        {
            _gameObjectIndex = selectedData.GetRepresantationIndex(GridPosition);
            if(_gameObjectIndex == -1)
            {
                return;
            }

            selectedData.RemoveObjectAt(GridPosition);
            _objectPlacer.RemoveObjectAt(_gameObjectIndex);
        }
        Vector3 CellPosition = _grid.CellToWorld(GridPosition);
        _previewSystem.UpdatePosition(CellPosition, CheckIfPositionIsValid(GridPosition));
    }

    private bool CheckIfPositionIsValid(Vector3Int gridPosition)
    {
        return !((_objectsData.CanPlaceObjectAt(gridPosition, Vector2Int.one)) && (_groundData.CanPlaceObjectAt(gridPosition, Vector2Int.one)));
    }

    public void UpdateState(Vector3Int GridPosition)
    {
        bool validity = CheckIfPositionIsValid(GridPosition);
        _previewSystem.UpdatePosition(_grid.CellToWorld(GridPosition), validity);
    }
}
