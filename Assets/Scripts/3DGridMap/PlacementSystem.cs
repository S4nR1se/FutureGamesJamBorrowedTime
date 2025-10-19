using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private GameObject _mouseIndicator, _cellIndicator;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Grid _grid;

    [SerializeField] private ObjectDataBase _dataBase;
    [SerializeField] private int _selectedObjectIndex = -1;
    [SerializeField] private GameObject _gridVisualization;

    private void Start()
    {
        StopPlacement();
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
        _cellIndicator.SetActive(true);
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
        GameObject NewTileObject = Instantiate(_dataBase.Get_Objects()[_selectedObjectIndex].Prefab);
        NewTileObject.transform.position = _grid.CellToWorld(GridPosition);
    }

    private void StopPlacement()
    {
        _selectedObjectIndex = -1;
        _gridVisualization.SetActive(false);
        _cellIndicator.SetActive(false);
        _inputManager.OnClicked -= PlaceStructure;
        _inputManager.OnExit -= StopPlacement;
    }

    private void Update()
    {
        if(_selectedObjectIndex < 0)
        {
            return;
        }

        Vector3 MousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int GridPosition = _grid.WorldToCell(MousePosition);
        _mouseIndicator.transform.position = MousePosition;
        _cellIndicator.transform.position = _grid.CellToWorld(GridPosition);
    }
}
