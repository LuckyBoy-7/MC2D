using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

[Serializable]
public class ZombieParameter
{
    [Header("Movement")] public float patrolMoveSpeed;
    public float patrolRestTime;
    public Rigidbody2D rigidbody;
    public int facingDirection; // x方向
    [Header("PhysicsCheck")] public float cliffCheckDownRaycastDist;
    public float boxLeftRightCastDist;
    public LayerMask groundLayer;
    public BoxCollider2D hitBoxCollider;
    [Header("Alert")] public LayerMask viewLayer;
    public float frontViewLength;
    public float behindViewLength;
    public GameObject alertIcon;
    public float alertTime;
    [Header("Chase")] public float chaseSpeed;
    public float xDeltaBehindPlayer;
    [Header("Question")] public GameObject questionIcon;
    public float questionTime;
}

public class ZombieFSM : FSM
{
    public ZombieParameter p;

    void Start()
    {
        // Patrol, Alert, Chase, Question
        states[StateType.Patrol] = new ZombiePatrol(p, this);
        states[StateType.Alert] = new ZombieAlert(p, this);
        states[StateType.Chase] = new ZombieChase(p, this);
        states[StateType.Question] = new ZombieQuestion(p, this);
        TransitionState(StateType.Patrol);
    }

    private void Update()
    {
        currentState.OnUpdate();
        // Debug.Log($"currentState: {currentState}");
    }

    private bool isOverLeftCliff
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomLeft = bottomCenter - Vector3.right * 0.5f;


            bool overLeftCliff = !Physics2D.Linecast(bottomLeft,
                bottomLeft + Vector3.down * p.cliffCheckDownRaycastDist,
                p.groundLayer);
            return overLeftCliff;
        }
    }

    private bool isOverRightCliff
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomRight = bottomCenter + Vector3.right * 0.5f;


            bool overRightCliff = !Physics2D.Linecast(bottomRight,
                bottomRight + Vector3.down * p.cliffCheckDownRaycastDist,
                p.groundLayer);
            return overRightCliff;
        }
    }

    public bool isWalkingDownCliff =>
        isOverLeftCliff && p.facingDirection == -1 || isOverRightCliff && p.facingDirection == 1;

    private bool isOverLeftGround // 就是到悬崖边上后检测下面是否是空地
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomLeft = bottomCenter - Vector3.right * 0.5f;


            bool overLeftGround = Physics2D.Raycast(bottomLeft, Vector2.down, 6, p.groundLayer);
            return overLeftGround;
        }
    }

    private bool isOverRightGround
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomRight = bottomCenter + Vector3.right * 0.5f;


            bool overRightGround = Physics2D.Raycast(bottomRight, Vector2.down, 6, p.groundLayer);
            return overRightGround;
        }
    }

    public bool isOverGroundAboveCliff => isOverLeftCliff && isOverLeftGround || isOverRightCliff && isOverRightGround;

    private bool isOnLeftWall =>
        Physics2D.OverlapBox(transform.position + Vector3.left * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 0.95f), 0, p.groundLayer);

    private bool isOnRightWall =>
        Physics2D.OverlapBox(transform.position + Vector3.right * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 0.95f), 0, p.groundLayer);

    public bool isHittingWall => isOnLeftWall && p.facingDirection == -1 || isOnRightWall && p.facingDirection == 1;

    public bool isPlayerInView => isPlayerInFrontView || isPlayerInBehindView;

    public bool isPlayerInFrontView
    {
        get
        {
            var frontStart = transform.position + Vector3.right * 0.5f;
            var rightRayCastHit = Physics2D.BoxCast(frontStart, new Vector2(0.01f, 0.95f), 0,
                Vector2.right * p.facingDirection, p.frontViewLength, p.viewLayer);
            return rightRayCastHit.collider != null && rightRayCastHit.collider.CompareTag("Player");
        }
    }

    public bool isPlayerInBehindView
    {
        get
        {
            var behindStart = transform.position - Vector3.right * 0.5f;
            var leftRayCastHit = Physics2D.BoxCast(behindStart, new Vector2(0.01f, 0.95f), 0,
                Vector2.right * -p.facingDirection, p.behindViewLength, p.viewLayer);
            return leftRayCastHit.collider != null && leftRayCastHit.collider.CompareTag("Player");
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Cliff Raycast check
        Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
        Vector3 bottomLeft = bottomCenter - Vector3.right * 0.5f;
        Vector3 bottomRight = bottomCenter + Vector3.right * 0.5f;

        Gizmos.DrawLine(bottomLeft, bottomLeft + Vector3.down * p.cliffCheckDownRaycastDist);
        Gizmos.DrawLine(bottomRight, bottomRight + Vector3.down * p.cliffCheckDownRaycastDist);
        // Wall Box
        Gizmos.DrawWireCube(transform.position + Vector3.right * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 0.95f));
        Gizmos.DrawWireCube(transform.position + Vector3.left * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 0.95f));
        // Hit Box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, p.hitBoxCollider.bounds.size);
        // Alert
        var frontCenter = transform.position + p.facingDirection * (Vector3.right * (0.5f + p.frontViewLength / 2));
        Gizmos.DrawWireCube(frontCenter, new Vector3(p.frontViewLength, 1));
        var behindCenter = transform.position - p.facingDirection * (Vector3.right * (0.5f + p.behindViewLength / 2));
        Gizmos.DrawWireCube(behindCenter, new Vector3(p.behindViewLength, 1));
    }


    public void ReverseFacingDirection() => p.facingDirection *= -1;
}

