using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileDatabase_SO", menuName = "Scriptable Objects/TileDatabase_SO")]
public class TileDatabase_SO : ScriptableObject
{
    [System.Serializable]
    public class TileEntry
    {
        public TileType tileType;
        public GameObject prefab;
    }

    public List<TileEntry> tiles = new List<TileEntry>();

    public GameObject GetPrefab(TileType type)
    {
        foreach (var entry in tiles)
        {
            if (entry.tileType == type)
                return entry.prefab;
        }
        return null;
    }
}
