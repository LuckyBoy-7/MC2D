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

    public override bool Attacked(int damage, Vector2 attackForceVec = default) // 被击打的方向加力度
    {
        if (currentState == states[StateType.Guard])
            return false;
        TakeDamage(damage);
        return true;
    }

    private void OnDrawGizmos()
    {
        var center = GetColliderCenter(out float width, out float height, out float w, out float h,
            out Vector3 right,
            out Vector3 left, out Vector3 down);
        Gizmos.color = Color.green;
        // Cliff Raycast check
        var position = transform.position;
        var scale = transform.localScale;
        var box = hitBoxCollider;


        var bottomLeft = position + down * h - right * w;
        var bottomRight = position + down * h + right * w;
        // cliff
        Gizmos.DrawLine(bottomLeft, bottomLeft + down * cliffCheckDownRaycastDist);
        Gizmos.DrawLine(bottomRight, bottomRight + down * cliffCheckDownRaycastDist);

        // guard
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, guardTriggerRadius);
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
        if ((m.transform.position - PlayerFSM.instance.transform.position).magnitude < m.guardTriggerRadius)
            m.TransitionState(StateType.Guard);
        if (isRest)
        {
            elapse += Time.deltaTime;
            if (elapse > m.restTime)
            {
                elapse = 0;
                m.ReverseFacingDirection();
                isRest = false;
            }

            return;
        }

        m.rigidbody.velocity = m.transform.right * (m.moveSpeed * m.facingDirection);
        if (m.isWalkingDownCliff)
        {
            isRest = true;
            m.rigidbody.velocity = Vector2.zero;
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

    public PufferGuard(PufferFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = Vector2.zero;
        m.guardHelmet.SetActive(true);
    }

    public void OnUpdate()
    {
        if ((m.transform.position - PlayerFSM.instance.transform.position).magnitude > m.guardTriggerRadius)
            m.TransitionState(StateType.Move);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.guardHelmet.SetActive(false);
    }
}