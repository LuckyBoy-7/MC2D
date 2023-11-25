using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class PlayerParameter
{
    [Header("Movement")] public Rigidbody2D rigidbody;

    public Vector2 facingDirection;
    public Vector2 keyDownDirection;
    public float moveSpeed;
    [Header("Idle")] public float dampingSpeed;
    [Header("Jump")] public bool canDoubleJump = true;
    public float firstJumpForce;
    public float firstJumpAdditionalForce;
    public float doubleJumpForce;
    public float doubleJumpAdditionalForce;
    public float jumpBufferTime;
    public float jumpBufferExpireTime = -1;
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
    public float moveChangeSpeed;


    [Header("RaycastCheck")] public Collider2D hitBoxCollider;
    public float boxLeftRightCastDist;
    public float boxDownCastDist;
    public LayerMask groundLayer;

    [Header("Attack")] public Animator attackRight;
    public Animator attackDown;
    public Animator attackUp;
    public float attackCoolDown;
    public float attackCoolDownExpireTime;

    [Header("Key")] public KeyCode leftKey;
    public KeyCode rightKey = KeyCode.L;
    public KeyCode upKey = KeyCode.I;
    public KeyCode downKey = KeyCode.K;
    public KeyCode jumpKey = KeyCode.D;
    public KeyCode dashKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.A;
    public KeyCode spellKey = KeyCode.Q;
}

public class PlayerFSM : SingletonFSM
{
    public PlayerParameter p;
    public static Vector3 position => GameObject.FindWithTag("Player").transform.position;

    void Start()
    {
        states[StateType.Idle] = new PlayerIdle(p, this);
        states[StateType.Run] = new PlayerRun(p, this);
        states[StateType.Hurt] = new PlayerHurt(p, this);
        states[StateType.Jump] = new PlayerJump(p, this);
        states[StateType.WallJump] = new PlayerWallJump(p, this);
        states[StateType.WallSlide] = new PlayerWallSlide(p, this);
        states[StateType.Fall] = new PlayerFall(p, this);
        states[StateType.Dash] = new PlayerDash(p, this);
        states[StateType.ReleaseArrow] = new PlayerReleaseArrow(p, this);
        states[StateType.DoubleJump] = new PlayerDoubleJump(p, this);
        states[StateType.Attack] = new PlayerAttack(p, this);
        TransitionState(StateType.Idle);
    }

    private void Update()
    {
        UpdateKeyDownDirection();
        if (Input.GetKeyDown(p.jumpKey))
            p.jumpBufferExpireTime = Time.time + p.jumpBufferTime;
        if (Input.GetKeyDown(p.dashKey))
            p.dashBufferExpireTime = Time.time + p.dashBufferTime;
        if (isOnGround)
        {
            p.canDoubleJump = true;
            p.canDash = true;
        }

        currentState.OnUpdate();


        Debug.Log($"currentState: {currentState}");
        // Debug.Log($"isOnGround: {isOnGround}");
        // Debug.Log($"isOnLeftWall: {isOnLeftWall}");
        // Debug.Log($"isOnRightWall: {isOnRightWall}");
    }

    public bool isOnGround =>
        Physics2D.OverlapBox(transform.position + Vector3.down * (0.5f + p.boxDownCastDist / 2),
            new Vector2(0.95f, p.boxDownCastDist), 0, p.groundLayer);

