using UnityEngine;

public class TilePreviewHelper : MonoBehaviour
{
    [SerializeField] private PlacementPopupUI popupUI;

    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;
    [SerializeField] private Material FreebieMaterial;

    public GameObject CurrentPreview => currentPreview;
    private GameObject currentPreview;

    private GridManager _gridManager;
    private ResourceManager _resourceManager;

    private Camera _mainCam;

    private CanvasGroup _popUpCanvasGroup;

    private BuildingData_SO _currentData;

    private float _yOffset;
    private float _rotationY;

    public void Initialize(GridManager gridManager)
    {
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _gridManager = gridManager;
        _mainCam = Camera.main;

        _popUpCanvasGroup = popupUI.GetComponent<CanvasGroup>();
        _popUpCanvasGroup.alpha = 0f;
    }

    public void ShowPreview(BuildingData_SO buildData)
    {
        ClearPreview();
        _currentData = buildData;
        _yOffset = buildData.PlacementYOffset;
        _rotationY = buildData.DefaultRotationY;
        GameObject prefab = buildData.PreviewPrefab;

        currentPreview = Instantiate(prefab, new Vector3(0, 100000, 0), Quaternion.Euler(0, _rotationY, 0), transform);

        foreach (var col in currentPreview.GetComponentsInChildren<Collider>())
            col.enabled = false;

        popupUI.Initialize(buildData);
        _popUpCanvasGroup.alpha = 1f;
    }

    public void UpdatePreview()
    {
        if (currentPreview == null) return;

        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2Int gridPos = _gridManager.WorldToGrid(hit.point);
            Vector3 snappedPos = _gridManager.GridToWorld(gridPos) + new Vector3(0, _yOffset, 0);

            currentPreview.transform.position = snappedPos;

            bool canPlace = _gridManager.IsAreaFree(gridPos, Vector2Int.one);
            ApplyMaterial(canPlace);

            popupUI.FollowMouse();
        }
    }

    private void ApplyMaterial(bool canPlace)
    {
        if (_currentData == null) return;

        TilePlacementManager placementManager = GameManager.Instance.GetManager<TilePlacementManager>();
        bool isFirstOfType = placementManager != null && !placementManager.HasBuiltType(_currentData.type);

        bool hasEnoughMaterials = isFirstOfType ||
                                  _resourceManager.GetValue(Resources.Materials) >= _currentData.MaterialCost;

        Material mat;

        if (!canPlace)
        {
            mat = invalidMaterial;
        }
        else if (isFirstOfType)
        {
            mat = FreebieMaterial;
        }
        else if (hasEnoughMaterials)
        {
            mat = validMaterial;
        }
        else
        {
            mat = invalidMaterial;
        }

        foreach (Renderer r in currentPreview.GetComponentsInChildren<Renderer>())
            r.material = mat;
    }
    public void ClearPreview()
    {
        if (currentPreview != null)
            Destroy(currentPreview);
        _popUpCanvasGroup.alpha = 0f;
    }
}
