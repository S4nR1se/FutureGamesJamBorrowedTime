using System.Resources;
using UnityEngine;

public class TilePlacementManager : Manager
{
    [SerializeField] private TileDatabase_SO tileDatabase;

    [SerializeField] private ConstructionPopupUI _constructionPopupUI;
    private CanvasGroup _constructionPopupCanvas;

    private TilePreviewHelper _previewHelper;

    private GridManager _gridManager;
    private ResourceManager _resourceManager;

    private TileType _selectedTileType = TileType.BaseTile;

    public TileType SelectedTileType => _selectedTileType;

    public override void Initialize()
    {
        _constructionPopupCanvas = _constructionPopupUI.GetComponent<CanvasGroup>();
        _resourceManager = GameManager.Instance.GetManager<ResourceManager>();
        _gridManager = GameManager.Instance.GetManager<GridManager>();

        _previewHelper = GetComponent<TilePreviewHelper>();
        if(_previewHelper != null)
        {
            _previewHelper.Initialize(_gridManager);
        }
    }
    private void OnEnable()
    {
        PlayingState.OnPlayingStateUpdate += UpdatePreview;
    }
    private void OnDisable()
    {
        PlayingState.OnPlayingStateUpdate -= UpdatePreview;
    }
    private void UpdatePreview()
    {
        if (_previewHelper != null)
        {
            if (_selectedTileType != TileType.BaseTile)
            {
                BuildingData_SO data = tileDatabase.tiles
                    .Find(x => x.tileType == _selectedTileType)?.buildingData;

                if (data != null && data.PreviewPrefab != null)
                {
                    if (_previewHelper.CurrentPreview == null ||
                        !_previewHelper.CurrentPreview.name.StartsWith(data.PreviewPrefab.name))
                    {
                        _previewHelper.ShowPreview(data);
                    }

                    _previewHelper.UpdatePreview();
                }
            }
            else
            {
                _previewHelper.ClearPreview();
            }
        }
    }
    public void UpdateConstructionPreview(ConstructionSite site)
    {
        Debug.Log("flag1");
        _constructionPopupCanvas.alpha = 1;
        _constructionPopupUI.Initialize(site.BuildData, site.RemainingBuildTime);
        _constructionPopupUI.FollowMouse();
    }
    public void ClearConstructionPreview()
    {
        _constructionPopupCanvas.alpha = 0;
    }
    public void SelectBuilding(TileType tileType)
    {
        _selectedTileType = tileType;
        if (_previewHelper != null) _previewHelper.ClearPreview();
    }

    public void TryPlaceBuilding(Tile targetTile)
    {
        if (_selectedTileType == TileType.BaseTile || targetTile == null)
            return;

        Vector2Int gridPos = _gridManager.WorldToGrid(targetTile.transform.position);
        Tile existingTile = _gridManager.GetTileAt(gridPos);

        if (existingTile?.tileType != TileType.BaseTile)
        {
            return;
        }

        BuildingData_SO data = tileDatabase.tiles.Find(x => x.tileType == _selectedTileType)?.buildingData;
        if (data == null)
        {
            return;
        }

        int materialCost = data.MaterialCost;
        int currentMaterials = _resourceManager.GetValue(Resources.Materials);

        if (currentMaterials < materialCost)
        {
            return;
        }

        _resourceManager.UpdateValue(Resources.Materials, -materialCost);

        GameObject constructionPrefab = tileDatabase.GetPrefab(TileType.ConstructionSite);
        if (constructionPrefab == null)
            return;

        GameObject constructionGO = Instantiate(
            constructionPrefab,
            targetTile.transform.position,
            Quaternion.identity,
            _gridManager.transform
        );

        ConstructionSite constructionSite = constructionGO.GetComponent<ConstructionSite>();
        if (constructionSite != null)
        {
            if (data != null)
            {
                constructionSite.SetUpConstructionZone(targetTile, data.BuildTime, _selectedTileType, tileDatabase);
            }
        }

        _gridManager.SetTileOccupied(gridPos, true);

        Destroy(targetTile.gameObject);
    }

    public void TryUpgradeBuilding(Tile buildingTile)
    {
        if (buildingTile == null || _selectedTileType == buildingTile.tileType)
            return;

        Vector2Int gridPos = _gridManager.WorldToGrid(buildingTile.transform.position);
        Tile existingTile = _gridManager.GetTileAt(gridPos);

        
        if (existingTile?.tileType == TileType.BaseTile)
        {
            return;
        }

        BuildingData_SO data = tileDatabase.tiles.Find(x => x.tileType == _selectedTileType)?.buildingData.Upgrade;
        if (data == null)
        {
            return;
        }

        int materialCost = data.MaterialCost;
        int currentMaterials = _resourceManager.GetValue(Resources.Materials);

        if (currentMaterials < materialCost)
        {
            return;
        }

        if (data.PurrCost > 0)
        {
            int purrCost = data.PurrCost;
            int currentPurr = _resourceManager.GetValue(Resources.Purr);
            if (purrCost < currentPurr)
            {
                return;
            }
            _resourceManager.UpdateValue(Resources.Purr, -purrCost);
        }        

        _resourceManager.UpdateValue(Resources.Materials, -materialCost);

        GameObject constructionPrefab = tileDatabase.GetPrefab(TileType.ConstructionSite);
        if (constructionPrefab == null)
            return;

        GameObject constructionGO = Instantiate(
            constructionPrefab,
            buildingTile.transform.position,
            Quaternion.identity,
            _gridManager.transform
        );

        ConstructionSite constructionSite = constructionGO.GetComponent<ConstructionSite>();
        if (constructionSite != null)
        {
            if (data != null)
            {
                constructionSite.SetUpConstructionZone(buildingTile, data.BuildTime, _selectedTileType, tileDatabase);
            }
        }

        _gridManager.SetTileOccupied(gridPos, true);

        Destroy(buildingTile.gameObject);
    }

    public void ClearSelection()
    {
        _selectedTileType = TileType.BaseTile;
        if (_previewHelper != null) _previewHelper.ClearPreview();
    }
}
