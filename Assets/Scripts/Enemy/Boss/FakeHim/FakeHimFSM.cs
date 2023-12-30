using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class FakeHimFSM : GroundEnemyFSM
{
    [Header("Debug")] public bool unlockGroundCrush = true;
    public bool unlockAirCrush = true;
    public bool unlockThrow = true; // 丢出黑曜石boomerang
    public bool unlockLash = true;
    public bool unlockStepBack = true;

    [Header("Appearance")] public SpriteRenderer spriteRenderer;
    public Sprite fragileSprite;
    [Header("Movement")] public float moveSpeed;

    [Header("Prefabs")] public Transform boomerangPrefab;
    [Header("Global")] public float restTimeAfterEachSkill;

    [Header("GroundCrush")] [Range(0, 1)] public float groundCrushTriggerThreshold = 1; // 触发时的血量百分比
    public BoxCollider2D groundCrushDetectArea;
    public float groundCrushCooldown;
    public float groundCrushCooldownExpireTime;

    public float groundCrushJumpBackDist;
    public float groundCrushJumpBackDuration;
    public float groundCrushChargeForceTime;
    public float groundCrushAheadDist;
    public float groundCrushAheadDuration;
    public AudioClip crushSfxSound;

    [Header("AirCrush")] [Range(0, 1)] public float airCrushTriggerThreshold = 1; // 触发时的血量百分比
    public float airCrushCooldown;
    public float airCrushCooldownExpireTime;

    public Vector2 airJumpDistVector; // 一般是一个较高的高度和一点向前的微小位移
    public float airJumpForce;
    public float airJumpDuration;
    public float airCrushStateOutDelay = 0.5f; // 冲到地面或墙壁上delay x秒后切换回move
    public float airCrushSpeed;

    [Header("Throw")] [Range(0, 1)] public float throwTriggerThreshold = 0.6f; // 触发时的血量百分比
    public BoxCollider2D throwDetectArea;
    public float throwCooldown;
    public float throwCooldownExpireTime;

    public float throwWindUpTime = 0.5f;
    public float throwDist; // 我的想法是丢出去，转几个圈，再拉回来
    public float throwDuration;
    public float boomerangRotateNumber;
    public float boomerangRotateDuration;
    public AudioClip throwSfxSound;

    [Header("Lash")] [Range(0, 1)] public float lashTriggerThreshold = 0.6f; // 舞丝
    public float lashCooldown;
    public float lashCooldownExpireTime;

    public Animator lashAnimator;
    public float lashJumpMinHeightAbovePlayer;
    public float lashJumpMaxHeightAbovePlayer;
    public float lashJumpForce;
    public float lashJumpDuration;
    public AudioClip lashSfxSound;

    [Header("StepBack")] [Range(0, 1)] public float stepBackTriggerThreshold = 0.6f; // 触发时的血量百分比
    public BoxCollider2D stepBackDetectArea;

    public float stepBackCooldown;
    public float stepBackCooldownExpireTime;
    public Vector2 stepBackForce;
    public float stepBackForceMultiplier;


    #region Trigger

    private bool IsHealPointInThreshold(float threshold) => healthPoint <= maxHealthPoint * threshold;

    public bool groundCrushTrigger => Time.time >= groundCrushCooldownExpireTime &&
                                      groundCrushDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) &&
                                      IsHealPointInThreshold(groundCrushTriggerThreshold)
                                      && isOnGround && unlockGroundCrush;

    public bool airCrushTrigger =>
        Time.time >= airCrushCooldownExpireTime && IsHealPointInThreshold(airCrushTriggerThreshold)
                                                && isOnGround && unlockAirCrush;

    public bool throwTrigger => Time.time >= throwCooldownExpireTime && IsHealPointInThreshold(throwTriggerThreshold) &&
                                isOnGround &&
                                throwDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockThrow;

    public bool lashTrigger => Time.time >= lashCooldownExpireTime && IsHealPointInThreshold(lashTriggerThreshold)
                                                                   && isOnGround && unlockLash;

    public bool stepBackTrigger =>
        Time.time >= stepBackCooldownExpireTime && IsHealPointInThreshold(stepBackTriggerThreshold) &&
        stepBackDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && isOnGround && unlockStepBack;

    #endregion


    protected override void Start()
    {
        base.Start();
        states[StateType.Move] = new FakeHimMove(this); // 跟player保持距离
        states[StateType.GroundCrush] = new FakeHimGroundCrush(this);
        states[StateType.AirCrush] = new FakeHimAirCrush(this);
        states[StateType.Throw] = new FakeHimThrow(this);
        states[StateType.Lash] = new FakeHimLash(this);
        states[StateType.StepBack] = new FakeHimStepBack(this);
        TransitionState(StateType.Move); // stepBack写在fsm里面
    }

    protected override void Update()
    {
        base.Update();
        if (healthPoint <= maxHealthPoint / 2)
            spriteRenderer.sprite = fragileSprite;
    }
    // private void LateUpdate()
    // {
    //     Debug.Log(currentState);
    // }
}

