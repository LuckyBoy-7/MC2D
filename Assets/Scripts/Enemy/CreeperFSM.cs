using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEditor.Rendering;
using UnityEngine;

public class CreeperFSM : GroundEnemyFSM
{
    [Header("Creeper")] public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float explosionCoolDown;
    public float explosionTriggerRadius;
    public float patrolRestTime;
    public Rigidbody2D tntPrefab;
    public float onDieSplillTNTForce;

    public float moveSpeed;
    public float chaseSpeed;


    protected override void Start()
    {
        base.Start();
        states[StateType.Patrol] = new CreeperPatrol(this); // idle
        states[StateType.Attack] = new CreeperAttack(this);
        TransitionState(StateType.Patrol);

        onKill += SpillTNT;
    }


    public void SpillTNT()
    {
        var left = Instantiate(tntPrefab, transform.position, Quaternion.identity);
        var mid = Instantiate(tntPrefab, transform.position, Quaternion.identity);
        var right = Instantiate(tntPrefab, transform.position, Quaternion.identity);
        left.AddForce(new Vector2(-1, 1) * onDieSplillTNTForce, ForceMode2D.Impulse);
        mid.AddForce(new Vector2(0, 1) * onDieSplillTNTForce, ForceMode2D.Impulse);
        right.AddForce(new Vector2(1, 1) * onDieSplillTNTForce, ForceMode2D.Impulse);
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
    private bool isResting;

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
            isResting = true;

        if (isResting)
        {
            elapse += Time.deltaTime;
            if (elapse > m.patrolRestTime)
            {
                elapse = 0;
                isResting = false;
                m.ReverseFacingDirection();
            }
        }
    }

    public void OnFixedUpdate()
    {
        if (isResting)
        {
            m.rigidbody.velocity = Vector2.zero;
            return;
        }

        m.LerpVelocityX(m.moveSpeed * m.facingDirection);
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

        if (elapse >= m.explosionCoolDown) // 时间到了，但player离太远了，先憋着
        {
            if ((PlayerFSM.instance.transform.position - m.transform.position).magnitude < m.explosionTriggerRadius)
            {
                elapse -= m.explosionCoolDown;
                ReleaseTNT();
            }
        }

        elapse += Time.deltaTime;
    }

    public void ReleaseTNT() => Object.Instantiate(m.tntPrefab, m.transform.position, Quaternion.identity);

    public void OnFixedUpdate()
    {
        m.LerpVelocityX(Mathf.Sign(PlayerFSM.instance.transform.position.x - m.transform.position.x) * m.chaseSpeed);
    }

    public void OnExit()
    {
    }
}

