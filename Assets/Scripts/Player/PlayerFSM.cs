using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class PlayerFSM : SingletonFSM<PlayerFSM>
{
    public SpriteRenderer spriteRenderer;
    [Header("Debug")] public bool isDebug;
    public bool isInvincibleDebug;
    public bool isOverPowerDebug;
    public bool isAllAbilityDebug;
    [Header("Currency")] public int curEmeraldNumber;
    [Header("Collectable")] public int strawberryNumber;
    [Header("Health")] public int maxHealthPoint;
    public int healthPoint;

    [Header("CameraConfiner")] public CinemachineConfiner2D cinemachineConfiner2D;

    [Header("Movement")] public Rigidbody2D rigidbody;

    public Vector2 facingDirection;
    public Vector2 keyDownDirection;
    public float moveSpeed;
    public float moveChangeSpeed;
    [Header("Idle")] public float dampingSpeed;
    [Header("Jump")] public bool canDoubleJump = true;
    public float firstJumpForce;
    public float firstJumpAdditionalForce;
    public float doubleJumpForce;
    public float doubleJumpAdditionalForce;
    public float jumpBufferTime;
    public float jumpBufferExpireTime = -1;
    public float wolfJumpBufferTime;
    public float wolfJumpBufferExpireTime;
    [Header("Fall")] public float maxFallingSpeed;
    [Header("Dash")] public bool canDash;
    public float dashSpeed;
    public float dashDuration;
    public float dashBufferTime;
    public float dashBufferExpireTime;
    public float dashCoolDown;
    public float dashCoolDownExpireTime;
    [Header("WallSlide")] public float wallSlideSpeed;
    [Header("WallJump")] public Vector2 wallJumpForce;
    public float wallJumpAdditionalForce;
    public float wallJumpMoveChangeSpeed;
    public float wallWolfJumpBufferTime;
    public float wallWolfJumpBufferExpireTime;


    [Header("RaycastCheck")] public Collider2D hitBoxCollider;
    public float boxLeftRightCastDist;
    public float wallCheckBoxHeight;
    public float boxDownCastDist;
    public LayerMask groundLayer;

    [Header("Attack")] public Animator attackRight;
    public PolygonCollider2D attackColliderRight;
    public Animator attackDown;
    public PolygonCollider2D attackColliderDown;
    public Animator attackUp;
    public PolygonCollider2D attackColliderUp;
    public float attackCoolDown;
    public float attackCoolDownExpireTime;
    public float attackForce;
    public float attackDownForceMultiplier;
    public float attackMoveChangeSpeed;
    public float attackBufferTime;
    public float attackBufferExpireTime;
    public int attackDamage;
    public int expAmountExtractedByAttack = 1;

    [Header("Exp")] public int maxExp = 9;
    public int curExp = 0;

    [Header("Hurt")] public float showHurtEffectTime;
    public Vector2 hurtDirection;
    public float hurtDirectionXMultiplier; // 方向加一个维度的缩放配合force就可以构造出360度任意受力情况
    public float hurtForce;
    public float invincibleTime;
    public float invincibleExpireTime;
    public event Action onDie;

    [Header("Recover")] public float recoverSpeed;
    public Animator recoverProcessAnim;
    public Animator recoverBurstAnim;

    [Header("Effect")] public SwordAttackEffect attackEffect;

    [Header("Spell")] public float spellPreparationTime; // 就是如果要施放法术，按键至少要摁这么久，否则就是聚集回血
    public float spellPreparationExpireTime = Single.PositiveInfinity; // 只在地上的时候有用，空中都是秒放的，地上才要判断
    [Header("ReleaseArrow")] public PlayerArrowPrefab arrowPrefab;
    public int arrowDamage;
    public float arrowSpeed;
    public float spellArrowKnockBackForce;
    public float spellArrowMoveChangeSpeed;
    public float frozeFrameTime;

    [Header("Drop")] public float dropWindupDeltaCompensation;
    public float dropWindupTime;
    public float windupForce;
    public float dropSpeed;
    public float dropInvincibleTime;

    public PlayerDropPrefab dropPrefab;
    public int dropDamage;
    public float dropDamageTimeGap;

    [Header("Roar")] public PlayerRoarPrefab roarPrefab;
    public int roarDamage;
    public float roarDamageTimeGap;
    public float roarOverTimePercent = 0.8f;

    [Header("SuperDash")] public SpriteRenderer spikePrefab;
    public int oneSideSpikeCount;
    public float spikeSpacing;
    public float spikeMinScaleY;
    public float spikeMaxScaleY;
    public float spikePopUpTime;
    public float spikePopUpTimeGap;
    public float superDashMoveChangeSpeed;
    public float superDashSpeed;

    [Header("Key")] public KeyCode leftKey = KeyCode.J;
    public KeyCode rightKey = KeyCode.L;
    public KeyCode upKey = KeyCode.I;
    public KeyCode downKey = KeyCode.K;
    public KeyCode jumpKey = KeyCode.D;
    public KeyCode dashKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.A;
    public KeyCode spellKey = KeyCode.Q;
    public KeyCode superDashKey = KeyCode.W;

    [Header("States")] public bool hasDashAbility;
    public bool hasRoarAbility;
    public bool hasDoubleJumpAbility;
    public bool hasSuperDashAbility;
    public bool hasDropAbility;
    public bool hasReleaseArrowAbility;
    public bool hasWallSlideAbility;


    void Start()
    {
        healthPoint = maxHealthPoint;
        states[StateType.Idle] = new PlayerIdle(this);
        states[StateType.Run] = new PlayerRun(this);
        states[StateType.Hurt] = new PlayerHurt(this);
        states[StateType.Jump] = new PlayerJump(this);
        states[StateType.WallJump] = new PlayerWallJump(this);
        states[StateType.WallSlide] = new PlayerWallSlide(this);
        states[StateType.Fall] = new PlayerFall(this);
        states[StateType.Dash] = new PlayerDash(this);
        states[StateType.DoubleJump] = new PlayerDoubleJump(this);
        states[StateType.Attack] = new PlayerAttack(this);
        states[StateType.Spell] = new PlayerSpell(this);
        states[StateType.ReleaseArrow] = new PlayerReleaseArrow(this);
        states[StateType.Roar] = new PlayerRoar(this);
        states[StateType.Drop] = new PlayerDrop(this);
        states[StateType.Recover] = new PlayerRecover(this);
        states[StateType.SuperDash] = new PlayerSuperDash(this);
        TransitionState(StateType.Idle);
    }

    private void Update()
    {
        if (GameManager.instance.state != GameStateType.Play)
            return;
        UpdateKeyDownDirection();
        UpdateInputBuffer();
        UpdateStates();
        TryUpdateDebug();

        currentState.OnUpdate();

        // Debug.Log($"currentState: {currentState}");
        // Debug.Log($"isOnGround: {isOnGround}");
        // Debug.Log($"isOnLeftWall: {isOnLeftWall}");
        // Debug.Log($"isOnRightWall: {isOnRightWall}");
    }

    private void LateUpdate()
    {
        LateUpdateInputBuffer();
    }

    private void TryUpdateDebug()
    {
        if (!isDebug)
            return;
        curExp = maxExp;
        PlayerExpUI.instance.UpdatePlayerExpUI();
        canDoubleJump = true;
        canDash = true;
        if (isInvincibleDebug)
            invincibleExpireTime = Time.time + invincibleTime;
        if (isOverPowerDebug)
            attackDamage = 10;
        if (isAllAbilityDebug)
        {
            hasDashAbility = true;
            hasRoarAbility = true;
            hasDoubleJumpAbility = true;
            hasSuperDashAbility = true;
            hasDropAbility = true;
            hasReleaseArrowAbility = true;
            hasWallSlideAbility = true;
        }
    }

    public void UpdateTriggerEnter2D(Collider2D other)
    {
        UpdateCollisionHurt(other);
        UpdateEmeraldSuck(other);
    }


    public void UpdateTriggerStay2D(Collider2D other)
    {
        UpdateCollisionHurt(other);
        UpdateBoundSwitch(other);
        UpdateHiddenChannelShowUp(other);
    }

    public void UpdateTriggerExit2D(Collider2D other)
    {
        UpdateHiddenChannelHide(other);
    }

    private void UpdateHiddenChannelShowUp(Collider2D other)
    {
        if (!other.CompareTag("HiddenChannel"))
            return;
        HiddenChannel.instance.FadeTo(0);
    }

    private void UpdateHiddenChannelHide(Collider2D other)
    {
        if (!other.CompareTag("HiddenChannel"))
            return;
        HiddenChannel.instance.FadeTo(1);
    }


    private void UpdateBoundSwitch(Collider2D other)
    {
        if (!other.CompareTag("Bound"))
            return;
        if (other.OverlapPoint(transform.position))
            cinemachineConfiner2D.m_BoundingShape2D = other;
    }

    private void UpdateEmeraldSuck(Collider2D other)
    {
        if (other.CompareTag("Emerald"))
        {
            curEmeraldNumber += 1;
            EmeraldUI.instance.UpdatePlayerEmeraldUI();
            Destroy(other.gameObject);
        }
    }

    private void UpdateCollisionHurt(Collider2D other)
    {
        bool damageable = other.CompareTag("Enemy")
                          || other.CompareTag("Spike");
        if (!damageable)
            return;
        TryTakeDamage(1, other.transform.position);
    }

    public void TryTakeDamage(int damage, Vector2 from)
    {
        if (isInvincible)
            return;
        hurtDirection = from.x > transform.position.x
            ? new Vector2(-hurtDirectionXMultiplier, 1)
            : new Vector2(hurtDirectionXMultiplier, 1);
        TakeDamage(damage);
        TransitionState(StateType.Hurt);
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }

    private void UpdateStates()
    {
        if (isOnGround)
        {
            canDoubleJump = true;
            canDash = true; // 虽然你冷却好了，预输入也有，但是不能冲就是不能冲
        }
    }

    private void UpdateInputBuffer()
    {
        if (Input.GetKeyDown(jumpKey))
            jumpBufferExpireTime = Time.time + jumpBufferTime;
        if (Input.GetKeyDown(dashKey))
            dashBufferExpireTime = Time.time + dashBufferTime;
        if (Input.GetKeyDown(attackKey))
            attackBufferExpireTime = Time.time + attackBufferTime;
        if (Input.GetKeyDown(spellKey))
            spellPreparationExpireTime = Time.time + spellPreparationTime;
        if (isOnGround)
        {
            wolfJumpBufferExpireTime = Time.time + wolfJumpBufferTime;
            wallWolfJumpBufferExpireTime = -1; // 如果在墙上滑倒底但是不跳。其实应该可以不加，因为毕竟0.1s很短
        }

        if (currentState == states[StateType.WallSlide])
            wallWolfJumpBufferExpireTime = Time.time + wallWolfJumpBufferTime;
    }

    private void LateUpdateInputBuffer()
    {
        if (Input.GetKeyUp(spellKey)) // 防止这个更新的比trigger更快，所以放到lateUpdate里了
            spellPreparationExpireTime = Single.PositiveInfinity;
    }

    #region PhysicsCheck

    public bool isOnGround =>
        Physics2D.OverlapBox(transform.position + Vector3.down * (0.5f + boxDownCastDist / 2),
            new Vector2(0.95f, boxDownCastDist), 0, groundLayer);

    public bool isOnRightWall =>
        Physics2D.OverlapBox(
            transform.position + Vector3.right * (0.5f + boxLeftRightCastDist / 2) +
            Vector3.down * (0.45f - wallCheckBoxHeight / 2),
            new Vector2(boxLeftRightCastDist, wallCheckBoxHeight), 0, groundLayer);

    public bool isOnLeftWall =>
        Physics2D.OverlapBox(
            transform.position + Vector3.left * (0.5f + boxLeftRightCastDist / 2) +
            Vector3.down * (0.45f - wallCheckBoxHeight / 2),
            new Vector2(boxLeftRightCastDist, wallCheckBoxHeight), 0, groundLayer);

    public bool isOnWall => isOnLeftWall || isOnRightWall;
    public bool isHittingWall => isOnLeftWall && facingDirection.x == -1 || isOnRightWall && facingDirection.x == 1;
    public bool isOffWall => isOnLeftWall && facingDirection.x == 1 || isOnRightWall && facingDirection.x == -1;

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Ground Box
        Gizmos.DrawWireCube(transform.position + Vector3.down * (0.5f + boxDownCastDist / 2),
            new Vector2(1, boxDownCastDist));
        // Wall Box
        Gizmos.DrawWireCube(
            transform.position + Vector3.right * (0.5f + boxLeftRightCastDist / 2) +
            Vector3.down * (0.45f - wallCheckBoxHeight / 2),
            new Vector2(boxLeftRightCastDist, wallCheckBoxHeight));
        Gizmos.DrawWireCube(
            transform.position + Vector3.left * (0.5f + boxLeftRightCastDist / 2) +
            Vector3.down * (0.45f - wallCheckBoxHeight / 2),
            new Vector2(boxLeftRightCastDist, wallCheckBoxHeight));
        // Hit Box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, hitBoxCollider.bounds.size);
    }

    public void UpdateKeyDownDirection()
    {
        var x = Input.GetKey(leftKey) ? -1 : Input.GetKey(rightKey) ? 1 : 0;
        var y = Input.GetKey(downKey) ? -1 : Input.GetKey(upKey) ? 1 : 0;
        keyDownDirection = new Vector2(x, y);
    }

    public void UpdateFacingDirection() // 看起来像是没什么用，但是如果我放黑波的时候改变了方向，下次就算没动键盘放出来的波会向相反方向移动
    {
        var (x, y) = (keyDownDirection.x, keyDownDirection.y);
        if (x != 0)
            facingDirection.x = x;
        if (y != 0)
            facingDirection.y = y;
        transform.localScale = new Vector3(facingDirection.x, 1, 1);
    }

    public void ReverseFacingDirection()
    {
        facingDirection.x *= -1;
        transform.localScale = new Vector3(facingDirection.x, 1, 1);
    }

    public void TakeDamage(int damage)
    {
        healthPoint -= damage;
        HealthUI.instance.UpdateUI();
        spriteRenderer.color = Color.red;
        spriteRenderer.DOColor(Color.white, showHurtEffectTime);
        TransitionState(StateType.Hurt);
        if (healthPoint == 0)
            Kill();
    }

    private void Kill()
    {
        onDie();
        gameObject.SetActive(false);
    }

    public bool isInvincible => Time.time <= invincibleExpireTime;

    public void PlayAttackEffect(Vector3 position) => Instantiate(attackEffect, position, Quaternion.identity);

    #region StateTransitionTrigger

    public bool wallSlideTrigger => hasWallSlideAbility && (
        (isOnLeftWall && Input.GetKey(leftKey) && rigidbody.velocity.x <= 1e-3 // 不然蹬墙跳一出去就就又变成wallSlide状态了
         || isOnRightWall && Input.GetKey(rightKey) && rigidbody.velocity.x >= -1e-3));

    public bool jumpTrigger => // 就是尝试跳跃后，如果在地上或不在地上但是狼跳还在
        Time.time <= jumpBufferExpireTime && (isOnGround || Time.time <= wolfJumpBufferExpireTime);

    public bool wallJumpTrigger => // 就是尝试跳跃后，如果在墙上或不在墙上但是狼跳还在
        Time.time <= jumpBufferExpireTime && Time.time <= wallWolfJumpBufferExpireTime;

    public bool doubleJumpTrigger => hasDoubleJumpAbility && Time.time <= jumpBufferExpireTime && canDoubleJump;

    public bool dashTrigger => hasDashAbility &&
                               Time.time <= dashBufferExpireTime && Time.time >= dashCoolDownExpireTime && canDash;

    public bool moveTrigger => Input.GetKey(leftKey) || Input.GetKey(rightKey);
    public bool fallTrigger => rigidbody.velocity.y < -1e-3;
    public bool offWallTrigger => isOnLeftWall && Input.GetKey(rightKey) || isOnRightWall && Input.GetKey(leftKey);
    public bool attackTrigger => Time.time >= attackCoolDownExpireTime && Time.time <= attackBufferExpireTime;

    public bool spellTrigger => curExp >= 3
                                && (isOnGround && Input.GetKeyUp(spellKey) && Time.time <= spellPreparationExpireTime
                                    || !isOnGround && Input.GetKeyDown(spellKey));

    public bool recoverTrigger => curExp >= 3
                                  && isOnGround && Input.GetKey(spellKey) && Time.time > spellPreparationExpireTime;

    public bool superDashTrigger => hasSuperDashAbility && Input.GetKeyDown(superDashKey);

    #endregion
}