public class FakeHimMove : IState
{
    private FakeHimFSM m;
    private double restElapse;
    private float moveJumpElapse;

    public FakeHimMove(FakeHimFSM m)
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
        if (m.groundCrushTrigger)
            availableSkills.Add(StateType.GroundCrush);
        if (m.airCrushTrigger)
            availableSkills.Add(StateType.AirCrush);
        if (m.throwTrigger)
            availableSkills.Add(StateType.Throw);
        if (m.lashTrigger)
            availableSkills.Add(StateType.Lash);
        if (m.stepBackTrigger)
            availableSkills.Add(StateType.StepBack);

        if (availableSkills.Count > 0)
            m.TransitionState(availableSkills[Random.Range(0, availableSkills.Count)]);
    }

    public void OnFixedUpdate()
    {
        m.LookBackwardsPlayer();
        if (m.isOnGround)
        {
            m.LerpVelocityX(m.facingDirection * m.moveSpeed);
        }
    }

    public void OnExit()
    {
        restElapse = 0;
        moveJumpElapse = 0;
    }
}

public class FakeHimGroundCrush : IState
{
    private FakeHimFSM m;

    public FakeHimGroundCrush(FakeHimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.LookTowardsPlayer();
        m.SetVelocityX(0);
        var s = DOTween.Sequence();
        s.Append(m.rigidbody.DOMoveX(m.rigidbody.position.x - m.facingDirection * m.groundCrushJumpBackDist,
            m.groundCrushJumpBackDuration));
        s.AppendInterval(m.groundCrushChargeForceTime);
        s.AppendCallback(() => AudioManager.instance.Play(m.crushSfxSound));
        s.Append(m.rigidbody.DOMoveX(m.rigidbody.position.x + m.facingDirection * m.groundCrushAheadDist,
            m.groundCrushAheadDuration));
        s.AppendCallback(() => m.TransitionState(StateType.Move));
    }

    public void OnUpdate()
    {
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.groundCrushCooldownExpireTime = Time.time + m.groundCrushCooldown;
    }
}

public class FakeHimAirCrush : IState
{
    private FakeHimFSM m;
    private float elapse;

    public FakeHimAirCrush(FakeHimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        // 跳跃->蓄力
        m.LookTowardsPlayer();
        m.SetGravity(0);
        m.canBeKnockedBack = false;
        var jumpTargetPos = m.transform.position +
                            new Vector3(m.facingDirection * m.airJumpDistVector.x, m.airJumpDistVector.y);
        m.transform.DOJump(jumpTargetPos, m.airJumpForce, 1, m.airJumpDuration).onComplete +=
            () =>
            {
                // 冲向player
                PlayerFSM player = PlayerFSM.instance;
                var dir = (player.transform.position - m.transform.position).normalized;
                m.SetVelocity(dir * m.airCrushSpeed);
                AudioManager.instance.Play(m.crushSfxSound);
            };
    }

