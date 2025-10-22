using UnityEngine;

public class TilePreviewHelper : MonoBehaviour
{
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    public GameObject CurrentPreview => currentPreview;
    private GameObject currentPreview;

    private GridManager _gridManager;
    private Camera _mainCam;

    private float _yOffset;
    private float _rotationY;

    public void Initialize(GridManager gridManager)
    {
        _gridManager = gridManager;
        _mainCam = Camera.main;
    }

    public void ShowPreview(GameObject prefab, float yOffset, float rotationY)
    {
        ClearPreview();
        _yOffset = yOffset;
        _rotationY = rotationY;

        currentPreview = Instantiate(prefab, Vector3.zero, Quaternion.Euler(0, _rotationY, 0), transform);

        foreach (var col in currentPreview.GetComponentsInChildren<Collider>())
            col.enabled = false;
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
    }
}
