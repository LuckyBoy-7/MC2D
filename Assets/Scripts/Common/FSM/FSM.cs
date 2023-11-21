using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public Dictionary<StateType, IState> states = new();
    protected IState currentState;


    public void TransitionState(StateType stateType)
    {
        currentState?.OnExit();
        currentState = states[stateType];
        currentState.OnEnter();
    }
}