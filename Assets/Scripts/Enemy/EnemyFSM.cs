using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyFSM : FSM
{
    public bool canBeLooted = true;
    public bool canBeKnockedBack = true;
    [Header("Currency")] public EmeraldEmitter emeraldEmitterPrefab;
    public int ownEmeraldNumber;

    [Header("Health")] public int maxHealthPoint = 5;
    public int healthPoint; // 还要调试用的
    public SpriteRenderer hurtMask;


    [Header("Movement")] public Rigidbody2D rigidbody;
    public float xVelocityChangeSpeed = 100; // 如果敌人在移动的时候被击飞，此时的移动速度需要lerp
    public float yVelocityChangeSpeed = 100; // 如果敌人在移动的时候被击飞，此时的移动速度需要lerp

    [Header("Hurt")] public BoxCollider2D hitBoxCollider;
    public float showHurtEffectTime = 0.3F;
    public float roarHurtElapse { get; set; } // 用于记录上吼下砸造成的连续伤害信息
    public float dropHurtElapse { get; set; }
    public float knockedBackForceMultiplier = 1;

    public event Action onKill;

    protected virtual void Start()
    {
        healthPoint = maxHealthPoint;
    }

    protected virtual void Update()
    {
        currentState.OnUpdate();
    }

    protected virtual void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }

    public void TakeDamage(int damage)
    {
        healthPoint -= damage;

        DOTween.Kill("MaskFadeOut"); // 因为两次击打时间可能很接近，所以可能还在淡出enemy就已经死了
        if (healthPoint <= 0) // 伤害可能会溢出
            Kill();
        else
        {
            hurtMask.color = hurtMask.color.WithAlpha(1);
            hurtMask.DOFade(0, showHurtEffectTime).SetId("MaskFadeOut");
        }
    }

    public virtual void Attacked(int damage, Vector2 attackForceVec = default) // 被击打的方向加力度
    {
        TakeDamage(damage);

        if (canBeKnockedBack)
            rigidbody.velocity = attackForceVec * knockedBackForceMultiplier;
    }

    private void Kill()
    {
        onKill?.Invoke();
        if (canBeLooted)
            Instantiate(emeraldEmitterPrefab, transform.position, Quaternion.identity).num = ownEmeraldNumber;
        Destroy(gameObject);
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("PlayerRoar"))
        {
            PlayerFSM player = PlayerFSM.instance;
            roarHurtElapse += Time.deltaTime;
            if (roarHurtElapse >= player.roarDamageTimeGap)
            {
                roarHurtElapse = 0;
                TakeDamage(player.roarDamage);
            }
        }
        else if (other.CompareTag("PlayerDrop"))
        {
            PlayerFSM player = PlayerFSM.instance;
            dropHurtElapse += Time.deltaTime;
            if (dropHurtElapse >= player.dropDamageTimeGap)
            {
                dropHurtElapse = 0;
                TakeDamage(player.dropDamage);
            }
        }

        else if (other.CompareTag("Spike"))
        {
            Attacked(1000000);
        }
    }

    public void LerpVelocityX(float to)
    {
        var newX = Mathf.MoveTowards(rigidbody.velocity.x, to, xVelocityChangeSpeed * Time.fixedDeltaTime);
        rigidbody.velocity = new Vector2(newX, rigidbody.velocity.y);
    }

    public void LerpVelocityY(float to)
    {
        var newY = Mathf.MoveTowards(rigidbody.velocity.y, to, yVelocityChangeSpeed * Time.fixedDeltaTime);
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, newY);
    }

    public void LerpVelocity(Vector2 velocity)
    {
        LerpVelocityX(velocity.x);
        LerpVelocityY(velocity.y);
    }

    public void LerpVelocity(float x, float y)
    {
        LerpVelocityX(x);
        LerpVelocityY(y);
    }
}

