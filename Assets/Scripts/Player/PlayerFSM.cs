using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerFSM : SingletonFSM<PlayerFSM>
{
    public SpriteRenderer spriteRenderer;
    [Header("Debug")] public bool isDebug;
    [Header("Health")] public int maxHealthPoint;
    public int healthPoint;
    public BoxCollider2D triggerBox;

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

    [Header("Effect")] public SwordAttackEffect attackEffect;

    [Header("ReleaseArrow")] public PlayerArrow arrowPrefab;
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

    [Header("Key")] public KeyCode leftKey = KeyCode.J;
    public KeyCode rightKey = KeyCode.L;
    public KeyCode upKey = KeyCode.I;
    public KeyCode downKey = KeyCode.K;
    public KeyCode jumpKey = KeyCode.D;
    public KeyCode dashKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.A;
    public KeyCode spellKey = KeyCode.Q;

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
        TransitionState(StateType.Idle);
    }

    private void Update()
    {
        UpdateKeyDownDirection();
        UpdateInputBuffer();
        UpdateStates();
        UpdateTriggerBoxOverlap();
        TryUpdateDebug();

        currentState.OnUpdate();


        // Debug.Log($"currentState: {currentState}");
        // Debug.Log($"isOnGround: {isOnGround}");
        // Debug.Log($"isOnLeftWall: {isOnLeftWall}");
        // Debug.Log($"isOnRightWall: {isOnRightWall}");
    }

    private void TryUpdateDebug()
    {
        if (!isDebug)
            return;
        curExp = maxExp;
        PlayerExpUI.instance.UpdatePlayerExpUI();
    }

    private void UpdateTriggerBoxOverlap()
    {
        if (isInvincible)
            return;
        // 这里是triggerBox的碰撞伤害的检测
        List<Collider2D> res = new();
        triggerBox.OverlapCollider(new ContactFilter2D { useTriggers = true }, res);
        foreach (var collider in res)
        {
            if (!collider.CompareTag("Enemy"))
                continue;
            TakeDamage(1);
            hurtDirection = collider.transform.position.x > transform.position.x
                ? new Vector2(-hurtDirectionXMultiplier, 1)
                : new Vector2(hurtDirectionXMultiplier, 1);
            TransitionState(StateType.Hurt);
            return;
        }
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
        if (isOnGround)
        {
            wolfJumpBufferExpireTime = Time.time + wolfJumpBufferTime;
            wallWolfJumpBufferExpireTime = -1; // 如果在墙上滑倒底但是不跳。其实应该可以不加，因为毕竟0.1s很短
        }

        if (currentState == states[StateType.WallSlide])
            wallWolfJumpBufferExpireTime = Time.time + wallWolfJumpBufferTime;
    }

    #region PhysicsCheck

    public bool isOnGround =>
        Physics2D.OverlapBox(transform.position + Vector3.down * (0.5f + boxDownCastDist / 2),
            new Vector2(0.95f, boxDownCastDist), 0, groundLayer);

    public bool isOnRightWall =>
        Physics2D.OverlapBox(transform.position + Vector3.right * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 0.95f), 0, groundLayer);

    public bool isOnLeftWall =>
        Physics2D.OverlapBox(transform.position + Vector3.left * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 0.95f), 0, groundLayer);

    public bool isOnWall => isOnLeftWall || isOnRightWall;

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Ground Box
        Gizmos.DrawWireCube(transform.position + Vector3.down * (0.5f + boxDownCastDist / 2),
            new Vector2(1, boxDownCastDist));
        // Wall Box
        Gizmos.DrawWireCube(transform.position + Vector3.right * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 1));
        Gizmos.DrawWireCube(transform.position + Vector3.left * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 1));
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
        HealthUI.instance.UpdateUI(healthPoint);
        spriteRenderer.color = Color.red;
        spriteRenderer.DOColor(Color.white, showHurtEffectTime);
        TransitionState(StateType.Hurt);
        if (healthPoint == 0)
            Kill();
    }

    private void Kill()
    {
        Destroy(gameObject);
    }

    public bool isInvincible => Time.time <= invincibleExpireTime;

    #region StateTransitionTrigger

    public bool wallSlideTrigger =>
        (isOnLeftWall && Input.GetKey(leftKey) && rigidbody.velocity.x <= 1e-5 // 不然蹬墙跳一出去就就又变成wallSlide状态了
         || isOnRightWall && Input.GetKey(rightKey) && rigidbody.velocity.x >= -1e-5);

    public bool jumpTrigger => // 就是尝试跳跃后，如果在地上或不在地上但是狼跳还在
        Time.time <= jumpBufferExpireTime && (isOnGround || Time.time <= wolfJumpBufferExpireTime);

    public bool wallJumpTrigger => // 就是尝试跳跃后，如果在墙上或不在墙上但是狼跳还在
        Time.time <= jumpBufferExpireTime && Time.time <= wallWolfJumpBufferExpireTime;

    public bool doubleJumpTrigger => Time.time <= jumpBufferExpireTime && canDoubleJump;

    public bool dashTrigger =>
        Time.time <= dashBufferExpireTime && Time.time >= dashCoolDownExpireTime && canDash;

    public bool moveTrigger => Input.GetKey(leftKey) || Input.GetKey(rightKey);
    public bool fallTrigger => rigidbody.velocity.y < -1e-3;
    public bool offWallTrigger => isOnLeftWall && Input.GetKey(rightKey) || isOnRightWall && Input.GetKey(leftKey);
    public bool attackTrigger => Time.time >= attackCoolDownExpireTime && Time.time <= attackBufferExpireTime;

    public bool spellTrigger => Input.GetKeyDown(spellKey) && curExp >= 3; // 因为放波大部分时候是不需要预输入的

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

        if (Input.GetKey(m.downKey))
            m.TransitionState(StateType.Drop);
        else if (Input.GetKey(m.upKey))
            m.TransitionState(StateType.Roar);
        else
            m.TransitionState(StateType.ReleaseArrow);
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

