using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreeperFSM : GroundEnemyFSM
{
    [Header("Creeper")] public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float explosionCoolDown;
    public float explosionTriggerRadius;
    public float patrolRestTime;
    public TNT tntPrefab;

    public float moveSpeed;
    public float chaseSpeed;


    protected override void Start()
    {
        base.Start();
        states[StateType.Patrol] = new CreeperPatrol(this); // idle
        states[StateType.Attack] = new CreeperAttack(this);
        TransitionState(StateType.Patrol);
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

        // explosionTriggerCircle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, explosionTriggerRadius);

        // hit box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, hitBoxCollider.size);
    }
}

public class CreeperPatrol : IState
{
    private CreeperFSM m;
    private float elapse;
    private bool isRest;

    public CreeperPatrol(CreeperFSM m)
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

        if (m.isWalkingDownCliff)
        {
            isRest = true;
            m.ReverseFacingDirection();
        }

        if (isRest)
        {
            elapse += Time.deltaTime;
            if (elapse > m.patrolRestTime)
            {
                elapse = 0;
                isRest = false;
            }
        }
    }

    public void OnFixedUpdate()
    {
        if (isRest)
        {
            m.rigidbody.velocity = Vector2.zero;
            return;
        }

        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.moveSpeed * m.facingDirection, m.xVelocityChangeSpeed);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
    }
}

public class CreeperAttack : IState
{
    private CreeperFSM m;
    private double elapse;

    public CreeperAttack(CreeperFSM m)
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
            m.TransitionState(StateType.Patrol);
            return;
        }

        if (elapse >= m.explosionCoolDown)
        {
            elapse -= m.explosionCoolDown;
            Shoot();
        }

        elapse += Time.deltaTime;
    }


    private void Shoot()
    {
    }


    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.chaseSpeed, m.xVelocityChangeSpeed);
        // var newY = Mathf.MoveTowards(m.rigidbody.velocity.y, , m.yVelocityChangeSpeed);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
    }
}