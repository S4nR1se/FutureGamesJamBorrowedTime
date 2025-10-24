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
    private IInteractable _previousSelection;
    private IWorker _previousWorkerSelection;
    private IInteractable _currentHover;

    private TilePlacementManager _buildingManager;
    private UIManager _UIManager;
    private GridManager _gridManager;
    private TimeManager _timeManager;

    public GameObject CurrentSelection => _currentSelection.Component;
    public IWorker PreviousWorkerSelection
    {
        get { return _previousWorkerSelection; }
    }

    public override void Initialize()
    {
        _mainCam = Camera.main;
        _interactableLayer = LayerMask.GetMask("Interactable");

        _gridManager = GameManager.Instance.GetManager<GridManager>();
        _buildingManager = GameManager.Instance.GetManager<TilePlacementManager>();
        _UIManager = GameManager.Instance.GetManager<UIManager>();
        _timeManager = GameManager.Instance.GetManager<TimeManager>();
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
        HandleRightClick();
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
        if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1))
            return;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayer | LayerMask.GetMask("Ground")))
        {
            var interactables = hit.collider.GetComponentsInParent<IInteractable>();

            IInteractable interactable = null;

            foreach (var i in interactables)
            {
                if (i is NPC npc)
                {
                    if (npc.GetOccupiedZone() != null && npc.GetOccupiedZone().Type != ZoneType.ConstructionSite)
                    {
                        continue;
                    }
                    interactable = i;
                }
                else if (i is Building)
                {
                    interactable = i;
                    break;
                }
                else if (i is Tile)
                {
                    interactable = i;
                }
                else if (i is IWorker)
                {
                    interactable = i;
                }
            }

            if (interactable != null)
            {
                if (Input.GetMouseButtonDown(0) && interactable is Tile tile && _buildingManager.SelectedTileType != TileType.BaseTile)
                {
                    _buildingManager.TryPlaceBuilding(tile);
                }
                else
                {
                    _currentSelection?.OnDeselect();

                    if (_currentSelection != null)
                        _previousSelection = _currentSelection;

                    if (_currentSelection is IWorker prevWorker)
                        _previousWorkerSelection = prevWorker;

                    _currentSelection = interactable;
                    _currentSelection.OnSelect(this);

                    if (_currentSelection is NPC selectedNPC)
                    {
                        _UIManager.DisplayNPCInfo(selectedNPC);
                    }
                    else
                    {
                        _UIManager.HideNPCInfo();
                    }
                }
            }
            else
            {
                _buildingManager.ClearSelection();
                _currentSelection?.OnDeselect();

                if (_currentSelection != null)
                    _previousSelection = _currentSelection;

                _currentSelection = null;
                _UIManager.HideNPCInfo();
            }
        }

        if (_previousWorkerSelection != null && _currentSelection is not IWorker)
        {
            _previousWorkerSelection = null;
        }
    }
    private void HandleRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            _buildingManager.ClearSelection();
            _currentSelection?.OnDeselect();

            if (_currentSelection != null)
                _previousSelection = _currentSelection;

            _currentSelection = null;
            _previousWorkerSelection = null;
            _UIManager.HideNPCInfo();
            _UIManager.HideAllInfo();
        }
    }
    public GameObject GetPreviousSelection()
    {
        if (_previousSelection == null)
            return null;

        GameObject prevGO = _previousSelection.Component;

        if (prevGO == null)
        {
            _previousSelection = null;
            return null;
        }

        return prevGO;
    }
    public void ClearPreviousSelection()
    {
        _previousSelection = null;
    }
    public bool TryGetPreviousWorkerSelection(out IWorker previousWorker)
    {
        previousWorker = _previousWorkerSelection;
        return previousWorker != null;
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
            case "PassTime":
                _timeManager.PassTime();
                break;
            default:
                Debug.LogWarning($"Unhandled action: {action}");
                break;
        }
    }
}