public class PlayerIdle : IState
{
    private PlayerFSM m;

    public PlayerIdle(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        m.UpdateFacingDirection();
        if (m.moveTrigger)
            m.TransitionState(StateType.Run);
        else if (m.jumpTrigger)
            m.TransitionState(StateType.Jump);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
        else if (m.recoverTrigger)
            m.TransitionState(StateType.Recover);
        else if (m.superDashTrigger)
            m.TransitionState(StateType.SuperDash);
        else if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, 0, m.dampingSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
    }
}

public class PlayerRun : IState
{
    private PlayerFSM m;

    public PlayerRun(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        m.UpdateFacingDirection();
        if (!m.moveTrigger)
            m.TransitionState(StateType.Idle);
        else if (m.jumpTrigger)
            m.TransitionState(StateType.Jump);
        else if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
        else if (m.recoverTrigger)
            m.TransitionState(StateType.Recover);
        else if (m.superDashTrigger)
            m.TransitionState(StateType.SuperDash);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.moveSpeed * m.keyDownDirection.x,
            m.moveChangeSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
    }
}

public class PlayerHurt : IState
{
    private PlayerFSM m;

    public PlayerHurt(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.invincibleExpireTime = Time.time + m.invincibleTime;
        m.rigidbody.velocity = m.hurtDirection * m.hurtForce;
    }