public class GroundEnemyFSM : EnemyFSM
{
    [Header("PhysicsCheck")] protected float cliffCheckDownRaycastDist = 0.3f;
    protected float boxLeftRightCastDist = 0.1f;
    public LayerMask groundLayer;
    [Header("Fall")] public float maxFallingSpeed;
    [Header("Movement")] public int facingDirection; // x方向


    protected override void Update()
    {
        base.Update();
        UpdateFacingDirection();
    }

    private void UpdateFacingDirection()
    {
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(scale.x) * facingDirection, scale.y, scale.z);
    }

    public void ReverseFacingDirection() => facingDirection *= -1;

    #region PhysicsCheck

    private bool isOverLeftCliff
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);

            Vector3 bottomCenter = center + Vector3.down * h;
            Vector3 bottomLeft = bottomCenter - Vector3.right * w;


            bool overLeftCliff = !Physics2D.Linecast(bottomLeft,
                bottomLeft + Vector3.down * cliffCheckDownRaycastDist,
                groundLayer);
            return overLeftCliff;
        }
    }

    private bool isOverRightCliff
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);

            Vector3 bottomCenter = center + Vector3.down * h;
            Vector3 bottomRight = bottomCenter + Vector3.right * w;


            bool overRightCliff = !Physics2D.Linecast(bottomRight,
                bottomRight + Vector3.down * cliffCheckDownRaycastDist,
                groundLayer);
            return overRightCliff;
        }
    }

    public bool isWalkingDownCliff => // 因为有可能生成时在空中，但先进入patrol，结果检测到“悬崖”，又转向
        !isOverRightCliff && isOverLeftCliff && facingDirection == -1 ||
        !isOverLeftCliff && isOverRightCliff && facingDirection == 1;

    private bool isOverLeftGround // 就是到悬崖边上后检测下面是否是空地
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);

            Vector3 bottomCenter = center + Vector3.down * h;
            Vector3 bottomLeft = bottomCenter - Vector3.right * w;


            bool overLeftGround = Physics2D.Raycast(bottomLeft, Vector2.down, 6, groundLayer);
            return overLeftGround;
        }
    }

    private bool isOverRightGround
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);

            Vector3 bottomCenter = center + Vector3.down * h;
            Vector3 bottomRight = bottomCenter + Vector3.right * w;


            bool overRightGround = Physics2D.Raycast(bottomRight, Vector2.down, 6, groundLayer);
            return overRightGround;
        }
    }

    public bool isOverGroundAboveCliff => isOverLeftCliff && isOverLeftGround || isOverRightCliff && isOverRightGround;

    private bool isOnLeftWall
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);

            return Physics2D.OverlapBox(center + Vector3.left * (w + boxLeftRightCastDist / 2),
                new Vector2(boxLeftRightCastDist, height * 0.95f), 0, groundLayer);
        }
    }


    private bool isOnRightWall
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);

            return Physics2D.OverlapBox(center + Vector3.right * (w + boxLeftRightCastDist / 2),
                new Vector2(boxLeftRightCastDist, height * 0.95f), 0, groundLayer);
        }
    }

    public bool isHittingWall => isOnLeftWall && facingDirection == -1 || isOnRightWall && facingDirection == 1;

    // 只给那些能被knockedBack的对象调用
    public bool isOnGround
    {
        get
        {
            var center = GetColliderCenter(out var width, out var height, out var w, out var h);
            return Physics2D.OverlapBox(center + Vector3.down * (h + boxLeftRightCastDist / 2),
                new Vector2(width * 0.95f, boxLeftRightCastDist), 0, groundLayer);
        }
    }

    private Vector3 GetColliderCenter(out float width, out float height, out float w, out float h)
    {
        var bounds = hitBoxCollider.bounds;
        (width, height) = (bounds.max.x - bounds.min.x, bounds.max.y - bounds.min.y);
        (w, h) = (width / 2, height / 2);
        return bounds.center;
    }

    #endregion

    #region StatesTransition

    public bool fallTrigger => rigidbody.velocity.y < -1e-3 && canBeKnockedBack;

    #endregion
}

