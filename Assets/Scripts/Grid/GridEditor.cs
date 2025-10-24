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
        GameObject gridParent = gridManager.gameObject;

        for (int x = 0; x < gridManager.GridSize; x++)
        {
            for (int y = 0; y < gridManager.GridSize; y++)
            {
                Vector3 worldPos = gridManager.gridOrigin + new Vector3(x * gridManager.CellSize, 0, y * gridManager.CellSize);

                GameObject tile = (GameObject)PrefabUtility.InstantiatePrefab(gridManager.TilePrefab, gridParent.transform);
                tile.transform.position = worldPos;

                Tile tileComponent = tile.GetComponent<Tile>();
                if (tileComponent == null)
                    tileComponent = tile.AddComponent<Tile>();

                tileComponent.tileType = TileType.BaseTile;
            }
        }
        EditorUtility.SetDirty(gridManager);
    }

    private void ClearGrid(GridManager gridManager)
    {
        while (gridManager.transform.childCount > 0)
        {
            DestroyImmediate(gridManager.transform.GetChild(0).gameObject);
        }
    }
}