using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class SlimeBossFSM : GroundEnemyFSM
{
    [Header("Debug")] public bool unlockCrush = true;
    public bool unlockDrop = true;
    public bool unlockJump = true;
    public bool unlockThrow = true;
    public bool unlockThrowOne = true;

    [Header("BodySize")] public float minBodySize; // max默认为1，即当前体型
    private Vector3 origBodySize;

    [Header("Movement")] public float moveJumpForce;
    public float moveJumpForceYMultiplier;
    public float moveJumpTimeGap;
    [Header("Prefabs")] public SlimeFSM slimeMiddle; // 一个个吐的时候
    public SlimeFSM slimeSmall; // 吐一堆的时候
    [Header("Global")] public float restTimeAfterEachSkill;

    [Header("Crush")] [Range(0, 1)] public float crushTriggerThreshold = 1; // 触发时的血量百分比
    public BoxCollider2D crushDetectArea; // 只有player在这个box之外才会喷射slime，不然就是坐牢
    public float crushCooldown;
    public float crushCooldownExpireTime;
    public float crushDuration;
    public float crushMoveSpeed;
    [Header("Drop")] [Range(0, 1)] public float dropTriggerThreshold = 0.8f; // 触发时的血量百分比
    public BoxCollider2D dropDetectArea;
    public float dropCooldown;
    public float dropCooldownExpireTime;

    public float dropGravity;
    public float jumpBeforeDropHeight;
    public float dropSpeed;
    public float dropHangTime;

    [Header("Jump")] [Range(0, 1)] public float jumpTriggerThreshold = 0.7f; // 触发时的血量百分比
    public BoxCollider2D jumpDetectArea;
    public float jumpCooldown;
    public float jumpCooldownExpireTime;
    public float jumpDuration;
    public float jumpPosDelta; // 跳到player左(右)jumpPosDelta或本身位置

    [Header("Throw")] [Range(0, 1)] public float throwTriggerThreshold = 0.6f; // 触发时的血量百分比
    public BoxCollider2D throwDetectArea;

    public float throwCooldown;
    public float throwCooldownExpireTime;
    public int throwSlimeNumber;
    public float throwDistGap;
    public float throwDuration;

    [Header("ThrowOne")] // 只有player在throwDetectArea之外才会喷射slime，不然就是坐牢
    public float throwOneCooldown;

    public float throwOneCooldownExpireTime;
    public float throwOneDuration;

    #region Trigger

    private bool IsHealPointInThreshold(float threshold) => healthPoint <= maxHealthPoint * threshold;

    public bool crushTrigger => Time.time >= crushCooldownExpireTime && IsHealPointInThreshold(crushTriggerThreshold) &&
                                !crushDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider)
                                && isOnGround && unlockCrush;

    public bool dropTrigger => Time.time >= dropCooldownExpireTime && IsHealPointInThreshold(dropTriggerThreshold) &&
                               isOnGround &&
                               dropDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockDrop;

    public bool jumpTrigger => Time.time >= jumpCooldownExpireTime && IsHealPointInThreshold(jumpTriggerThreshold) &&
                               isOnGround &&
                               jumpDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockJump;

    public bool throwTrigger => Time.time >= throwCooldownExpireTime && IsHealPointInThreshold(throwTriggerThreshold) &&
                                isOnGround &&
                                throwDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockThrow;

    public bool throwOneTrigger => Time.time >= throwOneCooldownExpireTime && isOnGround &&
                                   throwDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockThrowOne;

    #endregion


    protected override void Start()
    {
        origBodySize = transform.localScale;

        base.Start();
        states[StateType.Move] = new SlimeBossMove(this);
        states[StateType.Crush] = new SlimeBossCrush(this);
        states[StateType.Drop] = new SlimeBossDrop(this);
        states[StateType.ThrowOne] = new SlimeBossThrowOne(this);
        states[StateType.Throw] = new SlimeBossThrow(this);
        states[StateType.Jump] = new SlimeBossJump(this);
        TransitionState(StateType.Move);
    }

    protected override void Update()
    {
        base.Update();
        var scale = origBodySize * (((float)healthPoint / maxHealthPoint) * (1 - minBodySize) + minBodySize);
        transform.localScale = scale;
    }

    public override void Reset()
    {
        base.Reset();
        transform.localScale = origBodySize;
    }
}

public class SlimeBossMove : IState
{
    private SlimeBossFSM m;
    private double restElapse;
    private float moveJumpElapse;

    public SlimeBossMove(SlimeBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        restElapse += Time.deltaTime;
        if (restElapse <= m.restTimeAfterEachSkill)
            return;
        List<StateType> availableSkills = new();
        if (m.crushTrigger)
            availableSkills.Add(StateType.Crush);
        if (m.dropTrigger)
            availableSkills.Add(StateType.Drop);
        if (m.jumpTrigger)
            availableSkills.Add(StateType.Jump);
        if (m.throwOneTrigger)
            availableSkills.Add(StateType.ThrowOne);
        if (m.throwTrigger)
            availableSkills.Add(StateType.Throw);

        if (availableSkills.Count > 0)
            m.TransitionState(availableSkills[Random.Range(0, availableSkills.Count)]);
        else if (m.isOnGround)
        {
            // movement
            moveJumpElapse += Time.deltaTime;
            if (moveJumpElapse <= m.moveJumpTimeGap)
                return;
            moveJumpElapse -= m.moveJumpTimeGap;

            m.LookTowardsPlayer();
            var dir = new Vector2(m.facingDirection * 1, m.moveJumpForceYMultiplier);
            m.SetVelocity(dir * m.moveJumpForce);
        }
    }