public class FlyEnemyFSM : EnemyFSM
{
    [Header("Detection")] public float playerInDetectionRadius;
    public float playerOutDetectionRadius;
    public LayerMask viewLayer;
    [Header("Attack")] public Action intervalAttackFunc;
    public Action keepAttackFunc;
    public float attackCoolDown;
    [Header("Movement")] public float idleRadius;
    public float keepDistance;
    public Vector2 targetPos;
    public Vector2 pivotPos;
    public float moveSpeed;
    [Header("Status")] public bool isSelfControlMovement = false;

    protected override void Start()
    {
        base.Start();
        states[StateType.Wait] = new FlyEnemyWait(this); // idle
        states[StateType.Attack] = new FlyEnemyAttack(this);
        TransitionState(StateType.Wait);
    }

    protected virtual void Update()
    {
        currentState.OnUpdate();
    }

    private void FixedUpdate()
    {
        currentState.OnFixedUpdate();
    }

    private void OnDrawGizmos()
    {
        if (PlayerFSM.instance != null && (PlayerFSM.instance.transform.position - transform.position).magnitude <=
            playerInDetectionRadius)
        {
            // 如果player在攻击范围内，则画一条线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, PlayerFSM.instance.transform.position);
        }

        // detect player in
        Gizmos.color = Color.green;
        var position = transform.position;
        Gizmos.DrawWireSphere(position, playerInDetectionRadius);

        // detect player out
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position, playerOutDetectionRadius);
        // hit box
        Gizmos.DrawWireCube(position, hitBoxCollider.size);

        // idle box
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pivotPos, idleRadius);
    }
}

public class FlyEnemyWait : IState
{
    private FlyEnemyFSM m;

    public FlyEnemyWait(FlyEnemyFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
    }

    public void OnUpdate()
    {
        var dir = (PlayerFSM.instance.transform.position - m.transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(m.transform.position, dir, m.playerInDetectionRadius, m.viewLayer);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
            m.TransitionState(StateType.Attack);
    }

    public void OnFixedUpdate()
    {
        m.LerpVelocity(0, 0);
    }

    public void OnExit()
    {
    }
}

public class FlyEnemyAttack : IState
{
    private FlyEnemyFSM m;
    private double elapse;

    public FlyEnemyAttack(FlyEnemyFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        m.pivotPos = m.transform.position;
        RollTargetPos();
    }

    public void OnUpdate()
    {
        var dir = (PlayerFSM.instance.transform.position - m.transform.position).normalized;
        // player逃出范围("或被墙挡住" 这点去掉，不然enemy看上去有点呆)
        if ((PlayerFSM.instance.transform.position - m.transform.position).magnitude > m.playerOutDetectionRadius)
        {
            m.TransitionState(StateType.Wait);
            return;
        }

        m.pivotPos = -dir * m.keepDistance + PlayerFSM.instance.transform.position;
        if (m.hitBoxCollider.IsTouchingLayers(1 << LayerMask.NameToLayer("Platform")))
            RollTargetPos();

        m.keepAttackFunc?.Invoke();
        if (elapse >= m.attackCoolDown)
        {
            elapse -= m.attackCoolDown;
            m.intervalAttackFunc?.Invoke();
        }

        elapse += Time.deltaTime;
    }

    private void RollTargetPos()
    {
        m.targetPos = m.pivotPos + Random.insideUnitCircle * m.idleRadius;
    }


    public void OnFixedUpdate()
    {
        if (m.isSelfControlMovement)
            return;
        if (((Vector2)m.transform.position - m.targetPos).magnitude < 0.1f)
            RollTargetPos();
        var targetSpeed = (m.targetPos - (Vector2)m.transform.position).normalized * m.moveSpeed;
        m.LerpVelocity(targetSpeed);
    }

    public void OnExit()
    {
    }
}