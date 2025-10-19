using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager: MonoBehaviour
{
    [SerializeField] private Camera _sceneCamera;
    [SerializeField] private LayerMask _placementLayerMask;

    private Vector3 _LastPosition;

    public event Action OnClicked, OnExit;

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            OnClicked?.Invoke();
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnExit?.Invoke();
        }
    }

    public bool IsPointerOverUI()
        => EventSystem.current.IsPointerOverGameObject();
    

    public Vector3 GetSelectedMapPosition()
    {
        Vector3 MousePos = Input.mousePosition;
        MousePos.z = _sceneCamera.nearClipPlane;
        Ray Ray = _sceneCamera.ScreenPointToRay(MousePos);
        RaycastHit Hit;

        if(Physics.Raycast(Ray,out Hit, 1000, _placementLayerMask))
        {
            _LastPosition = Hit.point;
        }

        return _LastPosition;
    }
}