    public void OnFixedUpdate()
    {
        if (m.isOnGround)
            m.LerpVelocity(Vector2.zero);
    }

    public void OnExit()
    {
        restElapse = 0;
        moveJumpElapse = 0;
    }
}

public class SlimeBossCrush : IState
{
    private SlimeBossFSM m;
    private float elapse;

    public SlimeBossCrush(SlimeBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.LookTowardsPlayer();
        m.SetVelocityX(m.facingDirection * m.crushMoveSpeed);
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse > m.crushDuration || m.isHittingWall)
            m.TransitionState(StateType.Move);
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.crushCooldownExpireTime = Time.time + m.crushCooldown;
        elapse = 0;
    }
}

public class SlimeBossDrop : IState
{
    private SlimeBossFSM m;
    private float elapse;
    private bool isStart;

    public SlimeBossDrop(SlimeBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter() // 我们希望跳到player头上时刚好在最好点
    {
        m.SetGravity(m.dropGravity);
        float x1, x2, y1, y2, t, vx, vy, g;
        g = Physics2D.gravity.magnitude * m.rigidbody.gravityScale;
        x1 = m.transform.position.x;
        y1 = m.transform.position.y;
        x2 = PlayerFSM.instance.transform.position.x;
        y2 = PlayerFSM.instance.transform.position.y + m.jumpBeforeDropHeight;
        t = Mathf.Sqrt((y2 - y1) * 2 / g);

        vx = (x2 - x1) / t;
        vy = (y2 - y1 + 1f / 2 * g * t * t) / t;
        m.SetVelocity(vx, vy);
    }

    public void OnUpdate()
    {
        if (m.isOnGround && m.isMovingDown)
        {
            m.TransitionState(StateType.Move);
            return;
        }

        if (m.isMovingDown && !isStart)
            m.StartCoroutine(HangAndDrop());
    }

    private IEnumerator HangAndDrop()
    {
        isStart = true;
        m.SetAfloat();
        m.SetVelocity(0, 0);
        yield return new WaitForSeconds(m.dropHangTime);
        m.ResetGravity();
        m.SetVelocityY(-m.dropSpeed);
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.dropCooldownExpireTime = Time.time + m.dropCooldown;
        elapse = 0;
        isStart = false;
        m.ResetGravity();
    }
}

public class SlimeBossThrow : IState
{
    private SlimeBossFSM m;

    public SlimeBossThrow(SlimeBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        float x0 = PlayerFSM.instance.transform.position.x - m.throwDistGap * (m.throwSlimeNumber - 1) / 2;
        float y = PlayerFSM.instance.transform.position.y;
        for (int i = 0; i < m.throwSlimeNumber; i++)
        {
            float x = x0 + m.throwDistGap * i;
            var slime = Object.Instantiate(m.slimeSmall, m.transform.position, Quaternion.identity,
                m.transform.parent);
            slime.canBeLooted = false;
            slime.PushedTo(new Vector2(x, y), m.throwDuration);
        }

        m.TransitionState(StateType.Move);
    }


    public void OnUpdate()
    {
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.throwCooldownExpireTime = Time.time + m.throwCooldown;
    }
}

public class SlimeBossThrowOne : IState
{
    private SlimeBossFSM m;

    public SlimeBossThrowOne(SlimeBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        var slime = Object.Instantiate(m.slimeMiddle, m.transform.position, Quaternion.identity, m.transform.parent);
        slime.canBeLooted = false;
        slime.PushedToPlayer(m.throwOneDuration);
        m.TransitionState(StateType.Move);
    }

    public void OnUpdate()
    {
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.throwOneCooldownExpireTime = Time.time + m.throwOneCooldown;
    }
}

public class SlimeBossJump : IState
{
    private SlimeBossFSM m;

    public SlimeBossJump(SlimeBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        PlayerFSM player = PlayerFSM.instance;
        List<float> delta = new() { -m.jumpPosDelta, 0, m.jumpPosDelta };
        float x1, x2, y1, y2, t, vx, vy, g;
        g = Physics2D.gravity.magnitude * m.rigidbody.gravityScale;
        x1 = m.transform.position.x;
        y1 = m.transform.position.y;
        x2 = PlayerFSM.instance.transform.position.x + delta[Random.Range(0, delta.Count)];
        y2 = PlayerFSM.instance.transform.position.y;
        t = m.jumpDuration;

        vx = (x2 - x1) / t;
        vy = (y2 - y1 + 1f / 2 * g * t * t) / t;
        m.SetVelocity(vx, vy);
    }


    public void OnUpdate()
    {
        if (m.isOnGround && m.isMovingDown)
            m.TransitionState(StateType.Move);
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.jumpCooldownExpireTime = Time.time + m.jumpCooldown;
    }
}