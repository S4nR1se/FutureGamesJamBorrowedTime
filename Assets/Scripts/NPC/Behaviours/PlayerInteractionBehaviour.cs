using UnityEngine;
using UnityEngine.UIElements;

public class PlayerInteractionBehaviour : Behaviour
{
    private Renderer _meshRenderer;

    private Material _outlineMaterial;

    private bool _isOutlineActive = false;
    private bool _isSelected = false;

    private bool _useAltClickSound = false;

    public PlayerInteractionBehaviour(Renderer meshRenderer)
    {
        _meshRenderer = meshRenderer;

        if (_meshRenderer.materials.Length > 1)
            _outlineMaterial = _meshRenderer.materials[1];
        DisableOutline();
    }
    public void OnSelect(PlayerInputManager playerInputManager)
    {
        _isSelected = true;
        EnableOutline();

        SoundManager.Instance.PlaySound("ClickOnPeasant_v1", transform.position);
    }
    public void OnDeselect()
    {
        _isSelected = false;
        DisableOutline();
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

            _outlineMaterial.SetFloat("_OutlineScale", 1.1f);
        }
    }

    private void DisableOutline()
    {
        if (_isSelected) return;
        if (_outlineMaterial != null && _isOutlineActive)
        {
            _isOutlineActive = false;

            _outlineMaterial.SetFloat("_OutlineScale", 0f);
        }
    }
}
