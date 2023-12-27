using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class HimFSM : GroundEnemyFSM
{
    [Header("Debug")] public bool unlockGroundCrush = true;
    public bool unlockAirCrush = true;
    public bool unlockStepBack = true;

    public bool unlockGroundSpike = true; // 丢出黑曜石boomerang
    public bool unlockShootSpike = true; // 丢出黑曜石boomerang
    public bool unlockBurstSpike = true; // 丢出黑曜石boomerang
    public bool unlockSingExplode = true; // 丢出黑曜石boomerang

    [Header("Movement")] public float moveSpeed;

    [Header("Prefabs")] public HimSpike spikePrefab;
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

    [Header("AirCrush")] [Range(0, 1)] public float airCrushTriggerThreshold = 1; // 触发时的血量百分比
    public float airCrushCooldown;
    public float airCrushCooldownExpireTime;

    public Vector2 airJumpDistVector; // 一般是一个较高的高度和一点向前的微小位移
    public float airJumpForce;
    public float airJumpDuration;
    public float airCrushStateOutDelay = 0.5f; // 冲到地面或墙壁上delay x秒后切换回move
    public float airCrushSpeed;

    [Header("StepBack")] [Range(0, 1)] public float stepBackTriggerThreshold = 0.6f; // 触发时的血量百分比
    public BoxCollider2D stepBackDetectArea;

    public float stepBackCooldown;
    public float stepBackCooldownExpireTime;
    public Vector2 stepBackForce;
    public float stepBackForceMultiplier;

    [Header("Drop && GroundSpike")] [Range(0, 1)]
    public float groundSpikeTriggerThreshold = 0.6f; // 触发时的血量百分比

    public float groundSpikeCooldown;
    public float groundSpikeCooldownExpireTime;

    public float dropGravity;
    public float jumpBeforeDropHeight;
    public float jumpBeforeDropRemainTime;
    public float dropSpeed;

    public float groundSpikeInitialLength = 0.5f;
    public float groundSpikeGap;
    public float groundSpikeWindUpTime; // 给一个停顿，即反应时间
    public float groundSpikeStrikeLength;
    public float groundSpikeStrikeDuration;
    public float groundSpikeRemainTimeAfterStrike;

    [Header("ShootSpike")] [Range(0, 1)] public float shootSpikeTriggerThreshold = 0.6f;
    public BoxCollider2D shootSpikeDetectArea;
    public float shootSpikeCooldown;
    public float shootSpikeCooldownExpireTime;

    public float shootSpikeLength;
    public float shootSpikeStartAngle;
    public float shootSpikeDeltaAngle;
    public float shootSpikeNumber;
    public float shootSpikeSpeed;
    public float shootSpikeTimeGap;

    [Header("BurstSpike")] [Range(0, 1)] public float burstSpikeTriggerThreshold = 0.6f;
    public BoxCollider2D burstSpikeDetectArea;
    public float burstSpikeCooldown;
    public float burstSpikeCooldownExpireTime;

    public float burstSpikeLength = 4f;
    public float burstSpikeWindupTime = 0.5f;
    public int burstSpikeNumber = 8;
    public float burstSpikeSpeed;
    public int burstSpikeWave = 3;
    public float burstSpikeWaveTimeGap = 1;


    [Header("SingExplode")] [Range(0, 1)] public float singExplodeTriggerThreshold = 0.6f;
    public BoxCollider2D singExplodeDetectArea;
    public float singExplodeCooldown;
    public float singExplodeCooldownExpireTime;

    public HimCircle[] singCircle;
    public float[] singRadius;
    public float singExpandDuration;
    public float singRemainDuration;
    public Color turnToColor;
    public float singShrinkDuration;

    #region Trigger

    private bool IsHealPointInThreshold(float threshold) => healthPoint <= maxHealthPoint * threshold;

    public bool groundCrushTrigger => Time.time >= groundCrushCooldownExpireTime &&
                                      groundCrushDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) &&
                                      IsHealPointInThreshold(groundCrushTriggerThreshold)
                                      && isOnGround && unlockGroundCrush;

    public bool airCrushTrigger =>
        Time.time >= airCrushCooldownExpireTime && IsHealPointInThreshold(airCrushTriggerThreshold)
                                                && isOnGround && unlockAirCrush;


    public bool stepBackTrigger =>
        Time.time >= stepBackCooldownExpireTime && IsHealPointInThreshold(stepBackTriggerThreshold) &&
        stepBackDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && isOnGround && unlockStepBack;


    public bool groundSpikeTrigger => Time.time >= groundSpikeCooldownExpireTime &&
                                      IsHealPointInThreshold(groundSpikeTriggerThreshold) &&
                                      isOnGround && unlockGroundSpike;

    public bool shootSpikeTrigger => Time.time >= shootSpikeCooldownExpireTime &&
                                     shootSpikeDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) &&
                                     IsHealPointInThreshold(shootSpikeTriggerThreshold) &&
                                     isOnGround && unlockShootSpike;

    public bool burstSpikeTrigger => Time.time >= burstSpikeCooldownExpireTime &&
                                     burstSpikeDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) &&
                                     IsHealPointInThreshold(burstSpikeTriggerThreshold) &&
                                     isOnGround && unlockBurstSpike;

    public bool singExplodeTrigger => Time.time >= singExplodeCooldownExpireTime &&
                                      singExplodeDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) &&
                                      IsHealPointInThreshold(singExplodeTriggerThreshold) &&
                                      isOnGround && unlockSingExplode;

    #endregion

    protected override void Start()
    {
        base.Start();
        states[StateType.Move] = new HimMove(this); // 跟player保持距离
        states[StateType.GroundCrush] = new HimGroundCrush(this);
        states[StateType.AirCrush] = new HimAirCrush(this);
        states[StateType.StepBack] = new HimStepBack(this);
        states[StateType.GroundSpike] = new HimGroundSpike(this);
        states[StateType.ShootSpike] = new HimShootSpike(this);
        states[StateType.BurstSpike] = new HimBurstSpike(this);
        states[StateType.SingExplode] = new HimSingExplode(this);
        TransitionState(StateType.Move); // stepBack写在fsm里面
    }

    private void LateUpdate()
    {
        Debug.Log(currentState);
    }

    public override void Kill()
    {
        base.Kill();
        TransitionState(StateType.Move);
    }
}

