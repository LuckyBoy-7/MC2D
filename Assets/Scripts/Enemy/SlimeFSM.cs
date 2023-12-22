using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SlimeFSM : GroundEnemyFSM
{
    public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float jumpCoolDown;
    public float jumpForce;
    public float jumpForceYCompensation;
    public Rigidbody2D subSlimePrefab;
    public float emitForce;


    protected override void Start()
    {
        onKill += () =>
        {
            if (subSlimePrefab != null)
            {
                EmitSlime(2);
            }
        };

        base.Start();
        states[StateType.Wait] = new SlimeWait(this);
        states[StateType.Attack] = new SlimeAttack(this);
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
        Gizmos.DrawWireCube(position, hitBoxCollider.size * transform.localScale);
    }

    private void EmitSlime(int num)
    {
        for (int i = 0; i < num; i++)
        {
            var force = Random.insideUnitCircle * emitForce;
            var slime = Instantiate(subSlimePrefab, transform.position, Quaternion.identity);
            slime.GetComponent<SlimeFSM>().canBeLooted = canBeLooted;
            slime.AddForce(force, ForceMode2D.Impulse);
            slime.AddForce(rigidbody.velocity, ForceMode2D.Impulse);  // 带上父物体的惯性
        }
    }
}

public class SlimeWait : IState
{
    private SlimeFSM m;

    public SlimeWait(SlimeFSM m)
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

public class SlimeAttack : IState
{
    private SlimeFSM m;

    private float elapse;

    public SlimeAttack(SlimeFSM m)
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