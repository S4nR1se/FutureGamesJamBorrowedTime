using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _placedGameObjects = new();

    public int PlaceObject(GameObject Prefab, Vector3 GridPosition)
    {
        GameObject NewTileObject = Instantiate(Prefab);
        NewTileObject.transform.position = GridPosition;
        _placedGameObjects.Add(NewTileObject);
        return _placedGameObjects.Count - 1;
    }
}