    public void OnUpdate()
    {
        m.TransitionState(StateType.Fall);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
    }
}

public class PlayerJump : IState
{
    private PlayerFSM m;
    private bool isKeyReleased;

    public PlayerJump(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = Vector2.up * m.firstJumpForce;
        m.jumpBufferExpireTime = -1; // 重置，否则如果从跳跃到落地的时间<bufferTime，则跳跃会被触发两次 
        m.wolfJumpBufferExpireTime = -1;
    }

    public void OnUpdate()
    {
        m.UpdateFacingDirection();
        isKeyReleased = isKeyReleased || Input.GetKeyUp(m.jumpKey);

        if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
        else if (m.doubleJumpTrigger)
            m.TransitionState(StateType.DoubleJump);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
    }

    public void OnFixedUpdate()
    {
        if (!isKeyReleased)
            m.rigidbody.AddForce(m.firstJumpAdditionalForce * Vector2.up); // 因为更新速率恒定，所以加不加deltatime都一样

        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.moveSpeed * m.keyDownDirection.x,
            m.moveChangeSpeed);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}

public class PlayerWallJump : IState
{
    private PlayerFSM m;
    private bool isKeyReleased;

    public PlayerWallJump(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = new Vector2(m.facingDirection.x * m.wallJumpForce.x, m.wallJumpForce.y);
        m.jumpBufferExpireTime = -1; // 重置，否则如果从跳跃到落地的时间<bufferTime，则跳跃会被触发两次 
        m.wallWolfJumpBufferExpireTime = -1;
    }


