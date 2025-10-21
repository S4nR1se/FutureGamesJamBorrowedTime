using UnityEngine.EventSystems;
using UnityEngine;

[System.Serializable]
public class KeyMapping
{
    public string actionName;
    public KeyCode key;
}

public class PlayerInputManager : Manager
{
    [SerializeField] private KeyMappings_SO keyMappingConfig;

    private Camera _mainCam;
    private LayerMask _interactableLayer;

    private IInteractable _currentSelection;
    private IInteractable _currentHover;

    private TilePlacementManager _buildingManager;
    private GridManager _gridManager;

    public override void Initialize()
    {
        _mainCam = Camera.main;
        _interactableLayer = LayerMask.GetMask("Interactable");

        _gridManager = GameManager.Instance.GetManager<GridManager>();
        _buildingManager = GameManager.Instance.GetManager<TilePlacementManager>();
    }

    private void Update()
    {
        KeyboardInput();
        MouseInput();
    }

    private void KeyboardInput()
    {
        foreach (var mapping in keyMappingConfig.keyMappings)
        {
            if (Input.GetKeyDown(mapping.key))
            {
                HandleAction(mapping.actionName);
            }
        }
    }

    private void MouseInput()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

        HandleHover(ray);
        HandleClick(ray);
    }

    private void HandleHover(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable != _currentHover)
            {
                _currentHover?.OnHoverExit();
                _currentHover = interactable;
                _currentHover.OnHover();
            }
        }
        else
        {
            _currentHover?.OnHoverExit();
            _currentHover = null;
        }
    }

    private void HandleClick(Ray ray)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayer | LayerMask.GetMask("Ground")))
            {
                Vector3 hitPoint = hit.point;

                if (_buildingManager.SelectedTileType != TileType.BaseTile)
                {
                    _buildingManager.TryPlaceBuilding(hitPoint);
                }
                else
                {
                    Vector2Int gridPos = _gridManager.WorldToGrid(hitPoint);
                    Tile clickedTile = _gridManager.GetTileAt(gridPos);

                    if (clickedTile != null && clickedTile.TryGetComponent<IInteractable>(out var interactable))
                    {
                        _currentSelection?.OnDeselect();
                        _currentSelection = interactable;
                        _currentSelection.OnSelect(this);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _buildingManager.ClearSelection();
            _currentSelection?.OnDeselect();
            _currentSelection = null;
        }
    }

    private void HandleAction(string action)
    {
        switch (action)
        {
            case "BuildingOption#1":
                _buildingManager.SelectBuilding(TileType.House);
                break;
            case "BuildingOption#2":
                _buildingManager.SelectBuilding(TileType.Farm);
                break;
            case "BuildingOption#3":
                _buildingManager.SelectBuilding(TileType.Workshop);
                break;
            case "BuildingOption#4":
                _buildingManager.SelectBuilding(TileType.Temple);
                break;
            default:
                Debug.LogWarning($"Unhandled action: {action}");
                break;
        }
    }
}
