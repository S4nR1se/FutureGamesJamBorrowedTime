using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private GameObject _mouseIndicator;//, _cellIndicator;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Grid _grid;

    [SerializeField] private ObjectDataBase _dataBase;
    [SerializeField] private int _selectedObjectIndex = -1;
    [SerializeField] private GameObject _gridVisualization;
    [SerializeField] private PreviewSystem _preview;

    private GridData _mapGroundData, _placedObjectsData;
    private Renderer _previewRenderer;
    private List<GameObject> _placedGameObjects = new();
    private Vector3Int _lastDetectedPosition = Vector3Int.zero;


    private void Start()
    {
        StopPlacement();
        _mapGroundData = new GridData();
        _placedObjectsData = new GridData();
        //_previewRenderer = _cellIndicator.GetComponentInChildren<Renderer>();
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        _selectedObjectIndex = _dataBase.Get_Objects().FindIndex(data => data.ID == ID);
        if(_selectedObjectIndex < 0)
        {
            Debug.LogError($"No object found with {ID})");
            return;
        }
        _gridVisualization.SetActive(true);
        _preview.StartShowingPlacementPreview(_dataBase.ObjectsData[_selectedObjectIndex].Prefab, _dataBase.ObjectsData[_selectedObjectIndex].Size);
        //_cellIndicator.SetActive(true);
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

        bool PlacementValidity = CheckPlacementValidity(GridPosition, _selectedObjectIndex);
        if(PlacementValidity == false)
        {
            return;
        }

        GameObject NewTileObject = Instantiate(_dataBase.Get_Objects()[_selectedObjectIndex].Prefab);
        NewTileObject.transform.position = _grid.CellToWorld(GridPosition);
        _placedGameObjects.Add(NewTileObject);

        GridData SelectedData = _dataBase.ObjectsData[_selectedObjectIndex].ID == 0 ? _mapGroundData : _placedObjectsData;
        SelectedData.AddObjectAt(GridPosition, _dataBase.ObjectsData[_selectedObjectIndex].Size, _dataBase.ObjectsData[_selectedObjectIndex].ID, _placedGameObjects.Count -1);

        _preview.UpdatePosition(_grid.CellToWorld(GridPosition),false);
    }

    private bool CheckPlacementValidity(Vector3Int GridPosition,int SelectedObjectIndex)
    {
        GridData SelectedData = _dataBase.ObjectsData[SelectedObjectIndex].ID == 0 ? _mapGroundData : _placedObjectsData;
        return SelectedData.CanPlaceObjectAt(GridPosition, _dataBase.ObjectsData[SelectedObjectIndex].Size);
    }

    private void StopPlacement()
    {
        _selectedObjectIndex = -1;
        _gridVisualization.SetActive(false);
        //_cellIndicator.SetActive(false);
        _preview.StopShowingPreview();
        _inputManager.OnClicked -= PlaceStructure;
        _inputManager.OnExit -= StopPlacement;
        _lastDetectedPosition = Vector3Int.zero;
    }

    private void Update()
    {
        if(_selectedObjectIndex < 0)
        {
            return;
        }

        Vector3 MousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int GridPosition = _grid.WorldToCell(MousePosition);

        if(_lastDetectedPosition != GridPosition)
        {
            bool PlacementValidity = CheckPlacementValidity(GridPosition, _selectedObjectIndex);
            //_previewRenderer.material.color = PlacementValidity ? Color.white : Color.red;

            _mouseIndicator.transform.position = MousePosition;
            //_cellIndicator.transform.position = _grid.CellToWorld(GridPosition);
            _preview.UpdatePosition(_grid.CellToWorld(GridPosition), PlacementValidity);
            _lastDetectedPosition = GridPosition;
        }
        
       
    }
}