    public void OnUpdate()
    {
        m.UpdateFacingDirection();
        isKeyReleased = isKeyReleased || Input.GetKeyUp(m.jumpKey);

        if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.doubleJumpTrigger)
            m.TransitionState(StateType.DoubleJump);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
    }

    public void OnFixedUpdate()
    {
        float newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.moveSpeed * m.keyDownDirection.x,
            m.wallJumpMoveChangeSpeed);
        if (!isKeyReleased)
            m.rigidbody.AddForce(m.wallJumpAdditionalForce * Vector2.up);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}

public class PlayerWallSlide : IState
{
    private PlayerFSM m;
    private float gravityScaleBackup;

    public PlayerWallSlide(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        gravityScaleBackup = m.rigidbody.gravityScale;
        m.rigidbody.gravityScale = 0;
        m.rigidbody.velocity = new Vector2(0, -m.wallSlideSpeed);
        m.canDash = true;
        m.canDoubleJump = true;
        m.ReverseFacingDirection();
    }

    public void OnUpdate()
    {
        if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else if (m.wallJumpTrigger)
            m.TransitionState(StateType.WallJump);
        else if (m.offWallTrigger || !m.isOnWall) // 手动出去或者被动出去
            m.TransitionState(StateType.Fall);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.superDashTrigger)
            m.TransitionState(StateType.SuperDash);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.rigidbody.gravityScale = gravityScaleBackup;
    }
}

public class PlayerFall : IState
{
    private PlayerFSM m;

