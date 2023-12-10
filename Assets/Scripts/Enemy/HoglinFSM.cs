using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoglinFSM : GroundEnemyFSM
{
    [Header("Move")] // 最无脑的生物
    public float moveSpeed;

    protected override void Start()
    {
        base.Start();
        states[StateType.Move] = new HoglinMove(this);
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

        // cliff
        Gizmos.DrawLine(position, position + transformDown * cliffCheckDownRaycastDist);

        // Hit Box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
    }
}

public class HoglinMove : IState
{
    private HoglinFSM m;
    private float elapse;
    private bool isRest;
    private bool isTurning;

    public HoglinMove(HoglinFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        if (m.isWalkingDownCliff || m.isHittingWall)
        {
            m.ReverseFacingDirection();
        }
    }

    public void OnFixedUpdate()
    {
        m.LerpVelocityX(m.facingDirection * m.moveSpeed);
    }

    public void OnExit()
    {
    }
}