    public void OnUpdate()
    {
        if (m.isHittingPlatform)
        {
            elapse += Time.deltaTime;
            if (elapse > m.airCrushStateOutDelay)
                m.TransitionState(StateType.Move);
        }
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.canBeKnockedBack = true;
        m.airCrushCooldownExpireTime = Time.time + m.airCrushCooldown;
        m.ResetGravity();
        elapse = 0;
    }
}

public class FakeHimThrow : IState
{
    private FakeHimFSM m;

    public FakeHimThrow(FakeHimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.SetVelocity(Vector2.zero);
        m.canBeKnockedBack = false;
        m.LookTowardsPlayer();
        var spawnPos = m.transform.position + m.facingDirection * Vector3.right;
        var targetPos = m.transform.position + m.facingDirection * m.throwDist * Vector3.right;
        var boomerang = Object.Instantiate(m.boomerangPrefab, spawnPos, Quaternion.identity);
        var s = DOTween.Sequence();
        s.AppendInterval(m.throwWindUpTime);
        s.AppendCallback(() => AudioManager.instance.Play(m.throwSfxSound));
        s.Append(boomerang.DOMove(targetPos, m.throwDuration));
        s.Append(boomerang.DORotate(new(0, 0, m.boomerangRotateNumber * 360), m.boomerangRotateDuration,
            RotateMode.FastBeyond360));
        s.Append(boomerang.DOMove(m.transform.position, m.throwDuration));
        s.AppendCallback(() =>
        {
            Object.Destroy(boomerang.gameObject);
            m.canBeKnockedBack = true;
            m.TransitionState(StateType.Move);
        });
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

public class FakeHimLash : IState
{
    private FakeHimFSM m;
    private bool isJumpOver;
    private AudioSource lashAudioSource;

    private bool canOperate;

    public FakeHimLash(FakeHimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        // 跳跃->蓄力
        m.LookTowardsPlayer();
        m.SetVelocity(Vector2.zero);
        m.SetGravity(0);
        m.canBeKnockedBack = false;
        float height = Random.Range(m.lashJumpMinHeightAbovePlayer, m.lashJumpMaxHeightAbovePlayer);
        var jumpTargetPos = PlayerFSM.instance.transform.position + Vector3.up * height;
        m.transform.DOJump(jumpTargetPos, m.lashJumpForce, 1, m.lashJumpDuration).onComplete +=
            () =>
            {
                isJumpOver = true;
                m.lashAnimator.Play("Lash");
                lashAudioSource = AudioManager.instance.GetAudioSource(m.lashSfxSound);
                m.StartCoroutine(WaitForAFrame());
            };
    }

    private IEnumerator WaitForAFrame()
    {
        canOperate = false;
        yield return new WaitForEndOfFrame();
        canOperate = true;
    }


    public void OnUpdate()
    {
        if (!isJumpOver || !canOperate)
            return;
        var state = m.lashAnimator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName("Lash")) // 说明动画播放完毕
            m.TransitionState(StateType.Move);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.lashCooldownExpireTime = Time.time + m.lashCooldown;
        m.ResetGravity();
        isJumpOver = false;
        m.canBeKnockedBack = true;
        if (lashAudioSource)
            Object.Destroy(lashAudioSource);
    }
}

public class FakeHimStepBack : IState
{
    private FakeHimFSM m;

    public FakeHimStepBack(FakeHimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        // 跳跃->蓄力
        m.LookTowardsPlayer(); // 应该为后撤步，而不是向后跑
        m.SetVelocity(Vector2.zero);
        m.Pushed(new Vector2(m.stepBackForce.x * -m.facingDirection, m.stepBackForce.y) * m.stepBackForceMultiplier);
    }


    public void OnUpdate()
    {
        if (m.isOnGround)
            m.TransitionState(StateType.Move);
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.stepBackCooldownExpireTime = Time.time + m.stepBackCooldown;
    }
}