    public PlayerFall(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        m.UpdateFacingDirection();

        if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else if (m.wallJumpTrigger) // 墙上的狼跳
            m.TransitionState(StateType.WallJump);
        else if (m.jumpTrigger) // 狼跳
            m.TransitionState(StateType.Jump);
        else if (m.doubleJumpTrigger)
            m.TransitionState(StateType.DoubleJump);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.wallSlideTrigger)
            m.TransitionState(StateType.WallSlide);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
    }

    public void OnFixedUpdate()
    {
        var newY = -Mathf.Min(-m.rigidbody.velocity.y, m.maxFallingSpeed);
        float newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.moveSpeed * m.keyDownDirection.x,
            m.moveChangeSpeed * Time.deltaTime);
        m.rigidbody.velocity = new Vector2(newX, newY);
    }

    public void OnExit()
    {
    }
}

public class PlayerDash : IState
{
    private PlayerFSM m;
    private float gravityScaleBackup;
    private float elapse;

    public PlayerDash(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.dashBufferExpireTime = -1;
        m.canDash = false;
        m.rigidbody.velocity = new Vector2(m.facingDirection.x * m.dashSpeed, 0);
        gravityScaleBackup = m.rigidbody.gravityScale;
        m.rigidbody.gravityScale = 0;
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse <= m.dashDuration)
            return;

        if (m.jumpTrigger)
            m.TransitionState(m.isOnGround ? StateType.Jump : StateType.DoubleJump);
        else if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else if (!m.isOnGround) // 这里的处理要特殊一点，因为player冲刺状态y轴速度恒为0，不这么判断状态就出不去了
            m.TransitionState(StateType.Fall);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.rigidbody.gravityScale = gravityScaleBackup;
        elapse = 0;
        m.dashCoolDownExpireTime = Time.time + m.dashCoolDown;
        m.rigidbody.velocity = Vector2.zero;
    }
}

public class PlayerSpell : IState
{
    private PlayerFSM m;

    public PlayerSpell(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.curExp -= 3;
        PlayerExpUI.instance.UpdatePlayerExpUI();

        if (Input.GetKey(m.downKey) && m.hasDropAbility)
            m.TransitionState(StateType.Drop);
        else if (Input.GetKey(m.upKey) && m.hasRoarAbility)
            m.TransitionState(StateType.Roar);
        else if (m.hasReleaseArrowAbility)
            m.TransitionState(StateType.ReleaseArrow);
        else
            m.TransitionState(m.preStateType);
    }

    public void OnUpdate()
    {
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
    }
}

public class PlayerRoar : IState
{
    private PlayerFSM m;
    private float gravityBackup;
    private PlayerRoarPrefab roarPrefab;
    private Animator anim;
    private AnimatorStateInfo info;

    public PlayerRoar(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        gravityBackup = m.rigidbody.gravityScale;
        m.rigidbody.gravityScale = 0;
        m.rigidbody.velocity = Vector2.zero;
        roarPrefab = PlayerFSM.Instantiate(m.roarPrefab, PlayerFSM.instance.transform.position, Quaternion.identity);
        roarPrefab.transform.position += Vector3.up * 0.5f;
        anim = roarPrefab.GetComponent<Animator>();
    }

    public void OnUpdate()
    {
        info = anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("Roar") && info.normalizedTime <= m.roarOverTimePercent) // 因为提早出来了，所以也没有null的风险
            return;
        if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else
            m.TransitionState(StateType.Fall);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        m.rigidbody.gravityScale = gravityBackup;
    }
}

public class PlayerDrop : IState
{
    private PlayerFSM m;
    private float elapse;
    private float gravityBackup;
    private bool isFirstOnGround;
    private PlayerDropPrefab dropPrefab;

    public PlayerDrop(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = Vector2.up * m.windupForce;
        gravityBackup = m.rigidbody.gravityScale;
        m.rigidbody.gravityScale = 0;
        if (m.isOnGround)
            elapse += m.dropWindupDeltaCompensation;
    }

    public void OnUpdate()
    {
        if (isFirstOnGround)
        {
            if (dropPrefab == null)
                m.TransitionState(StateType.Idle);
            return;
        }

        elapse += Time.deltaTime;
        if (elapse < m.dropWindupTime)
            return;
        // 开始下砸
        m.rigidbody.velocity = Vector2.down * m.dropSpeed;
        UpdateCollisionWithTrapDoor();

        if (m.isOnGround)
        {
            isFirstOnGround = true;
            dropPrefab = PlayerFSM.Instantiate(m.dropPrefab, m.transform);
            dropPrefab.transform.position += Vector3.down * 0.5f;
            m.invincibleExpireTime = Time.time + m.dropInvincibleTime;
        }
    }

    private void UpdateCollisionWithTrapDoor()
    {
        List<Collider2D> res = new();
        m.hitBoxCollider.OverlapCollider(new ContactFilter2D(), res);
        foreach (var other in res)
        {
            if (!other.CompareTag("TrapDoor"))
                return;
            other.GetComponent<TrapDoor>().Attacked();
        }
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
        elapse = 0;
        m.rigidbody.gravityScale = gravityBackup;
        isFirstOnGround = false;
    }
}

public class PlayerReleaseArrow : IState
{
    private PlayerFSM m;
    private float gravityBackup;
    private float elapse;

