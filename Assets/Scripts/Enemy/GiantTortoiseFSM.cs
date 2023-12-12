using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantTortoiseFSM : GroundEnemyFSM
{
    [Header("Move")] // 最无脑的生物
    public float moveSpeed;

    protected override void Start()
    {
        base.Start();
        states[StateType.Move] = new GiantTortoiseMove(this);
        TransitionState(StateType.Move);
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
        var bottomLeft = position + transformDown * box.size.y / 2 - transform.right * box.size.x / 2;
        var bottomRight = position + transformDown * box.size.y / 2 + transform.right * box.size.x / 2;
        // cliff
        Gizmos.DrawLine(bottomLeft, bottomLeft + transformDown * cliffCheckDownRaycastDist);
        Gizmos.DrawLine(bottomRight, bottomRight + transformDown * cliffCheckDownRaycastDist);
    }

    public override void Attacked(int damage, Vector2 attackForceVec = default)
    {
        if (Mathf.Sign(PlayerFSM.instance.transform.position.x - transform.position.x) == Mathf.Sign(facingDirection))
        {
            base.Attacked(damage, attackForceVec);
        }
    }
}

public class GiantTortoiseMove : IState
{
    private GiantTortoiseFSM m;
    private float elapse;
    private bool isRest;
    private bool isTurning;

    public GiantTortoiseMove(GiantTortoiseFSM m)
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
            Debug.Log(11111);
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