public class ZombiePatrol : IState
{
    private ZombieParameter p;
    private ZombieFSM manager;
    private float elapse;
    private bool isRest;

    public ZombiePatrol(ZombieParameter p, ZombieFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        if (manager.isPlayerInView)
        {
            manager.TransitionState(StateType.Alert);
            return;
        }

        if (isRest)
        {
            elapse += Time.deltaTime;
            if (elapse >= p.patrolRestTime)
            {
                manager.ReverseFacingDirection();
                elapse = 0;
                isRest = false;
            }

            return;
        }

        p.rigidbody.velocity = new Vector2(p.facingDirection * p.patrolMoveSpeed, p.rigidbody.velocity.y);
        if (manager.isWalkingDownCliff || manager.isHittingWall)
        {
            p.rigidbody.velocity = Vector2.zero;
            isRest = true;
        }
    }

    public void OnExit()
    {
        elapse = 0;
    }
}

public class ZombieAlert : IState
{
    private ZombieParameter p;
    private ZombieFSM manager;
    private float elapse;

    public ZombieAlert(ZombieParameter p, ZombieFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        p.alertIcon.SetActive(true);
        p.rigidbody.velocity = new Vector2(0, p.rigidbody.velocity.y);
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse > p.alertTime)
            manager.TransitionState(StateType.Chase);
    }

    public void OnExit()
    {
        elapse = 0;
        p.alertIcon.SetActive(false);
    }
}

public class ZombieChase : IState
{
    private ZombieParameter p;
    private ZombieFSM manager;
    private float targetPosX;

    public ZombieChase(ZombieParameter p, ZombieFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        RollTargetPosAndResetOrient();
    }

    public void OnUpdate()
    {
        var position = p.rigidbody.position;
        p.rigidbody.position =
            Vector3.MoveTowards(position, new Vector3(targetPosX, position.y), p.chaseSpeed * Time.deltaTime);

        if (Mathf.Abs(p.rigidbody.position.x - targetPosX) <= 1e-5)
        {
            if (manager.isPlayerInView)
            {
                RollTargetPosAndResetOrient();
            }
            else
                manager.TransitionState(StateType.Question);

            return;
        }

        if (manager.isHittingWall)
            manager.TransitionState(StateType.Question);
        if (manager.isWalkingDownCliff && !manager.isOverGroundAboveCliff)  // 如果下面有空地，zombie会追下去
            manager.TransitionState(StateType.Question);
    }

    public void OnExit()
    {
    }

    private void RollTargetPosAndResetOrient()
    {
        p.facingDirection = PlayerFSM.instance.transform.position.x - manager.transform.position.x < 0 ? -1 : 1;
        targetPosX = PlayerFSM.instance.transform.position.x + p.facingDirection * p.xDeltaBehindPlayer;
    }
}

public class ZombieQuestion : IState
{
    private ZombieParameter p;
    private ZombieFSM manager;
    public float elapse;

    public ZombieQuestion(ZombieParameter p, ZombieFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        p.rigidbody.velocity = new Vector2(0, p.rigidbody.velocity.y);
        p.questionIcon.SetActive(true);
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse <= p.questionTime)
            return;
        manager.TransitionState(StateType.Patrol);
    }

    public void OnExit()
    {
        p.questionIcon.SetActive(false);
        elapse = 0;
    }
}