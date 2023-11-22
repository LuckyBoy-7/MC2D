using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

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


    [Header("RaycastCheck")] public Collider2D hitBoxCollider;
    public float boxLeftRightCastDist;
    public float boxDownCastDist;
    public LayerMask groundLayer;

    [Header("Key")] public KeyCode leftKey;
    public KeyCode rightKey = KeyCode.L;
    public KeyCode upKey = KeyCode.I;
    public KeyCode downKey = KeyCode.K;
    public KeyCode jumpKey = KeyCode.D;
    public KeyCode dashKey = KeyCode.S;
    public KeyCode attackKey = KeyCode.A;
    public KeyCode spellKey = KeyCode.Q;
}

public class PlayerFSM : FSM
{
    public PlayerParameter p;

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
        Debug.Log($"isOnGround: {isOnGround}");
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
    }

    public bool jumpTrigger => Time.time <= p.jumpBufferExpireTime;
    public bool dashTrigger => Time.time <= p.dashBufferExpireTime;
    public bool moveTrigger => Input.GetKey(p.leftKey) || Input.GetKey(p.rightKey);
    public bool fallTrigger => p.rigidbody.velocity.y < -1e-5;
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

    public PlayerWallJump(PlayerParameter p, PlayerFSM manager)
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

    }

    public void OnExit()
    {
    }
}

public class PlayerWallSlide : IState
{
    private PlayerParameter p;
    private PlayerFSM manager;

    public PlayerWallSlide(PlayerParameter p, PlayerFSM manager)
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
    }

    public void OnExit()
    {
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
        p.rigidbody.velocity = new Vector2(p.rigidbody.velocity.x, 0);
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
    }

    public void OnExit()
    {
        isKeyReleased = false;
    }
}