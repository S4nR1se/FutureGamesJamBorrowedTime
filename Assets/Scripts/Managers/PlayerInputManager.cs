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
    [SerializeField] private KeyMapping[] keyMappings;

    private Camera _mainCam;
    private LayerMask _interactableLayer;

    private IInteractable _currentSelection;
    private IInteractable _currentHover;

    public override void Initialize()
    {
        _mainCam = Camera.main;
        _interactableLayer = LayerMask.GetMask("Interactable");
    }
    private void Update()
    {
        KeyboardInput();
        MouseInput();
    }
    private void KeyboardInput()
    {
        foreach (var mapping in keyMappings)
        {
            if (Input.GetKeyDown(mapping.key))
            {
                HandleAction(mapping.actionName);
            }
        }
    }
    private void MouseInput()
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
       
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

        HandleHover(ray);
        HandleSelection(ray);
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
    private void HandleSelection(Ray ray)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _interactableLayer))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    _currentSelection?.OnDeselect();
                    _currentSelection = interactable;
                    _currentSelection.OnSelect();
                }
            }
            else
            {
                _currentSelection?.OnDeselect();
                _currentSelection = null;
            }
        }
    }
    private void HandleAction(string action)
    {
        switch (action)
        {
            default:
                break;
        }
    }
}
