using UnityEngine;

public class TilePreviewHelper : MonoBehaviour
{
    [SerializeField] private PlacementPopupUI popupUI;

    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    public GameObject CurrentPreview => currentPreview;
    private GameObject currentPreview;

    private GridManager _gridManager;
    private Camera _mainCam;

    private CanvasGroup _popUpCanvasGroup;

    private float _yOffset;
    private float _rotationY;

    public void Initialize(GridManager gridManager)
    {
        _gridManager = gridManager;
        _mainCam = Camera.main;

        _popUpCanvasGroup = popupUI.GetComponent<CanvasGroup>();
        _popUpCanvasGroup.alpha = 0f;
    }

    public void ShowPreview(BuildingData_SO buildData)
    {
        ClearPreview();
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
        Material mat = canPlace ? validMaterial : invalidMaterial;

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
