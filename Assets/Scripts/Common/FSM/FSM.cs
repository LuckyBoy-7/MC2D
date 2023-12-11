using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
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
    public StateType currentStateType;
    public StateType preStateType;

    public void TransitionState(StateType stateType)
    {
        currentState?.OnExit();
        preStateType = currentStateType;
        currentStateType = stateType;
        currentState = states[stateType];
        currentState.OnEnter();
    }
}