public class HimMove : IState
{
    private HimFSM m;
    private double restElapse;
    private float moveJumpElapse;

    public HimMove(HimFSM m)
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
        if (m.stepBackTrigger)
            availableSkills.Add(StateType.StepBack);
        if (m.groundSpikeTrigger)
            availableSkills.Add(StateType.GroundSpike);
        if (m.shootSpikeTrigger)
            availableSkills.Add(StateType.ShootSpike);
        if (m.burstSpikeTrigger)
            availableSkills.Add(StateType.BurstSpike);
        if (m.singExplodeTrigger)
            availableSkills.Add(StateType.SingExplode);

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

public class HimGroundCrush : IState
{
    private HimFSM m;

    public HimGroundCrush(HimFSM m)
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

public class HimAirCrush : IState
{
    private HimFSM m;
    private float elapse;

    public HimAirCrush(HimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        // 跳跃->蓄力
        m.LookTowardsPlayer();
        m.SetGravity(0);
        var jumpTargetPos = m.transform.position +
                            new Vector3(m.facingDirection * m.airJumpDistVector.x, m.airJumpDistVector.y);
        m.transform.DOJump(jumpTargetPos, m.airJumpForce, 1, m.airJumpDuration).onComplete +=
            () =>
            {
                // 冲向player
                PlayerFSM player = PlayerFSM.instance;
                var dir = (player.transform.position - m.transform.position).normalized;
                m.SetVelocity(dir * m.airCrushSpeed);
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
        m.airCrushCooldownExpireTime = Time.time + m.airCrushCooldown;
        m.ResetGravity();
        elapse = 0;
    }
}

public class HimStepBack : IState
{
    private HimFSM m;

