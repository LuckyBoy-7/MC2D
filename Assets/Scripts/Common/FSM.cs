using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public Parameter parameter;
    public Dictionary<StateType, IState> states = new();
    public IState currentState;

    private void Update()
    {
        currentState.OnUpdate();
    }

    public void TransitionState(StateType stateType)
    {
        currentState?.OnExit();
        currentState = states[stateType];
        currentState.OnEnter();
    }
}