    public PlayerReleaseArrow(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        var arrow = PlayerFSM.Instantiate(m.arrowPrefab, PlayerFSM.instance.transform.position, Quaternion.identity);
        arrow.SetMove(m.facingDirection.x, m.arrowSpeed);

        gravityBackup = m.rigidbody.gravityScale;
        m.rigidbody.gravityScale = 0;

        m.rigidbody.velocity = new Vector2(-m.facingDirection.x * m.spellArrowKnockBackForce, 0);
        Time.timeScale = 0.1f;
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime / Time.timeScale;
        if (elapse < m.frozeFrameTime)
            return;
        Time.timeScale = 1;

        if (Mathf.Abs(m.rigidbody.velocity.x) > 1e-3) // 后摇还没结束
            return;
        if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else
            m.TransitionState(StateType.Fall);
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, 0, m.spellArrowMoveChangeSpeed);
        m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        m.rigidbody.gravityScale = gravityBackup;
        elapse = 0;
    }
}

public class PlayerDoubleJump : IState
{
    private PlayerFSM m;
    private bool isKeyReleased;

    public PlayerDoubleJump(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.rigidbody.velocity = new Vector2(m.rigidbody.velocity.x, m.doubleJumpForce);
        // m.rigidbody.AddForce(Vector2.up * doubleJumpForce, ForceMode2D.Impulse);
        m.jumpBufferExpireTime = -1; // 重置，否则如果从跳跃到落地的时间<bufferTime，则跳跃会被触发两次 
        m.canDoubleJump = false;
    }


    public void OnUpdate()
    {
        m.UpdateFacingDirection();
        isKeyReleased = isKeyReleased || Input.GetKeyUp(m.jumpKey);

        if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.attackTrigger)
            m.TransitionState(StateType.Attack);
        else if (m.spellTrigger)
            m.TransitionState(StateType.Spell);
        else if (m.wallSlideTrigger)
            m.TransitionState(StateType.WallSlide);
    }

    public void OnFixedUpdate()
    {
        if (!isKeyReleased)
            m.rigidbody.AddForce(m.doubleJumpAdditionalForce * Vector2.up);

        m.rigidbody.velocity = new Vector2(m.moveSpeed * m.keyDownDirection.x, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}

public class PlayerAttack : IState
{
    private PlayerFSM m;
    private AnimatorStateInfo info;
    private Dictionary<Animator, Vector2> attackDirection;
    private Dictionary<Animator, PolygonCollider2D> attackCollider;
    private Animator currentAttack;
    private bool hasAttackSpike;
    private bool hasLootEmeraldPile;
    private bool hasCausedDamage;

    public PlayerAttack(PlayerFSM m)
    {
        this.m = m;
        attackDirection = new()
        {
            { m.attackRight, new Vector2(1, 0) * m.facingDirection.x },
            { m.attackUp, new Vector2(0, 1) },
            { m.attackDown, new Vector2(0, -1) }
        };

        attackCollider = new()
        {
            { m.attackRight, m.attackColliderRight },
            { m.attackUp, m.attackColliderUp },
            { m.attackDown, m.attackColliderDown }
        };
    }

    public void OnEnter()
    {
        m.attackBufferExpireTime = -1;

        List<int> direction = new() { -1, 1 };
        if (Input.GetKey(m.upKey))
            currentAttack = m.attackUp;
        else if (Input.GetKey(m.downKey) && !m.isOnGround)
            currentAttack = m.attackDown;
        else
            currentAttack = m.attackRight;
        currentAttack.transform.localScale = new Vector3(1, direction[Random.Range(0, 2)], 1);
        currentAttack.Play("PlayerAttack");
    }


    public void OnUpdate()
    {
        // 这段话要不停调用（天坑！！！）
        info = currentAttack.GetCurrentAnimatorStateInfo(0);
        // 更新向右攻击的方向
        attackDirection[m.attackRight] = new Vector2(1, 0) * m.facingDirection.x;
        UpdateTrigger();

        if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
        else if (m.doubleJumpTrigger)
            m.TransitionState(StateType.DoubleJump);

        // The integer part is the number of time a state has been looped. The fractional part is the % (0-1) of progress in the current loop.
        // 整数部分是循环次数，小数部分是运行进度, 主要是为了防止攻击的时候转向
        if (info.IsName("PlayerAttack") && info.normalizedTime - (int)info.normalizedTime < 0.95f)
            return;
        m.UpdateFacingDirection();
    }

    public void OnFixedUpdate()
    {
        var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, m.moveSpeed * m.keyDownDirection.x,
            m.attackMoveChangeSpeed);
        // 我感觉如果要写的更精细的话就要有更多的状态，比如FallAttack，RunAttack之类的
        if (m.fallTrigger)
        {
            var newY = -Mathf.Min(-m.rigidbody.velocity.y, m.maxFallingSpeed);
            m.rigidbody.velocity = new Vector2(newX, newY);
        }
        else
            m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
    }

    public void OnExit()
    {
        m.attackCoolDownExpireTime = Time.time + m.attackCoolDown;
        hasAttackSpike = false;
        hasLootEmeraldPile = false;
        hasCausedDamage = false;
    }

    public void UpdateTrigger()
    {
        List<Collider2D> colliders = new();
        Physics2D.OverlapCollider(attackCollider[currentAttack], new ContactFilter2D { useTriggers = true }, colliders);
        UpdateAttackEnemyTrigger(colliders);
        foreach (var other in colliders)
        {
            UpdateAttackSpikeTrigger(other);
            UpdateAttackArrowTrigger(other);
            UpdateAttackEmeraldPileTrigger(other);
            UpdateAttackWoodenDoorTrigger(other);
        }
    }

    private void UpdateAttackWoodenDoorTrigger(Collider2D other)
    {
        if (other.CompareTag("WoodenDoor"))
        {
            other.GetComponent<WoodenDoor>().Attacked();
            if (currentAttack == m.attackDown) // 只有下劈才刷新
            {
                m.canDoubleJump = true;
                m.canDash = true;
            }

            // 播放特效
            PlayerFSM.instance.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position));
        }
    }

    private void UpdateAttackArrowTrigger(Collider2D other)
    {
        if (other.CompareTag("EnemyArrow"))
        {
            PlayerFSM.Destroy(other.gameObject);
            Pushed();
            PlayerFSM.instance.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position));
        }
    }

    private void UpdateAttackEmeraldPileTrigger(Collider2D other)
    {
        if (hasLootEmeraldPile || !other.CompareTag("EmeraldPile"))
            return;
        other.GetComponent<EmeraldPile>().Looted();
        Pushed();
        hasLootEmeraldPile = true;
        PlayerFSM.instance.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position));
    }

    private void UpdateAttackSpikeTrigger(Collider2D other)
    {
        if (hasAttackSpike || !other.CompareTag("Spike"))
            return;

        hasAttackSpike = true;
        Pushed();
        // 更新属性
        if (currentAttack == m.attackDown) // 只有下劈才刷新
        {
            m.canDoubleJump = true;
            m.canDash = true;
        }

        // 播放特效
        PlayerFSM.instance.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position));
    }

    private void UpdateAttackEnemyTrigger(List<Collider2D> others)
    {
        if (hasCausedDamage)
            return;
        bool anyEnemyAttacked = false;
        foreach (var other in others)
        {
            if (!other.CompareTag("Enemy") || !other.isTrigger) // 只打trigger的，因为enemy有两个碰撞箱
                continue;
            if (!anyEnemyAttacked) // 在第一个打到的敌人上播放特效
                m.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position));
            anyEnemyAttacked = true;
            other.GetComponent<EnemyFSM>().Attacked(m.attackDamage, attackDirection[currentAttack] * m.attackForce);
        }

        if (anyEnemyAttacked)
        {
            hasCausedDamage = true;
            Pushed();
            // 更新属性
            if (currentAttack == m.attackDown) // 只有下劈才刷新
            {
                m.canDoubleJump = true;
                m.canDash = true;
            }

            // 更新exp
            m.curExp = Mathf.Min(m.curExp + m.expAmountExtractedByAttack, m.maxExp);
            PlayerExpUI.instance.UpdatePlayerExpUI();
        }
    }

    private void Pushed()
    {
        if (currentAttack == m.attackRight)
            m.rigidbody.velocity = new Vector2(-attackDirection[currentAttack].x * m.attackForce,
                m.rigidbody.velocity.y);
        else if (currentAttack == m.attackUp)
        {
        }
        else
        {
            m.rigidbody.velocity = new Vector2(m.rigidbody.velocity.x,
                -attackDirection[currentAttack].y * (m.attackForce * m.attackDownForceMultiplier));
            m.dashCoolDownExpireTime = -1; // 下劈立即刷新dash，怪不得下劈完冲不出去，我还以为是动画播的太慢了
        }
    }
}

