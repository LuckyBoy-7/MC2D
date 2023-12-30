using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;
using UnityEngine.Rendering.Universal;

public class SkeletonFSM : GroundEnemyFSM
{
    [Header("Skeleton")] public float playerDetectionRadius;
    public LayerMask viewLayer;
    public float shootCoolDown;
    public SkeletonArrow arrowPrefab;
    public float shootReachTime;

    protected override void Start()
    {
        base.Start();
        states[StateType.Wait] = new SkeletonWait(this);
        states[StateType.Attack] = new SkeletonAttack(this);
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

        Gizmos.color = Color.green;
        var position = transform.position;
        Gizmos.DrawWireSphere(position, playerDetectionRadius);

        // cliff
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, hitBoxCollider.size);
    }
}

public class SkeletonWait : IState
{
    private SkeletonFSM m;

    public SkeletonWait(SkeletonFSM m)
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

public class SkeletonAttack : IState
{
    private SkeletonFSM m;

    private float elapse;

    public SkeletonAttack(SkeletonFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        AudioManager.instance.Play(m.alertSfxSound);
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

        if (elapse >= m.shootCoolDown)
        {
            elapse -= m.shootCoolDown;
            Shoot();
        }

        elapse += Time.deltaTime;
    }

    private void Shoot()
    {
        var arrow = SkeletonFSM.Instantiate(m.arrowPrefab, m.transform.position, Quaternion.identity);
        var g = arrow.GetComponent<Rigidbody2D>().gravityScale * Physics2D.gravity.magnitude;
        var (x1, y1) = (m.transform.position.x, m.transform.position.y);
        var player = PlayerFSM.instance;
        var (x2, y2) = (player.transform.position.x, player.transform.position.y);
        var vx = (x2 - x1) / m.shootReachTime;
        var vy = (y2 - y1 + 0.5f * g * Mathf.Pow(m.shootReachTime, 2)) / m.shootReachTime;
        arrow.SetTrajectory(new Vector2(vx, vy));
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
    }
}