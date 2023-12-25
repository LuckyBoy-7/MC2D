using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class PlayerAttack : Singleton<PlayerAttack>
{
    [Header("Attack")] public List<int> attackDamages = new();
    public int attackDamagesIdx;
    public Animator attackRight;
    public Animator attackDown;
    public Animator attackUp;
    public float attackCoolDown;
    public float attackCoolDownExpireTime;
    public float attackForce;
    public float rightAttackForceBackMultiplier;
    public float attackDownForceMultiplier;
    public float attackBufferTime;
    public float attackBufferExpireTime;
    public int expAmountExtractedByAttack = 1;

    private PlayerFSM m;
    private AnimatorStateInfo info;
    private Dictionary<Animator, Vector2> attackDirection;
    private Dictionary<Animator, PolygonCollider2D> attackColliderDict;
    public PolygonCollider2D colliderUp;
    public PolygonCollider2D colliderDown;
    public PolygonCollider2D colliderRight;
    private Animator currentAttack;
    private bool hasAttackedEnemy;

    public bool isAttacking;

    private void Start()
    {
        m = PlayerFSM.instance;
        attackDirection = new Dictionary<Animator, Vector2>
        {
            { attackRight, new Vector2(1, 0) * m.facingDirection.x },
            { attackUp, new Vector2(0, 1) },
            { attackDown, new Vector2(0, -1) }
        };
        attackColliderDict = new Dictionary<Animator, PolygonCollider2D>
        {
            { attackRight, colliderRight },
            { attackUp, colliderUp },
            { attackDown, colliderDown }
        };
    }

    private void Attack()
    {
        isAttacking = true;
        attackBufferExpireTime = -1;

        List<int> direction = new() { -1, 1 };
        if (Input.GetKey(m.upKey))
            currentAttack = attackUp;
        else if (Input.GetKey(m.downKey) && !m.isOnGround)
            currentAttack = attackDown;
        else
            currentAttack = attackRight;
        currentAttack.transform.localScale = new Vector3(1, direction[Random.Range(0, 2)], 1);
        currentAttack.Play("PlayerAttack");

        // 更新向右攻击的方向
        attackDirection[attackRight] = new Vector2(1, 0) * m.facingDirection.x;

        StartCoroutine(UpdateAttackEnemyTrigger());
    }

    public void Update()
    {
        // Input Buffer
        if (Input.GetKeyDown(m.attackKey) && GameManager.instance.state == GameStateType.Play)
            attackBufferExpireTime = Time.time + attackBufferTime;
        // attack trigger
        if (Time.time >= attackCoolDownExpireTime && Time.time <= attackBufferExpireTime && !isAttacking)
        {
            if (new List<StateType>
                {
                    StateType.ReleaseArrow,
                    StateType.Drop,
                    StateType.Roar,
                    StateType.Recover,
                    StateType.SuperDash,
                    StateType.Dash
                }.Contains(
                    PlayerFSM.instance.currentStateType))
                return;
            Attack();
        }
    }

    private void LateUpdate()
    {
        if (!isAttacking)
            return;

        // OnEnter()
        // 这段话要不停调用（天坑！！！）
        info = currentAttack.GetCurrentAnimatorStateInfo(0);
        // The integer part is the number of time a state has been looped. The fractional part is the % (0-1) of progress in the current loop.
        // 整数部分是循环次数，小数部分是运行进度, 主要是为了防止攻击的时候转向
        if (info.IsName("PlayerAttack") && info.normalizedTime - (int)info.normalizedTime < 0.99f)
            return;

        // OnExit()
        attackCoolDownExpireTime = Time.time + attackCoolDown;
        hasAttackedEnemy = false;
        isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var component = other.GetComponent<ICanBeAttacked>();
        if (component != null)
        {
            component.Attacked();
            if (currentAttack == attackDown) // 只有下劈才刷新
            {
                m.canDoubleJump = true;
                m.canDash = true;
            }

            TryPushed();
            // 播放特效
            PlayerFSM.instance.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position));
        }
    }

    private IEnumerator UpdateAttackEnemyTrigger()
    {
        yield return new WaitForEndOfFrame();
        List<Collider2D> results = new();
        Physics2D.OverlapCollider(attackColliderDict[currentAttack], new ContactFilter2D() { useTriggers = true },
            results);
        foreach (var other in results) // 这样处理就不会出现，新生成的史莱姆马上被打死的情况
        {
            if (!other.CompareTag("Enemy"))
                continue;

            other.GetComponent<EnemyFSM>().Attacked(attackDamages[attackDamagesIdx], attackDirection[currentAttack] * attackForce);
            if (!hasAttackedEnemy) // 第一个打到的敌人 
            {
                m.PlayAttackEffect(other.bounds.ClosestPoint(currentAttack.transform.position)); // 在第一个打到的敌人上播放特效
                hasAttackedEnemy = true;
                TryPushed();
                // 更新属性
                if (currentAttack == attackDown) // 只有下劈才刷新
                {
                    m.canDoubleJump = true;
                    m.canDash = true;
                }

                // 更新exp
                m.curExp = Mathf.Min(m.curExp + expAmountExtractedByAttack, m.maxExp);
                PlayerExpUI.instance.UpdatePlayerExpUI();
            }
        }
    }

    public void TryPushed()
    {
        if (currentAttack == attackRight)
            m.rigidbody.velocity = new Vector2(
                -attackDirection[currentAttack].x * attackForce * rightAttackForceBackMultiplier,
                m.rigidbody.velocity.y);
        else if (currentAttack == attackDown)
        {
            m.rigidbody.velocity = new Vector2(m.rigidbody.velocity.x,
                -attackDirection[currentAttack].y * (attackForce * attackDownForceMultiplier));
            m.dashCoolDownExpireTime = -1; // 下劈立即刷新dash，怪不得下劈完冲不出去，我还以为是动画播的太慢了
        }
    }

    public void UpgradeSword()
    {
        attackDamagesIdx += 1;
        PlayerInfoUI.instance.UpdateItemInfoByType(AbilityType.Attack);
    }
}