public class PlayerRecover : IState
{
    private PlayerFSM m;
    private AnimatorStateInfo info;

    public PlayerRecover(PlayerFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.recoverProcessAnim.speed = m.recoverSpeed;
        m.recoverBurstAnim.speed = m.recoverSpeed;
        m.recoverProcessAnim.Play("RecoverProcess");
        m.rigidbody.velocity = Vector2.zero;
    }


    public void OnUpdate()
    {
        info = m.recoverProcessAnim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName("RecoverProcess") && info.normalizedTime <= 0.9f)
        {
            if (Input.GetKeyUp(m.spellKey))
            {
                m.recoverProcessAnim.Play("Empty");
                m.TransitionState(StateType.Idle);
            }

            return;
        }

        m.recoverBurstAnim.Play("RecoverBurst");
        m.curExp -= 3;
        PlayerExpUI.instance.UpdatePlayerExpUI();

        m.healthPoint = Mathf.Min(m.healthPoint + 1, m.maxHealthPoint);
        HealthUI.instance.UpdateUI();
        m.TransitionState(StateType.Idle);
    }

    public void OnFixedUpdate()
    {
    }

    public void OnExit()
    {
    }

    public void UpdateTrigger()
    {
    }
}

public class PlayerSuperDash : IState
{
    private PlayerFSM m;
    private float gravityBackup;
    private float readyElapse;
    private bool isReady;
    private bool isDashing;
    private bool isOver;
    private bool isShowingSpike;
    private float superDashWindupTime;

    Dictionary<Vector3, Vector3> dir2Angle = new()
    {
        { Vector3.up, new Vector3(0, 0, 0) }, { Vector3.right, new Vector3(0, 0, -90) },
        { Vector3.left, new Vector3(0, 0, 90) }
    };

