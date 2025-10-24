using System;

public class InitializingState : State
{
    public static event Action OnEnterInitializingState;

    public override void EnterState()
    {
        OnEnterInitializingState?.Invoke();
        GameManager.Instance.SwitchState<PlayingState>();
    }
}
