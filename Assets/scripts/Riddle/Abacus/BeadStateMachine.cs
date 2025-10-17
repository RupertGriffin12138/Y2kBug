using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class BeadStateMachine
{
    public BeadState currentState { get; private set; }
    public void Initialize(BeadState _startState)
    {
        currentState = _startState;
        currentState.Enter();
    }

    public void ChangeState(BeadState _newState)
    {
        currentState.Exit();
        currentState = _newState;
        currentState.Enter();
    }
}
