using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SlimeFSM : GroundEnemyFSM
{
    public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float jumpCoolDown;
    public float jumpForce;
    public float jumpForceYCompensation;
    public SlimeFSM subSlimePrefab;
    public float emitForce;
    public float emitAngle = 70;

    public float beSuckedProtectionTime = 0.1f; // 防止一生成就被slimeBoss Suck了
    public float beSuckedProtectionExpireTime;

    public bool canBeSucked => Time.time > beSuckedProtectionExpireTime;


    protected override void Start()
    {
        beSuckedProtectionExpireTime = Time.time + beSuckedProtectionTime;
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
            // 获得抛射角度和速度
            var half = emitAngle / 2;
            var angle = Random.Range(90 - half, 90 + half);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;

            var slime = Instantiate(subSlimePrefab, transform.position, Quaternion.identity);
            slime.canBeLooted = canBeLooted;
            slime.SetVelocity(dir * emitForce);
        }
    }

    public void PushedTo(Vector2 targetPos, float duration)
    {
        float x1, x2, y1, y2, t, vx, vy, g;
        g = Physics2D.gravity.magnitude * rigidbody.gravityScale;
        x1 = transform.position.x;
        y1 = transform.position.y;
        x2 = targetPos.x;
        y2 = targetPos.y;
        t = duration;

        vx = (x2 - x1) / t;
        vy = (y2 - y1 + 1f / 2 * g * t * t) / t;
        SetVelocity(vx, vy);
    }

    public void PushedToPlayer(float duration)
    {
        PushedTo(PlayerFSM.instance.transform.position, duration);
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