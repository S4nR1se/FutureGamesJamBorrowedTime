using UnityEngine;

public class Builder : MonoBehaviour
{
    [SerializeField] private GameObject[] tilePrefabs;
    private GridManager _gridManager;

    public void Initialize(GridManager gridManager)
    {
        _gridManager = gridManager;
    }

    public void PlaceTile()
    {

    }
}
