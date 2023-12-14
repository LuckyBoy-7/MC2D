using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderFSM : GroundEnemyFSM
{
    public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float jumpCoolDown;
    public float jumpForce;
    public float jumpForceYCompensation;

    protected override void Start()
    {
        base.Start();
        states[StateType.Wait] = new SpiderWait(this);
        states[StateType.Attack] = new SpiderAttack(this);
        TransitionState(StateType.Wait);
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

        // Player detection
        Gizmos.color = Color.green;
        var position = transform.position;
        Gizmos.DrawWireSphere(position, playerDetectionRadius);

        // hitBox
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, hitBoxCollider.size);
    }

    public bool isOnGround =>
        Physics2D.OverlapBox(transform.position + Vector3.down * 0.55f,
            new Vector2(1f, 0.1f), 0, groundLayer);
}

public class SpiderWait : IState
{
    private SpiderFSM m;

    public SpiderWait(SpiderFSM m)
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
    }

    public void OnExit()
    {
    }
}

public class SpiderAttack : IState
{
    private SpiderFSM m;

    private float elapse;

    public SpiderAttack(SpiderFSM m)
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

        if (elapse >= m.jumpCoolDown && m.isOnGround) // 在地上才能攻击
        {
            elapse -= m.jumpCoolDown;
            Jump();
        }

        elapse += Time.deltaTime;
    }

    private void Jump()
    {
        var dir = (PlayerFSM.instance.transform.position - m.transform.position).normalized;
        // if (PlayerFSM.instance.isOnGround)  // player在地上时才补偿向上的力
        dir += m.jumpForceYCompensation * Vector3.up;
        m.rigidbody.AddForce(dir * m.jumpForce, ForceMode2D.Impulse);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
    }
}