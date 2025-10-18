using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private Dictionary<Type, State> _states = new Dictionary<Type, State>();
    private State _currentState;

    protected void RegisterState(State state)
    {
        state.Initialize(this);
        _states[state.GetType()] = state;
    }
    public void SwitchState<TState>() where TState : State
    {
        Type stateType = typeof(TState);

        if (_states.TryGetValue(stateType, out State newState))
        {
            _currentState?.ExitState();
            _currentState = newState;
            _currentState.EnterState();
        }
        else
        {
            Debug.LogWarning($"State {stateType.Name} not registered!");
        }
    }
    public virtual void UpdateStateMachine()
    {
        _currentState?.UpdateState();
    }
    public virtual void FixedUpdateStateMachine()
    {
        _currentState?.FixedUpdateState();
    }
    public bool IsState<TState>() where TState: State
    {
        return _currentState != null && _currentState.GetType() == typeof(TState);
    }
}
