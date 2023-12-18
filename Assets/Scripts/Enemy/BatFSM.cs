using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BatFSM : EnemyFSM
{
    public float idleRadius;
    public Vector3 targetPos;
    public Vector3 pivotPos;
    public float moveSpeed;

    protected override void Start()
    {
        base.Start();
        states[StateType.Idle] = new BatIdleState(this);
        TransitionState(StateType.Idle);
        pivotPos = transform.position;
        RollTargetPos();
    }

    public void RollTargetPos() => targetPos = pivotPos + Random.insideUnitSphere * idleRadius;
}

public class BatIdleState : IState
{
    private BatFSM m;

    public BatIdleState(BatFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        m.transform.position = Vector3.MoveTowards(m.transform.position, m.targetPos, m.moveSpeed * Time.deltaTime);
        if ((m.transform.position - m.targetPos).magnitude < 1e-3)
            m.RollTargetPos();
    }

    public void OnFixedUpdate()
    {
        m.LerpVelocity(Vector2.zero);
    }

    public void OnExit()
    {
    }
}