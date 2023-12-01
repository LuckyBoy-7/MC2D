using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PhantomFSM : FlyEnemyFSM
{
    [Header("Phantom")] public float playerDetectionRadius;
    public LayerMask viewLayer;

    public float moveSpeed;

    protected override void Start()
    {
        base.Start();
        states[StateType.Wait] = new PhantomWait(this); // idle
        states[StateType.Attack] = new PhantomAttack(this);
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

    }
}

public class PhantomWait : IState
{
    private PhantomFSM m;

    public PhantomWait(PhantomFSM m)
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

public class PhantomAttack : IState
{
    private PhantomFSM m;
    private double elapse;

    public PhantomAttack(PhantomFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
    }


    public void OnFixedUpdate()
    {
        m.rigidbody.velocity = (PlayerFSM.instance.transform.position - m.transform.position).normalized * m.moveSpeed;
    }

    public void OnExit()
    {
    }
}