/// <summary>
/// 既然本来表现力就不佳了，那干脆就不造成伤害，单纯作为躲避和打开隐藏通路
/// </summary>
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
        m.rigidbody.velocity = Vector2.down * m.dropSpeed;

        if (m.isOnGround)
        {
            isFirstOnGround = true;
            dropPrefab = PlayerFSM.Instantiate(m.dropPrefab, m.transform);
            dropPrefab.transform.position += Vector3.down * 0.5f;
            m.invincibleExpireTime = Time.time + m.dropInvincibleTime;
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

        if (Mathf.Abs(m.rigidbody.velocity.x) > 1e-5) // 后摇还没结束
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
    private bool hasCausedDamage;

    public PlayerAttack(PlayerFSM m)
    {
        this.m = m;
        attackDirection = new()
        {
            { m.attackRight, new Vector2(1, 0) * m.facingDirection.x },
            { m.attackUp, new Vector2(0, 0) },
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

        // The integer part is the number of time a state has been looped. The fractional part is the % (0-1) of progress in the current loop.
        // 整数部分是循环次数，小数部分是运行进度, 主要是为了防止攻击的时候转向
        if (info.IsName("PlayerAttack") && info.normalizedTime - (int)info.normalizedTime < 0.95f)
            return;
        m.UpdateFacingDirection();
        if (m.isOnGround)
            m.TransitionState(m.moveTrigger ? StateType.Run : StateType.Idle);
        else if (m.fallTrigger)
            m.TransitionState(StateType.Fall);
        else if (m.dashTrigger)
            m.TransitionState(StateType.Dash);
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
        hasCausedDamage = false;
    }

    public void UpdateTrigger()
    {
        if (hasCausedDamage)
            return;
        List<Collider2D> triggeredEnemyBoxes = GetTriggeredEnemyBox();
        foreach (var collider in triggeredEnemyBoxes)
            collider.GetComponent<EnemyFSM>().Attacked(m.attackDamage, attackDirection[currentAttack] * m.attackForce);

        bool isEnemyAttacked = triggeredEnemyBoxes.Count > 0; // 击打多个敌人时，只受到一个单位的后坐力
        if (isEnemyAttacked)
        {
            hasCausedDamage = true;
            if (currentAttack == m.attackRight)
                m.rigidbody.velocity = new Vector2(-attackDirection[currentAttack].x * m.attackForce,
                    m.rigidbody.velocity.y);
            else
                m.rigidbody.velocity =
                    -attackDirection[currentAttack] * m.attackForce;
            // 更新属性
            if (currentAttack == m.attackDown) // 只有下劈才刷新
            {
                m.canDoubleJump = true;
                m.canDash = true;
            }

            // 播放特效
            Collider2D choiceBox = triggeredEnemyBoxes[Random.Range(0, triggeredEnemyBoxes.Count)];
            m.attackEffect.transform.position = choiceBox.bounds.ClosestPoint(currentAttack.transform.position);
            m.attackEffect.Play();

            // 更新exp
            m.curExp = Mathf.Min(m.curExp + m.expAmountExtractedByAttack, m.maxExp);
            PlayerExpUI.instance.UpdatePlayerExpUI();
        }
    }

    /// <summary>
    /// 获得所有跟剑碰撞的trigger的，tag为enemy的box。
    /// </summary>
    /// <returns></returns>
    private List<Collider2D> GetTriggeredEnemyBox()
    {
        // 可能是因为yield return什么数据类型都有，所以有装箱和拆箱的过程
        List<Collider2D> ans = new();
        List<Collider2D> res = new();
        Physics2D.OverlapCollider(attackCollider[currentAttack], new ContactFilter2D { useTriggers = true }, res);
        foreach (var collider in res)
        {
            if (!collider.CompareTag("Enemy") || !collider.isTrigger)
                continue;
            ans.Add(collider);
        }

        return ans;
    }
}