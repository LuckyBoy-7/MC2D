using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class CreeperBossFSM : GroundEnemyFSM
{
    [Header("Debug")] public bool unlockCrush = true;
    public bool unlockVomit = true;
    public bool unlockJump = true;
    public bool unlockThrow = true;
    public bool unlockWideExplosion = true;
    [Header("Movement")] public float moveSpeed;
    [Header("Prefabs")] public TNT tntPrefab;

    [Header("Global")] public float restTimeAfterEachSkill;

    [Header("Crush")] [Range(0, 1)] public float crushTriggerThreshold = 1; // 触发时的血量百分比
    public float crushCooldown;
    public float crushCooldownExpireTime;
    public BoxCollider2D crushDetectArea;

    public float crushJumpBackDist;
    public float crushJumpBackDuration;
    public float chargeForceTime;
    public float crushAheadDist;
    public float crushAheadDuration;
    public AudioClip crushSfxSound;
    [Header("Vomit")] [Range(0, 1)] public float vomitTriggerThreshold = 0.8f; // 触发时的血量百分比
    public BoxCollider2D vomitDetectArea;

    public float vomitCooldown;
    public float vomitCooldownExpireTime;
    public int vomitTNTNumber;
    public float vomitTimeGap;
    public float vomitForceReduce;
    public float vomitMaxForce;

    [Header("Jump")] [Range(0, 1)] public float jumpTriggerThreshold = 0.7f; // 触发时的血量百分比
    public BoxCollider2D jumpDetectArea;
    public float jumpCooldown;
    public float jumpCooldownExpireTime;
    public float jumpForce;
    public float jumpAngle;

    [Header("Throw")] [Range(0, 1)] public float throwTriggerThreshold = 0.6f; // 触发时的血量百分比
    public BoxCollider2D throwTNTDetectArea;

    public float throwCooldown;
    public float throwCooldownExpireTime;
    public int throwTNTNumber;
    public float throwForce;
    public float throwAngle;

    [Header("WideExplosion")] [Range(0, 1)]
    public float wideExplosionTriggerThreshold = 0.4f; // 触发时的血量百分比

    public Transform[] wideExplosionPoints;
    public float wideExplosionCooldown;
    public float wideExplosionCooldownExpireTime;
    public int wideExplosionTNTNumber;


    #region Trigger

    private bool IsHealPointInThreshold(float threshold) => healthPoint <= maxHealthPoint * threshold;

    public bool crushTrigger => Time.time >= crushCooldownExpireTime && IsHealPointInThreshold(crushTriggerThreshold) &&
                                crushDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockCrush;

    public bool vomitTrigger => Time.time >= vomitCooldownExpireTime && IsHealPointInThreshold(vomitTriggerThreshold) &&
                                vomitDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockVomit;

    public bool jumpTrigger => Time.time >= jumpCooldownExpireTime && IsHealPointInThreshold(jumpTriggerThreshold) &&
                               jumpDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockJump;

    public bool throwTrigger => Time.time >= throwCooldownExpireTime && IsHealPointInThreshold(throwTriggerThreshold) &&
                                throwTNTDetectArea.IsTouching(PlayerFSM.instance.hitBoxCollider) && unlockThrow;

    public bool wideExplosionTrigger => Time.time >= wideExplosionCooldownExpireTime &&
                                        IsHealPointInThreshold(wideExplosionTriggerThreshold) && unlockWideExplosion;

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float half = jumpAngle / 2;
        Vector2 left = Quaternion.Euler(0, 0, 90 + half) * Vector3.right;
        Gizmos.DrawRay(transform.position, left);
        Vector2 right = Quaternion.Euler(0, 0, 90 - half) * Vector3.right;
        Gizmos.DrawRay(transform.position, right);
    }

    protected override void Start()
    {
        base.Start();
        states[StateType.Move] = new CreeperBossMove(this);
        states[StateType.Crush] = new CreeperBossCrush(this);
        states[StateType.Vomit] = new CreeperBossVomit(this);
        states[StateType.Jump] = new CreeperBossJump(this);
        states[StateType.Throw] = new CreeperBossThrow(this);
        states[StateType.WideExplosion] = new CreeperBossWideExplosion(this);
        TransitionState(StateType.Move);
    }
}

public class CreeperBossMove : IState
{
    private CreeperBossFSM m;
    private double elapse;

