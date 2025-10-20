using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour, IInteractable
{
    public TileType tileType;

    private Renderer _renderer;
    private Material _outlineMaterial;

    private bool _isOutlineActive = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer.materials.Length > 1)
            _outlineMaterial = _renderer.materials[1];
        DisableOutline();
    }
    public void OnSelect()
    {
        
    }
    public void OnDeselect()
    {
        
    }

    public void OnHover()
    {
        EnableOutline();
    }

    public void OnHoverExit()
    {
        DisableOutline();
    }
    private void EnableOutline()
    {
        if (_outlineMaterial != null && !_isOutlineActive)
        {
            _isOutlineActive = true;

            _outlineMaterial.SetFloat("_OutlineScale", 1.2f);
        }
    }

    private void DisableOutline()
    {
        if (_outlineMaterial != null && _isOutlineActive)
        {
            _isOutlineActive = false;

            _outlineMaterial.SetFloat("_OutlineScale", 0f);
        }
    }
}
public enum TileType
{
    Path,
    Building,
    NonWalkable,
}