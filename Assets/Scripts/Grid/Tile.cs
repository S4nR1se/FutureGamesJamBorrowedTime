using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour, IInteractable
{
    public TileType tileType;
    public GameObject Component => gameObject;

    public void OnDeselect()
    {

    }

    public void OnHover()
    {

    }

    public void OnHoverExit()
    {

    }

    public void OnSelect(PlayerInputManager playerInputManager)
    {

    }
}
public enum TileType
{
    BaseTile,
    Farm,
    House,
    Workshop,
    Temple,
    Graveyard,
    ConstructionSite,
}