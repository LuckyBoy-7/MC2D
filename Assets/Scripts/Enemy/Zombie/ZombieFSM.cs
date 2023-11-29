using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class ZombieFSM : EnemyFSM
{
    [Header("Patrol")] public float patrolMoveSpeed;
    public float patrolRestTime;

    [Header("Alert")] public LayerMask viewLayer;
    public float frontViewLength;
    public float behindViewLength;
    public float alertTime;
    [Header("Chase")] public float chaseSpeed;
    public float xDeltaBehindPlayer;
    [Header("Question")] public float questionTime;
    

    void Start()
    {
        // p = (ZombieParameter)base.p;

        healthPoint = maxHealthPoint;
        // Patrol, Alert, Chase, Question
        states[StateType.Patrol] = new ZombiePatrol(this);
        states[StateType.Alert] = new ZombieAlert(this);
        states[StateType.Chase] = new ZombieChase(this);
        states[StateType.Question] = new ZombieQuestion(this);
        TransitionState(StateType.Patrol);
    }

    private void Update()
    {
        currentState.OnUpdate();
        // Debug.Log($"currentState: {currentState}");
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }


    public bool isPlayerInView => isPlayerInFrontView || isPlayerInBehindView;

    public bool isPlayerInFrontView
    {
        get
        {
            var frontStart = transform.position + Vector3.right * 0.5f;
            var rightRayCastHit = Physics2D.BoxCast(frontStart, new Vector2(0.01f, 0.95f), 0,
                Vector2.right * facingDirection, frontViewLength, viewLayer);
            return rightRayCastHit.collider != null && rightRayCastHit.collider.CompareTag("Player");
        }
    }

    public bool isPlayerInBehindView
    {
        get
        {
            var behindStart = transform.position - Vector3.right * 0.5f;
            var leftRayCastHit = Physics2D.BoxCast(behindStart, new Vector2(0.01f, 0.95f), 0,
                Vector2.right * -facingDirection, behindViewLength, viewLayer);
            return leftRayCastHit.collider != null && leftRayCastHit.collider.CompareTag("Player");
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Cliff Raycast check
        var position = transform.position;
        Vector3 bottomCenter = position + Vector3.down * 0.5f;
        Vector3 bottomLeft = bottomCenter - Vector3.right * 0.5f;
        Vector3 bottomRight = bottomCenter + Vector3.right * 0.5f;

        Gizmos.DrawLine(bottomLeft, bottomLeft + Vector3.down * cliffCheckDownRaycastDist);
        Gizmos.DrawLine(bottomRight, bottomRight + Vector3.down * cliffCheckDownRaycastDist);
        // Wall Box
        Gizmos.DrawWireCube(position + Vector3.right * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 0.95f));
        Gizmos.DrawWireCube(position + Vector3.left * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 0.95f));
        // Hit Box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(position, hitBoxCollider.bounds.size);
        // Alert
        var frontCenter = position + facingDirection * (Vector3.right * (0.5f + frontViewLength / 2));
        Gizmos.DrawWireCube(frontCenter, new Vector3(frontViewLength, 1));
        var behindCenter = position - facingDirection * (Vector3.right * (0.5f + behindViewLength / 2));
        Gizmos.DrawWireCube(behindCenter, new Vector3(behindViewLength, 1));
    }
}

public class ZombiePatrol : IState
{
    private ZombieFSM m;
    private float elapse;
    private bool isRest;

    public ZombiePatrol(ZombieFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        if (m.isPlayerInView)
        {
            m.TransitionState(StateType.Alert);
            return;
        }

        if (isRest)
        {
            elapse += Time.deltaTime;
            if (elapse >= m.patrolRestTime)
            {
                m.ReverseFacingDirection();
                elapse = 0;
                isRest = false;
            }

            return;
        }

        if (m.isWalkingDownCliff || m.isHittingWall)
        {
            m.rigidbody.velocity = Vector2.zero;
            isRest = true;
        }
    }

    public void OnFixedUpdate()
    {
        if (isRest)
            return;
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.facingDirection * m.patrolMoveSpeed,
            m.xVelocityChangeSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        elapse = 0;
    }
}

public class ZombieAlert : IState
{
    private ZombieFSM m;
    private float elapse;

    public ZombieAlert(ZombieFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = new Vector2(0, m.rigidbody.velocity.y);
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse > m.alertTime)
            m.TransitionState(StateType.Chase);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, 0,
            m.xVelocityChangeSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        elapse = 0;
    }
}

public class ZombieChase : IState
{
    private ZombieFSM m;
    private float targetPosX;

    public ZombieChase(ZombieFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        RollTargetPosAndResetOrient();
    }

    public void OnUpdate()
    {
        if (m.isHittingWall)
            m.TransitionState(StateType.Question);
        if (m.isWalkingDownCliff && !m.isOverGroundAboveCliff) // 如果下面有空地，zombie会追下去
            m.TransitionState(StateType.Question);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.facingDirection * m.chaseSpeed,
            m.xVelocityChangeSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
        if (m.rigidbody.position.x > targetPosX && m.facingDirection == 1 ||
            m.rigidbody.position.x < targetPosX && m.facingDirection == -1) // 到目的地了
        {
            if (m.isPlayerInView)
            {
                RollTargetPosAndResetOrient();
            }
            else
                m.TransitionState(StateType.Question);
        }
    }

    public void OnExit()
    {
    }

    private void RollTargetPosAndResetOrient()
    {
        m.facingDirection = PlayerFSM.instance.transform.position.x - m.transform.position.x < 0 ? -1 : 1;
        targetPosX = PlayerFSM.instance.transform.position.x + m.facingDirection * m.xDeltaBehindPlayer;
    }
}

public class ZombieQuestion : IState
{
    private ZombieFSM m;
    public float elapse;

    public ZombieQuestion(ZombieFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = new Vector2(0, m.rigidbody.velocity.y);
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse <= m.questionTime)
            return;
        m.TransitionState(StateType.Patrol);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, 0,
            m.xVelocityChangeSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        elapse = 0;
    }
}