    public HimStepBack(HimFSM m)
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

public class HimGroundSpike : IState
{
    private HimFSM m;
    private bool hasSpawnSpikes;
    private bool hasMovedDown;
    private float targetX;

    private int spawnedSpikeNumber;
    private int killedSpikeNumber;

    public HimGroundSpike(HimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
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

        targetX = x2;
    }

    public void OnUpdate()
    {
        if (!m.isMovingDown)
            return;

        if (!hasMovedDown)
        {
            hasMovedDown = true;
            m.StartCoroutine(RemainForSeconds());
        }

        if (m.isOnGround && !hasSpawnSpikes)
        {
            hasSpawnSpikes = true;
            SpawnSpikes();
        }

        if (hasSpawnSpikes && killedSpikeNumber == spawnedSpikeNumber)
        {
            m.TransitionState(StateType.Move);
        }
    }

    private IEnumerator RemainForSeconds()
    {
        m.SetPositionX(targetX);
        m.SetGravity(0);
        m.SetVelocity(0, 0);
        yield return new WaitForSeconds(m.jumpBeforeDropRemainTime);
        m.SetVelocity(0, -m.dropSpeed);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.groundSpikeCooldownExpireTime = Time.time + m.groundSpikeCooldown;
        hasSpawnSpikes = false;
        hasMovedDown = false;
        m.ResetGravity();
        spawnedSpikeNumber = 0;
        killedSpikeNumber = 0;
    }

    private void SpawnSpikes()
    {
        Vector3 position = m.transform.position;
        LayerMask platformMask = 1 << LayerMask.NameToLayer("Platform");
        // 获得him脚下的初始位置
        var startPos = Physics2D.Raycast(position, Vector2.down, Single.PositiveInfinity, platformMask).point;

        // right[0, +oo)
        // left(-oo, -1]
        void Helper(int start, int dir)
        {
            while (true)
            {
                var newPos = new Vector2(startPos.x + m.groundSpikeGap * start, startPos.y);
                if (!Physics2D.OverlapCircle(newPos, 0.01f, platformMask))
                    return;
                SpawnSpike(newPos);
                start += dir;
            }
        }

        Helper(0, 1);
        Helper(-1, -1);
    }

    private void SpawnSpike(Vector2 pos)
    {
        spawnedSpikeNumber += 1;
        var spike = Object.Instantiate(m.spikePrefab, pos, quaternion.identity);
        spike.Resize(m.groundSpikeInitialLength);
        spike.SetDirUp();
        spike.TurnOffTrigger();
        spike.StartCoroutine(SpikeStrike(spike));
    }

    private IEnumerator SpikeStrike(HimSpike spike)
    {
        yield return new WaitForSeconds(m.groundSpikeWindUpTime); // 前摇，给player的准备时间

        spike.TurnOnTrigger();
        spike.Resize(m.groundSpikeStrikeLength, m.groundSpikeStrikeDuration);
        yield return new WaitForSeconds(m.groundSpikeStrikeDuration);

        yield return new WaitForSeconds(m.groundSpikeRemainTimeAfterStrike); // 刺戳出来后的停顿

        spike.Resize(0, m.groundSpikeStrikeDuration);
        yield return new WaitForSeconds(m.groundSpikeStrikeDuration);

        spike.Kill();
        killedSpikeNumber += 1;
    }
}

public class HimShootSpike : IState
{
    private HimFSM m;
    private float elapse;

    public HimShootSpike(HimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.SetVelocity(0, 0);
        m.StartCoroutine(SpawnSpikes());
    }


