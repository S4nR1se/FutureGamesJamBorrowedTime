using UnityEngine;

public interface IInteractable
{
    GameObject Component { get; }
    void OnSelect(PlayerInputManager playerInputManager);
    void OnDeselect();
    void OnHover();
    void OnHoverExit();
}
