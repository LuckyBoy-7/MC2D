using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;

public class PufferFSM : GroundEnemyFSM
{
    [Header("Move")] // 最无脑的生物
    public float moveSpeed;

    public float restTime;

    [Header("Check")] public float guardTriggerRadius;
    public GameObject guardHelmet;
    

    protected override void Start()
    {
        base.Start();
        states[StateType.Move] = new PufferMove(this);
        states[StateType.Guard] = new PufferGuard(this);
        TransitionState(StateType.Move);
    }

    private void Update()
    {
        currentState.OnUpdate();
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Cliff Raycast check
        var position = transform.position;
        var scale = transform.localScale;
        var box = hitBoxCollider;
        var (width, height) = (box.bounds.max.x - box.bounds.min.x, box.bounds.max.y - box.bounds.min.y);
        var (w, h) = (width / 2, height / 2);
        var transformDown = Quaternion.Euler(0, 0, -90) * transform.right;

        var bottomLeft = position + transformDown * box.size.y / 2 - transform.right * box.size.x / 2;
        var bottomRight = position + transformDown * box.size.y / 2 + transform.right * box.size.x / 2;
        // cliff
        Gizmos.DrawLine(bottomLeft, bottomLeft + transformDown * cliffCheckDownRaycastDist);
        Gizmos.DrawLine(bottomRight, bottomRight + transformDown * cliffCheckDownRaycastDist);
    }
}

public class PufferMove : IState
{
    private PufferFSM m;
    private float elapse;
    private bool isRest;

    public PufferMove(PufferFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        if (isRest)
        {
            elapse += Time.deltaTime;
            if (elapse > m.restTime)
            {
                elapse = 0;
                isRest = false;
            }

            return;
        }

        m.rigidbody.velocity = m.facingDirection * m.transform.right * m.moveSpeed;
        if (m.isWalkingDownCliff)
        {
            isRest = true;
            m.rigidbody.velocity = Vector2.zero;
            m.ReverseFacingDirection();
        }
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        elapse = 0;
    }
}

public class PufferGuard : IState
{
    private PufferFSM m;
    private float elapse;
    private bool isRest;

    public PufferGuard(PufferFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        m.rigidbody.velocity = m.facingDirection * m.transform.right * m.moveSpeed;
        if (m.isWalkingDownCliff)
        {
            isRest = true;
            m.rigidbody.velocity = Vector2.zero;
            m.ReverseFacingDirection();
        }
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        elapse = 0;
    }
}