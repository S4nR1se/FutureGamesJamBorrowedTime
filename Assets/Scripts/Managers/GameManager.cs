using UnityEngine;

public class GameManager : StateMachine
{
    private void Awake()
    {
        RegisterState(new PlayingState());

        SwitchState<PlayingState>();
    }
}