    public bool isOnRightWall =>
        Physics2D.OverlapBox(transform.position + Vector3.right * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 0.95f), 0, p.groundLayer);

    public bool isOnLeftWall =>
        Physics2D.OverlapBox(transform.position + Vector3.left * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 0.95f), 0, p.groundLayer);

    public bool isOnWall => isOnLeftWall || isOnRightWall;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        // Ground Box
        Gizmos.DrawWireCube(transform.position + Vector3.down * (0.5f + p.boxDownCastDist / 2),
            new Vector2(1, p.boxDownCastDist));
        // Wall Box
        Gizmos.DrawWireCube(transform.position + Vector3.right * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 1));
        Gizmos.DrawWireCube(transform.position + Vector3.left * (0.5f + p.boxLeftRightCastDist / 2),
            new Vector2(p.boxLeftRightCastDist, 1));
        // Hit Box
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, p.hitBoxCollider.bounds.size);
    }

    public void UpdateKeyDownDirection()
    {
        var x = Input.GetKey(p.leftKey) ? -1 : Input.GetKey(p.rightKey) ? 1 : 0;
        var y = Input.GetKey(p.downKey) ? -1 : Input.GetKey(p.upKey) ? 1 : 0;
        p.keyDownDirection = new Vector2(x, y);
    }

    public void UpdateFacingDirection() // 看起来像是没什么用，但是如果我放黑波的时候改变了方向，下次就算没动键盘放出来的波会向相反方向移动
    {
        var (x, y) = (p.keyDownDirection.x, p.keyDownDirection.y);
        if (x != 0)
            p.facingDirection.x = x;
        if (y != 0)
            p.facingDirection.y = y;
        transform.localScale = new Vector3(p.facingDirection.x, 1, 1);
    }

    public void ReverseFacingDirection() => p.facingDirection.x *= -1;

    public bool jumpTrigger => Time.time <= p.jumpBufferExpireTime;
    public bool dashTrigger => Time.time <= p.dashBufferExpireTime && Time.time >= p.dashCoolDownExpireTime;
    public bool moveTrigger => Input.GetKey(p.leftKey) || Input.GetKey(p.rightKey);
    public bool fallTrigger => p.rigidbody.velocity.y < -1e-5;

    public bool wallSlideTrigger =>
        (isOnLeftWall && Input.GetKey(p.leftKey) && p.rigidbody.velocity.x <= 1e-5 // 不然蹬墙跳一出去就就又变成wallSlide状态了
         || isOnRightWall && Input.GetKey(p.rightKey) && p.rigidbody.velocity.x >= -1e-5);


    public bool offWallTrigger => isOnLeftWall && Input.GetKey(p.rightKey) || isOnRightWall && Input.GetKey(p.leftKey);
    public bool attackTrigger => Input.GetKeyDown(p.attackKey) && Time.time >= p.attackCoolDownExpireTime;
}

public class PlayerIdle : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;

    public PlayerIdle(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        manager.UpdateFacingDirection();
        var newX = Mathf.MoveTowards(p.rigidbody.velocity.x, 0, p.dampingSpeed);
        p.rigidbody.velocity = new Vector2(newX, p.rigidbody.velocity.y);

        if (manager.moveTrigger)
            manager.TransitionState(StateType.Run);
        else if (manager.jumpTrigger && manager.isOnGround)
            manager.TransitionState(StateType.Jump);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
        else if (manager.attackTrigger)
            manager.TransitionState(StateType.Attack);
    }

    public void OnExit()
    {
    }
}

public class PlayerRun : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;

    public PlayerRun(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        manager.UpdateFacingDirection();
        p.rigidbody.velocity = new Vector2(p.moveSpeed * p.keyDownDirection.x, p.rigidbody.velocity.y);
        if (!manager.moveTrigger)
            manager.TransitionState(StateType.Idle);
        else if (manager.jumpTrigger && manager.isOnGround)
            manager.TransitionState(StateType.Jump);
        else if (manager.fallTrigger)
            manager.TransitionState(StateType.Fall);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
        else if (manager.attackTrigger)
            manager.TransitionState(StateType.Attack);
    }

    public void OnExit()
    {
    }
}

public class PlayerHurt : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;

    public PlayerHurt(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}

