using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private GameObject _mouseIndicator, _cellIndicator;
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private Grid _grid; 

    private void Update()
    {
        Vector3 MousePosition = _inputManager.GetSelectedMapPosition();
        Vector3Int GridPosition = _grid.WorldToCell(MousePosition);
        _mouseIndicator.transform.position = MousePosition;
        _cellIndicator.transform.position = _grid.CellToWorld(GridPosition);
    }
}