    // 确定每个方向对应的旋转，因为只有刺向上的图片
    Dictionary<Vector3, Vector3> dir2StartPosOffset = new()
    {
        { Vector3.up, new Vector3(0, -0.5f, 0) }, { Vector3.right, new Vector3(-0.5f, 0, 0) },
        { Vector3.left, new Vector3(0.5f, 0, 90) }
    };

    private List<GameObject> spikes = new();

    public PlayerSuperDash(PlayerFSM m)
    {
        this.m = m;
        superDashWindupTime = (m.oneSideSpikeCount - 1) * m.spikePopUpTimeGap + m.spikePopUpTime;
    }

    public void OnEnter()
    {
        // Debug.Log("SuperDashEnter!");
        readyElapse = 0;
        isReady = false;
        isDashing = false;
        isOver = false;
        isShowingSpike = false;
        gravityBackup = m.rigidbody.gravityScale;
        m.rigidbody.gravityScale = 0;
    }


    public void OnUpdate()
    {
        readyElapse += Time.deltaTime;
        // if (readyElapse > m.superDashWindupTime && readyElapse - Time.deltaTime < m.superDashWindupTime)
        //     Debug.Log("Ready");
        isReady = readyElapse >= superDashWindupTime;
        if (!isReady)
        {
            m.rigidbody.velocity = Vector2.zero;

            if (Input.GetKeyUp(m.superDashKey))
            {
                m.TransitionState(m.preStateType);
            }

            if (!isShowingSpike)
                m.StartCoroutine(TryShowSpike());
        }
        else if (!isDashing)
        {
            if (Input.GetKeyUp(m.superDashKey))
            {
                m.rigidbody.velocity = new Vector2(m.facingDirection.x * m.superDashSpeed, 0);
                spikes.ForEach(Object.Destroy);
                isDashing = true;
            }
        }
        else if (isDashing)
        {
            if (Input.GetKeyUp(m.jumpKey) || Input.GetKeyUp(m.dashKey) || Input.GetKeyUp(m.superDashKey) ||
                m.isHittingWall)
            {
                isOver = true;
            }
        }
    }

    private IEnumerator TryShowSpike() // 根据已有spike数和对应elapse判断是否生成以及位置
    {
        isShowingSpike = true;

        // 确定生成的刺面对的方向
        var spikeFacingDir = Vector3.up;
        var placingDir = Vector3.right;
        if (m.preStateType == StateType.WallSlide)
        {
            spikeFacingDir = m.facingDirection.x == 1 ? Vector3.right : Vector3.left;
            placingDir = Vector3.up;
        }

        bool right = true;
        bool left = true;
        for (int i = 0; i < m.oneSideSpikeCount; i++)
        {
            if (right)
                right = SpawnSpike(spikeFacingDir, i, placingDir, dir2Angle[spikeFacingDir]);
            if (left)
                left = SpawnSpike(spikeFacingDir, i, -placingDir, dir2Angle[spikeFacingDir]);
            yield return new WaitForSeconds(m.spikePopUpTimeGap);
        }
    }

    private bool SpawnSpike(Vector3 spikeFacingDir, int order, Vector3 placingDir, Vector3 eulerAngle)
    {
        bool retval = true;
        // 做完这些，刺就在正确的位置上了
        var pos = m.transform.position + dir2StartPosOffset[spikeFacingDir] + placingDir * (order * m.spikeSpacing);
        var spike = Object.Instantiate(m.spikePrefab, pos, Quaternion.identity);
        spikes.Add(spike.gameObject);
        var (width, height) = (spike.bounds.size.x, spike.bounds.size.y);
        var (w, h) = (width / 2, height / 2);
        spike.transform.Rotate(eulerAngle);
        // 如果这根刺露出来了, 就要把这根刺调整到合适的位置，并且让后续不要再生成刺了
        if (!Physics2D.Raycast(spike.transform.position + placingDir * w, -spikeFacingDir, 0.1f, m.groundLayer))
        {
            while (!Physics2D.Raycast(spike.transform.position + placingDir * w, -spikeFacingDir, 0.1f, m.groundLayer))
            {
                spike.transform.position -= placingDir * 0.001f;
                Physics2D.SyncTransforms();
            }

            retval = false;
        }

        // 调整缩放
        var scale = m.spikeMinScaleY + (m.spikeMaxScaleY - m.spikeMinScaleY) * order / m.oneSideSpikeCount;
        spike.transform.localScale = new Vector3(1, scale, 1);

        // 动画
        spike.transform.position -= spikeFacingDir * (height * scale);
        spike.transform.DOMove(spike.transform.position + spikeFacingDir * (height * scale), m.spikePopUpTime);

        return retval;
    }

    public void OnFixedUpdate()
    {
        if (isOver)
        {
            var newX = Mathf.MoveTowards(m.rigidbody.velocity.x, 0, m.superDashMoveChangeSpeed);
            m.rigidbody.velocity = new Vector2(newX, m.rigidbody.velocity.y);
            if (Mathf.Abs(newX) < 1e-3)
                m.TransitionState(m.preStateType);
        }
    }

    public void OnExit()
    {
        spikes.ForEach((spike) => m.StopAllCoroutines());
        spikes.ForEach(Object.Destroy); // 可能还没冲刺就被打断了
        m.rigidbody.gravityScale = gravityBackup;
    }
}