    private IEnumerator SpawnSpikes()
    {
        float startAngle;
        float dir;
        if (m.transform.position.x < PlayerFSM.instance.transform.position.x)
        {
            startAngle = m.shootSpikeStartAngle;
            dir = 1;
        }
        else
        {
            startAngle = -180 - m.shootSpikeStartAngle;
            dir = -1;
        }

        for (int i = 0; i < m.shootSpikeNumber; i++)
        {
            var angle = startAngle + m.shootSpikeDeltaAngle * i * dir;
            var pushedDir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            var spike = Object.Instantiate(m.spikePrefab, m.transform.position, Quaternion.identity);
            spike.Resize(m.shootSpikeLength);
            spike.SetDir(angle);
            spike.Pushed(pushedDir * m.shootSpikeSpeed);
            yield return new WaitForSeconds(m.shootSpikeTimeGap);
        }
    }


    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse > 1f)
        {
            m.TransitionState(StateType.Move);
        }
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.shootSpikeCooldownExpireTime = Time.time + m.shootSpikeCooldown;
        elapse = 0;
    }
}

public class HimBurstSpike : IState
{
    private HimFSM m;

    public HimBurstSpike(HimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.SetVelocity(0, 0);
        m.StartCoroutine(Burst());
    }

    public IEnumerator Burst()
    {
        float startDeltaAngle = 360f / m.burstSpikeNumber / (m.burstSpikeWave + 1);
        for (int i = 0; i < m.burstSpikeWave; i++)
        {
            float startAngle = startDeltaAngle * i;
            for (int j = 0; j < m.burstSpikeNumber; j++)
            {
                float deltaAngle = 360 / m.burstSpikeNumber;
                var angle = startAngle + deltaAngle * j;
                m.StartCoroutine(BurstOne(angle));
            }

            yield return new WaitForSeconds(m.burstSpikeWaveTimeGap);
        }

        m.TransitionState(StateType.Move);
    }

    private IEnumerator BurstOne(float angle)
    {
        var pushedDir = Quaternion.Euler(0, 0, angle) * Vector2.right;
        var spike = Object.Instantiate(m.spikePrefab, m.transform.position, Quaternion.identity);
        spike.Resize(m.burstSpikeLength);
        spike.SetDir(angle);
        spike.TurnOffTrigger();
        spike.Pushed(pushedDir * m.burstSpikeSpeed / 10);
        yield return new WaitForSeconds(m.burstSpikeWindupTime);
        spike.Pushed(pushedDir * m.burstSpikeSpeed);
        spike.TurnOnTrigger();
    }


    public void OnUpdate()
    {
    }


    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.burstSpikeCooldownExpireTime = Time.time + m.burstSpikeCooldown;
    }
}

public class HimSingExplode : IState
{
    private HimFSM m;
    private float finishedCount;

    public HimSingExplode(HimFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.SetVelocity(0, 0);
        for (int i = 0; i < m.singRadius.Length; i++)
        {
            m.StartCoroutine(Sing(i));
        }
    }

    private IEnumerator Sing(int i)
    {
        m.singCircle[i].Resize(m.singRadius[i], m.singRadius[i] / m.singRadius[^1] * m.singExpandDuration);
        yield return new WaitForSeconds(m.singExpandDuration);

        m.singCircle[i].ChangeColor(m.turnToColor, m.singRemainDuration);
        yield return new WaitForSeconds(m.singRemainDuration);

        m.singCircle[i].TurnOnTrigger();
        m.singCircle[i].Resize(0, m.singShrinkDuration);
        yield return new WaitForSeconds(m.singShrinkDuration);
        m.singCircle[i].TurnOffTrigger();
        m.singCircle[i].Reset();
        finishedCount += 1;
    }

    public void OnUpdate()
    {
        if (finishedCount == m.singCircle.Length)
            m.TransitionState(StateType.Move);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.singExplodeCooldownExpireTime = Time.time + m.singExplodeCooldown;
        finishedCount = 0;
        foreach (var sing in m.singCircle)
        {
            sing.Reset();
        }
    }
}