public class PlayerJump : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;
    private bool isKeyReleased;

    public PlayerJump(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        p.rigidbody.AddForce(Vector2.up * p.firstJumpForce, ForceMode2D.Impulse);
        p.jumpBufferExpireTime = -1; // 重置，否则如果从跳跃到落地的时间<bufferTime，则跳跃会被触发两次 
    }

    public void OnUpdate()
    {
        manager.UpdateFacingDirection();
        isKeyReleased = isKeyReleased || Input.GetKeyUp(p.jumpKey);
        if (!isKeyReleased)
            p.rigidbody.AddForce(p.firstJumpAdditionalForce * Vector2.up);

        p.rigidbody.velocity = new Vector2(p.moveSpeed * p.keyDownDirection.x, p.rigidbody.velocity.y);
        if (manager.fallTrigger)
            manager.TransitionState(StateType.Fall);
        else if (manager.jumpTrigger && p.canDoubleJump)
            manager.TransitionState(StateType.DoubleJump);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
        else if (manager.attackTrigger)
            manager.TransitionState(StateType.Attack);
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}

public class PlayerWallJump : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;
    private bool isKeyReleased;

    public PlayerWallJump(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        p.rigidbody.velocity = new Vector2(p.facingDirection.x * p.wallJumpForce.x, p.wallJumpForce.y);
        p.jumpBufferExpireTime = -1; // 重置，否则如果从跳跃到落地的时间<bufferTime，则跳跃会被触发两次 
    }


    public void OnUpdate()
    {
        manager.UpdateFacingDirection();
        isKeyReleased = isKeyReleased || Input.GetKeyUp(p.jumpKey);
        float newX = Mathf.MoveTowards(p.rigidbody.velocity.x, p.moveSpeed * p.keyDownDirection.x, p.moveChangeSpeed);
        p.rigidbody.velocity = new Vector2(newX, p.rigidbody.velocity.y);
        if (!isKeyReleased)
            p.rigidbody.AddForce(p.wallJumpAdditionalForce * Vector2.up);

        if (manager.fallTrigger)
            manager.TransitionState(StateType.Fall);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
        else if (manager.jumpTrigger && p.canDoubleJump)
            manager.TransitionState(StateType.DoubleJump);
        else if (manager.attackTrigger)
            manager.TransitionState(StateType.Attack);
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}

public class PlayerWallSlide : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;
    private float gravityScaleBackup;

    public PlayerWallSlide(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        gravityScaleBackup = p.rigidbody.gravityScale;
        p.rigidbody.gravityScale = 0;
        p.rigidbody.velocity = new Vector2(0, -p.wallSlideSpeed);
        p.canDash = true;
        p.canDoubleJump = true;
        manager.ReverseFacingDirection();
    }

    public void OnUpdate()
    {
        if (manager.isOnGround)
            manager.TransitionState(manager.moveTrigger ? StateType.Run : StateType.Idle);
        else if (manager.jumpTrigger)
            manager.TransitionState(StateType.WallJump);
        else if (manager.offWallTrigger || !manager.isOnWall) // 手动出去或者被动出去
            manager.TransitionState(StateType.Fall);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
    }

    public void OnExit()
    {
        p.rigidbody.gravityScale = gravityScaleBackup;
    }
}

public class PlayerFall : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;

    public PlayerFall(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        manager.UpdateFacingDirection();
        var newY = -Mathf.Min(-p.rigidbody.velocity.y, p.maxFallingSpeed);
        p.rigidbody.velocity = new Vector2(p.moveSpeed * p.keyDownDirection.x, newY);
        if (manager.isOnGround)
            manager.TransitionState(manager.moveTrigger ? StateType.Run : StateType.Idle);
        else if (manager.jumpTrigger && p.canDoubleJump)
            manager.TransitionState(StateType.DoubleJump);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
        else if (manager.wallSlideTrigger)
            manager.TransitionState(StateType.WallSlide);
        else if (manager.attackTrigger)
            manager.TransitionState(StateType.Attack);
    }

    public void OnExit()
    {
    }
}

