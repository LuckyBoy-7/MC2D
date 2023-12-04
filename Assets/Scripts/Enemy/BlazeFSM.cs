using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlazeFSM : FlyEnemyFSM
{
    [Header("Blaze")] public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float shootCoolDown;
    public float shootSpeed;
    public FireBall fireBallPrefab;
    public float attackAngle;

    public float idleRadius;
    public float keepDistance;
    public Vector2 targetPos;
    public Vector2 pivotPos;
    public float moveSpeed;

    protected override void Start()
    {
        base.Start();
        states[StateType.Wait] = new BlazeWait(this); // idle
        states[StateType.Attack] = new BlazeAttack(this);
        TransitionState(StateType.Wait);
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
        if (PlayerFSM.instance != null && (PlayerFSM.instance.transform.position - transform.position).magnitude <=
            playerDetectionRadius)
        {
            // 如果player在攻击范围内，则画一条线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, PlayerFSM.instance.transform.position);
        }

        // detect player
        Gizmos.color = Color.green;
        var position = transform.position;
        Gizmos.DrawWireSphere(position, playerDetectionRadius);

        // hit box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, hitBoxCollider.size);

        // idle box
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pivotPos, idleRadius);
    }
}

public class BlazeWait : IState
{
    private BlazeFSM m;

    public BlazeWait(BlazeFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        var dir = (PlayerFSM.instance.transform.position - m.transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(m.transform.position, dir, m.playerDetectionRadius, m.viewLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
            m.TransitionState(StateType.Attack);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, 0, m.xVelocityChangeSpeed);
        var newY = Mathf.MoveTowards(m.rigidbody.velocity.y, 0, m.yVelocityChangeSpeed);
        m.rigidbody.velocity = new Vector2(newX, newY);
    }

    public void OnExit()
    {
    }
}

public class BlazeAttack : IState
{
    private BlazeFSM m;
    private double elapse;

    public BlazeAttack(BlazeFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        var dir = (PlayerFSM.instance.transform.position - m.transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(m.transform.position, dir, m.playerDetectionRadius, m.viewLayer);
        // player逃出范围或被墙挡住
        if (hit.collider == null || !hit.collider.CompareTag("Player"))
        {
            m.TransitionState(StateType.Wait);
            return;
        }

        m.pivotPos = -dir * m.keepDistance + PlayerFSM.instance.transform.position;
        if (m.hitBoxCollider.IsTouchingLayers(LayerMask.NameToLayer("Platform")))
            RollTargetPos();

        if (elapse >= m.shootCoolDown)
        {
            elapse -= m.shootCoolDown;
            Shoot();
        }

        elapse += Time.deltaTime;
    }

    private void RollTargetPos()
    {
        m.targetPos = m.pivotPos + Random.insideUnitCircle * m.idleRadius;
    }

    private void Shoot()
    {
        var dir1 = (PlayerFSM.instance.transform.position - m.transform.position).normalized;
        var dir2 = Quaternion.Euler(0, 0, m.attackAngle) * dir1;
        var dir3 = Quaternion.Euler(0, 0, -m.attackAngle) * dir1;
        var fireBall1 = BlazeFSM.Instantiate(m.fireBallPrefab, m.transform.position, Quaternion.identity);
        var fireBall2 = BlazeFSM.Instantiate(m.fireBallPrefab, m.transform.position, Quaternion.identity);
        var fireBall3 = BlazeFSM.Instantiate(m.fireBallPrefab, m.transform.position, Quaternion.identity);
        fireBall1.SetTrajectory(dir1 * m.shootSpeed);
        fireBall2.SetTrajectory(dir2 * m.shootSpeed);
        fireBall3.SetTrajectory(dir3 * m.shootSpeed);
    }


    public void OnFixedUpdate()
    {
        if ((m.transform.position - PlayerFSM.instance.transform.position).magnitude < 1e-3)
            RollTargetPos();
        else
        {
            var targetSpeed = (m.targetPos - (Vector2)m.transform.position).normalized * m.moveSpeed;
            m.LerpVelocityX(targetSpeed.x);
            m.LerpVelocityY(targetSpeed.y);
        }
    }

    public void OnExit()
    {
    }
}