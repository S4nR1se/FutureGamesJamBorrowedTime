using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridManager gridManager = (GridManager)target;

        if (GUILayout.Button("Generate Grid"))
        {
            GenerateGrid(gridManager);
        }

        if (GUILayout.Button("Clear Grid"))
        {
            ClearGrid(gridManager);
        }
    }

    private void GenerateGrid(GridManager gridManager)
    {
        ClearGrid(gridManager);

        if (gridManager.BaseTilePrefabs == null || gridManager.BaseTilePrefabs.Length == 0)
        {
            Debug.LogError("No base tile prefabs assigned! Please assign at least one base tile prefab.");
            return;
        }

        GameObject gridParent = gridManager.gameObject;

        for (int x = 0; x < gridManager.GridSize; x++)
        {
            for (int y = 0; y < gridManager.GridSize; y++)
            {
                Vector3 worldPos = gridManager.gridOrigin + new Vector3(x * gridManager.CellSize, 0, y * gridManager.CellSize);
                GameObject selectedPrefab = GetWeightedRandomPrefab(gridManager);

                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, gridParent.transform);
                tile.transform.position = worldPos;

                Tile tileComponent = tile.GetComponent<Tile>();
                if (tileComponent == null)
                    tileComponent = tile.AddComponent<Tile>();

                tileComponent.tileType = TileType.BaseTile;

                CallDecorationPlacement(gridManager, tile.transform);
            }
        }

        EditorUtility.SetDirty(gridManager);
    }

    private GameObject GetWeightedRandomPrefab(GridManager gridManager)
    {
        if (gridManager.BaseTilePrefabs.Length == 1)
            return gridManager.BaseTilePrefabs[0];

        float randomValue = Random.value;
        if (randomValue < gridManager.FirstPrefabWeight)
        {
            return gridManager.BaseTilePrefabs[0];
        }
        else
        {
            int otherIndex = Random.Range(1, gridManager.BaseTilePrefabs.Length);
            return gridManager.BaseTilePrefabs[otherIndex];
        }
    }

    private void CallDecorationPlacement(GridManager gridManager, Transform tileTransform)
    {
        gridManager.TryPlaceDecoration(tileTransform);
    }

    private void ClearGrid(GridManager gridManager)
    {
        while (gridManager.transform.childCount > 0)
        {
            DestroyImmediate(gridManager.transform.GetChild(0).gameObject);
        }
    }
}