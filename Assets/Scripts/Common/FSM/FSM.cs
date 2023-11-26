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

public class SingletonFSM<T> : MonoBehaviour where T: SingletonFSM<T>
{
    public static T instance;

    protected virtual void Awake()
    {
        if (instance == null)
            instance = (T)this;
        else
            Destroy(gameObject);
    }
    public Dictionary<StateType, IState> states = new();
    protected IState currentState;

    public void TransitionState(StateType stateType)
    {
        currentState?.OnExit();
        currentState = states[stateType];
        currentState.OnEnter();
    }
}