using UnityEngine;

public interface IInteractable
{
    GameObject Component { get; }
    void OnSelect();
    void OnDeselect();
    void OnHover();
    void OnHoverExit();
}