public class PlayerDash : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;
    private float gravityScaleBackup;
    private float elapse;

    public PlayerDash(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        p.dashBufferExpireTime = -1;
        p.canDash = false;
        p.rigidbody.velocity = new Vector2(p.facingDirection.x * p.dashSpeed, 0);
        gravityScaleBackup = p.rigidbody.gravityScale;
        p.rigidbody.gravityScale = 0;
    }

    public void OnUpdate()
    {
        elapse += Time.deltaTime;
        if (elapse <= p.dashDuration)
            return;

        if (manager.jumpTrigger)
            manager.TransitionState(manager.isOnGround ? StateType.Jump : StateType.DoubleJump);
        else if (manager.isOnGround)
            manager.TransitionState(manager.moveTrigger ? StateType.Run : StateType.Idle);
        else if (!manager.isOnGround) // 这里的处理要特殊一点，因为player冲刺状态y轴速度恒为0，不这么判断状态就出不去了
            manager.TransitionState(StateType.Fall);
    }

    public void OnExit()
    {
        p.rigidbody.gravityScale = gravityScaleBackup;
        elapse = 0;
        p.dashCoolDownExpireTime = Time.time + p.dashCoolDown;
    }
}

public class PlayerReleaseArrow : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;

    public PlayerReleaseArrow(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
    }
}

public class PlayerDoubleJump : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;
    private bool isKeyReleased;

    public PlayerDoubleJump(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        p.rigidbody.velocity = new Vector2(p.rigidbody.velocity.x, p.doubleJumpForce);
        // p.rigidbody.AddForce(Vector2.up * p.doubleJumpForce, ForceMode2D.Impulse);
        p.jumpBufferExpireTime = -1; // 重置，否则如果从跳跃到落地的时间<bufferTime，则跳跃会被触发两次 
        p.canDoubleJump = false;
    }


    public void OnUpdate()
    {
        manager.UpdateFacingDirection();
        isKeyReleased = isKeyReleased || Input.GetKeyUp(p.jumpKey);
        if (!isKeyReleased)
            p.rigidbody.AddForce(p.doubleJumpAdditionalForce * Vector2.up);

        p.rigidbody.velocity = new Vector2(p.moveSpeed * p.keyDownDirection.x, p.rigidbody.velocity.y);
        if (manager.fallTrigger)
            manager.TransitionState(StateType.Fall);
        else if (manager.dashTrigger && p.canDash)
            manager.TransitionState(StateType.Dash);
        else if (manager.attackTrigger)
            manager.TransitionState(StateType.Attack);
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}

public class PlayerAttack : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;
    private AnimatorStateInfo info;

    public PlayerAttack(PlayerParameter p, PlayerFSM manager)
    {
        this.p = p;
        this.manager = manager;
    }

    public void OnEnter()
    {
        List<int> direction = new() { -1, 1 };
        Animator animator;
        if (Input.GetKey(p.upKey))
            animator = p.attackUp;
        else if (Input.GetKey(p.downKey))
            animator = p.attackDown;
        else
            animator = p.attackRight;
        animator.transform.localScale = new Vector3(1, direction[Random.Range(0, 2)], 1);
        animator.Play("PlayerAttack");
        info = animator.GetCurrentAnimatorStateInfo(0);
    }


    public void OnUpdate()
    {
        // 我感觉如果要写的更精细的话就要有更多的状态，比如FallAttack，RunAttack之类的
        if (manager.fallTrigger)
        {
            var newY = -Mathf.Min(-p.rigidbody.velocity.y, p.maxFallingSpeed);
            p.rigidbody.velocity = new Vector2(p.moveSpeed * p.keyDownDirection.x, newY);
        }
        else
            p.rigidbody.velocity = new Vector2(p.moveSpeed * p.keyDownDirection.x, p.rigidbody.velocity.y);

        if (info.IsName("PlayerAttack") && info.normalizedTime < 0.9f)
            return;
        if (manager.isOnGround)
            manager.TransitionState(manager.moveTrigger ? StateType.Run : StateType.Idle);
        else if (manager.fallTrigger)
            manager.TransitionState(StateType.Fall);
    }

    public void OnExit()
    {
        p.attackCoolDownExpireTime = Time.time + p.attackCoolDown;
    }
}