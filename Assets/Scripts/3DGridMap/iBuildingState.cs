using UnityEngine;

public interface iBuildingState
{
    void EndState();
    void OnAction(Vector3Int GridPosition);
    void UpdateState(Vector3Int GridPosition);
}