using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class PlacementState : iBuildingState
{
    private int _selectedObjectIndex = -1;
    private int ID;
    private Grid _grid;
    private PreviewSystem _previewSystem;
    private ObjectDataBase _dataBase;
    private GridData _groundData;
    private GridData _objectsData;
    private ObjectPlacer _objectPlacer;

    public PlacementState(int iD, Grid grid, PreviewSystem previewSystem,
                          ObjectDataBase database, GridData groundData,
                          GridData objectsData, ObjectPlacer objectPlacer)
    {
        ID = iD;
        _grid = grid;
        _previewSystem = previewSystem;
        _dataBase = database;
        _groundData = groundData;
        _objectsData = objectsData;
        _objectPlacer = objectPlacer;

        _selectedObjectIndex = _dataBase.Get_Objects().FindIndex(data => data.ID == ID);
        if (_selectedObjectIndex > -1)
        {
            _previewSystem.StartShowingPlacementPreview(_dataBase.ObjectsData[_selectedObjectIndex].Prefab, _dataBase.ObjectsData[_selectedObjectIndex].Size);
        }
        else
        {
            throw new System.Exception($"No object with id {iD}");
        }
    }

    public void EndState()
    {
        _previewSystem.StopShowingPreview();
    }

    public void OnAction(Vector3Int GridPosition)
    {
        bool PlacementValidity = CheckPlacementValidity(GridPosition, _selectedObjectIndex);
        if (PlacementValidity == false)
        {
            return;
        }

        int index = _objectPlacer.PlaceObject(_dataBase.Get_Objects()[_selectedObjectIndex].Prefab, _grid.CellToWorld(GridPosition));

        GridData SelectedData = _dataBase.ObjectsData[_selectedObjectIndex].ID == 0 ? _groundData : _objectsData;
        SelectedData.AddObjectAt(GridPosition, _dataBase.ObjectsData[_selectedObjectIndex].Size, _dataBase.ObjectsData[_selectedObjectIndex].ID, index);

        _previewSystem.UpdatePosition(_grid.CellToWorld(GridPosition), false);
    }

    private bool CheckPlacementValidity(Vector3Int GridPosition, int SelectedObjectIndex)
    {
        GridData SelectedData = _dataBase.ObjectsData[SelectedObjectIndex].ID == 0 ? _groundData : _objectsData;
        return SelectedData.CanPlaceObjectAt(GridPosition, _dataBase.ObjectsData[SelectedObjectIndex].Size);
    }

    public void UpdateState(Vector3Int GridPosition)
    {
        bool PlacementValidity = CheckPlacementValidity(GridPosition, _selectedObjectIndex);
        _previewSystem.UpdatePosition(_grid.CellToWorld(GridPosition), PlacementValidity);
    }
}
