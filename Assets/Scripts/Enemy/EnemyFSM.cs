using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemyFSM : FSM
{
    public bool canBeLooted = true;
    public bool canBeKnockedBack = true;
    [Header("Currency")] public EmeraldEmitter emeraldEmitterPrefab;
    public int ownEmeraldNumber;

    [Header("Health")] public int maxHealthPoint = 5;
    public int healthPoint = 5;
    public SpriteRenderer hurtMask;

    [Header("PhysicsCheck")] public float cliffCheckDownRaycastDist = 0.3f;
    public float boxLeftRightCastDist = 0.1f;
    public LayerMask groundLayer;
    public BoxCollider2D hitBoxCollider;

    [Header("Movement")] public Rigidbody2D rigidbody;
    public int facingDirection; // x方向
    public float xVelocityChangeSpeed = 100; // 如果敌人在移动的时候被击飞，此时的移动速度需要lerp
    public float yVelocityChangeSpeed; // 如果敌人在移动的时候被击飞，此时的移动速度需要lerp

    [Header("Hurt")] public float showHurtEffectTime = 0.3F;
    public float roarHurtElapse;
    public float dropHurtElapse;


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
            rigidbody.velocity = attackForceVec;
    }

    private void Kill()
    {
        if (canBeLooted)
            Instantiate(emeraldEmitterPrefab, transform.position, Quaternion.identity).num = ownEmeraldNumber;
        Destroy(gameObject);
    }


    #region PhysicsCheck

    private bool isOverLeftCliff
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomLeft = bottomCenter - Vector3.right * 0.5f;


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
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomRight = bottomCenter + Vector3.right * 0.5f;


            bool overRightCliff = !Physics2D.Linecast(bottomRight,
                bottomRight + Vector3.down * cliffCheckDownRaycastDist,
                groundLayer);
            return overRightCliff;
        }
    }

    public bool isWalkingDownCliff =>
        isOverLeftCliff && facingDirection == -1 || isOverRightCliff && facingDirection == 1;

    private bool isOverLeftGround // 就是到悬崖边上后检测下面是否是空地
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomLeft = bottomCenter - Vector3.right * 0.5f;


            bool overLeftGround = Physics2D.Raycast(bottomLeft, Vector2.down, 6, groundLayer);
            return overLeftGround;
        }
    }

    private bool isOverRightGround
    {
        get
        {
            Vector3 bottomCenter = transform.position + Vector3.down * 0.5f;
            Vector3 bottomRight = bottomCenter + Vector3.right * 0.5f;


            bool overRightGround = Physics2D.Raycast(bottomRight, Vector2.down, 6, groundLayer);
            return overRightGround;
        }
    }

    public bool isOverGroundAboveCliff => isOverLeftCliff && isOverLeftGround || isOverRightCliff && isOverRightGround;

    private bool isOnLeftWall =>
        Physics2D.OverlapBox(transform.position + Vector3.left * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 0.95f), 0, groundLayer);

    private bool isOnRightWall =>
        Physics2D.OverlapBox(transform.position + Vector3.right * (0.5f + boxLeftRightCastDist / 2),
            new Vector2(boxLeftRightCastDist, 0.95f), 0, groundLayer);

    public bool isHittingWall => isOnLeftWall && facingDirection == -1 || isOnRightWall && facingDirection == 1;

    #endregion

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
    }
}