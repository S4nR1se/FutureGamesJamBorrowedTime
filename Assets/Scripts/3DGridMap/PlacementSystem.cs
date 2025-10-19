using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Grid _grid;

    [SerializeField] private ObjectDataBase _dataBase;
    [SerializeField] private GameObject _gridVisualization;
    [SerializeField] private PreviewSystem _preview;
    
    [SerializeField] private ObjectPlacer _objectPlacer;

    private GridData _mapGroundData, _placedObjectsData;
    private Vector3Int _lastDetectedPosition = Vector3Int.zero;

    iBuildingState buildingState;


    private void Start()
    {
        StopPlacement();
        _mapGroundData = new GridData();
        _placedObjectsData = new GridData();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        _gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID,_grid, _preview, _dataBase, _mapGroundData, _placedObjectsData,_objectPlacer);
        _inputManager.OnClicked += PlaceStructure;
        _inputManager.OnExit += StopPlacement;
    }

    public void StartRemoving()
    {
        StopPlacement();
        _gridVisualization.SetActive(true);
        buildingState = new RemovingState(_grid, _preview, _mapGroundData, _placedObjectsData, _objectPlacer);
        _inputManager.OnClicked += PlaceStructure;
        _inputManager.OnExit += StopPlacement;
    }

    private void PlaceStructure()
    {
        if(_inputManager.IsPointerOverUI())
        {
            return;
        }
        Vector3 MousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int GridPosition = _grid.WorldToCell(MousePosition);

        buildingState.OnAction(GridPosition);
    }

    private void StopPlacement()
    {
        if (buildingState == null)
        {
            return;
        }
        _gridVisualization.SetActive(false);
       buildingState.EndState();
        _inputManager.OnClicked -= PlaceStructure;
        _inputManager.OnExit -= StopPlacement;
        _lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    private void Update()
    {
        if(buildingState == null)
        {
            return;
        }

        Vector3 MousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int GridPosition = _grid.WorldToCell(MousePosition);

        if(_lastDetectedPosition != GridPosition)
        {
            buildingState.UpdateState(GridPosition);
            _lastDetectedPosition = GridPosition;
        }
    }
}
