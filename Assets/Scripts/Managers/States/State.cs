using UnityEngine;

public class State
{
    protected StateMachine _stateMachine;
    protected Transform _transform;
    protected GameObject _gameObject;

    public virtual void Initialize(StateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _transform = stateMachine.transform;
        _gameObject = stateMachine.gameObject;
    }

    public virtual void EnterState() { }
    public virtual void UpdateState() { }
    public virtual void FixedUpdateState() { }
    public virtual void ExitState() { }
}
