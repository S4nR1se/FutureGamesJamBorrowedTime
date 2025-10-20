using UnityEngine;

public class Tile : MonoBehaviour
{
    public TileType tileType;
}
public enum TileType
{
    Path,
    Building,
    NonWalkable,
}