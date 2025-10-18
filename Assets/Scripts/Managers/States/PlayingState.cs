using System;
using UnityEngine;

public class PlayingState : State
{
    public static event Action OnEnterPlayingState;
    public static event Action OnPlayingStateUpdate;
    public static event Action OnPlayingStateFixedUpdate;
    public static event Action OnExitPlayingState;

    public override void EnterState()
    {
        OnEnterPlayingState?.Invoke();
    }
    public override void UpdateState()
    {
        OnPlayingStateUpdate?.Invoke();
    }
    public override void FixedUpdateState()
    {
        OnPlayingStateFixedUpdate?.Invoke();
    }
    public override void ExitState()
    {
        OnExitPlayingState?.Invoke();
    }
}