    public CreeperBossMove(CreeperBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse <= m.restTimeAfterEachSkill)
            return;
        List<StateType> availableSkills = new();
        if (m.crushTrigger)
            availableSkills.Add(StateType.Crush);
        if (m.vomitTrigger)
            availableSkills.Add(StateType.Vomit);
        if (m.jumpTrigger)
            availableSkills.Add(StateType.Jump);
        if (m.throwTrigger)
            availableSkills.Add(StateType.Throw);
        if (m.wideExplosionTrigger)
            availableSkills.Add(StateType.WideExplosion);

        if (availableSkills.Count > 0)
            m.TransitionState(availableSkills[Random.Range(0, availableSkills.Count)]);
    }


    public void OnFixedUpdate()
    {
        m.LookTowardsPlayer();
        m.LerpVelocityX(m.facingDirection * m.moveSpeed);
    }

    public void OnExit()
    {
        elapse = 0;
    }
}

public class CreeperBossCrush : IState
{
    private CreeperBossFSM m;

    public CreeperBossCrush(CreeperBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.SetVelocityX(0);
        var s = DOTween.Sequence();
        s.Append(m.rigidbody.DOMoveX(m.rigidbody.position.x - m.facingDirection * m.crushJumpBackDist,
            m.crushJumpBackDuration));
        s.AppendInterval(m.chargeForceTime);
        s.AppendCallback(() => AudioManager.instance.Play(m.crushSfxSound));
        s.Append(m.rigidbody.DOMoveX(m.rigidbody.position.x + m.facingDirection * m.crushAheadDist,
            m.crushAheadDuration));
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
        m.crushCooldownExpireTime = Time.time + m.crushCooldown;
    }
}

public class CreeperBossVomit : IState
{
    private CreeperBossFSM m;

    public CreeperBossVomit(CreeperBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.StartCoroutine(Vomit());
    }

    private IEnumerator Vomit()
    {
        for (int i = 0; i < m.vomitTNTNumber; i++)
        {
            var tnt = Object.Instantiate(m.tntPrefab, m.transform.position, Quaternion.identity);
            tnt.Pushed(m.facingDirection * (m.vomitMaxForce - m.vomitForceReduce * i) * Vector2.right);
            yield return new WaitForSeconds(m.vomitTimeGap);
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
        m.vomitCooldownExpireTime = Time.time + m.vomitCooldown;
    }
}

public class CreeperBossThrow : IState
{
    private CreeperBossFSM m;

    public CreeperBossThrow(CreeperBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        var startAngle = 90 + m.throwAngle / 2;
        var angleDelta = m.throwTNTNumber > 1 ? m.throwAngle / (m.throwTNTNumber - 1) : 0;
        for (int i = 0; i < m.throwTNTNumber; i++)
        {
            var tnt = Object.Instantiate(m.tntPrefab, m.transform.position, Quaternion.identity);
            var dir = Quaternion.Euler(0, 0, startAngle - angleDelta * i) * Vector3.right;
            tnt.Pushed(dir * m.throwForce);
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

public class CreeperBossJump : IState
{
    private CreeperBossFSM m;

    public CreeperBossJump(CreeperBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        PlayerFSM player = PlayerFSM.instance;

        var half = m.jumpAngle / 2;
        var angle = player.transform.position.x < m.transform.position.x
            ? Random.Range(90, 90 + half)
            : Random.Range(90 - half, 90);
        Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
        m.SetVelocity(dir * m.jumpForce);
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
public class CreeperBossWideExplosion : IState
{
    private CreeperBossFSM m;

    public CreeperBossWideExplosion(CreeperBossFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        int i = 0;
        Transform[] lst = m.wideExplosionPoints;
        for (int j = 0; j < m.wideExplosionTNTNumber; j++)
        {
            int k = Random.Range(i, lst.Length); // 随机number个pos出来
            (lst[i], lst[k]) = (lst[k], lst[i]);
            i += 1;
        }

        for (int j = 0; j < m.wideExplosionTNTNumber; j++)
        {
            var tnt = Object.Instantiate(m.tntPrefab, lst[j].position, Quaternion.identity);
            tnt.SetFloat();
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
        m.wideExplosionCooldownExpireTime = Time.time + m.wideExplosionCooldown;
    }
}