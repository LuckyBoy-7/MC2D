using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

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
    public int facingDirection; // x方向
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


    public void Attacked(int damage, Vector2 attackForceVec = default) // 被击打的方向加力度
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


    public void ReverseFacingDirection() => facingDirection *= -1;

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
}

public class GroundEnemyFSM : EnemyFSM
{
    [Header("PhysicsCheck")] protected float cliffCheckDownRaycastDist = 0.3f;
    protected float boxLeftRightCastDist = 0.1f;
    public LayerMask groundLayer;
    [Header("Fall")] public float maxFallingSpeed;

    protected override void Update()
    {
        if (fallTrigger)
            TransitionState(StateType.Fall);
        base.Update();
    }

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
}