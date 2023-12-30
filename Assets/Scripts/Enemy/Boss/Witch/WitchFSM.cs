using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WitchFSM : EnemyFSM
{
    [Header("Movement")] public List<Transform> wanderPoses;
    public float wanderSpeed;

    [Header("Arrow Attack")] [Range(0, 1f)]
    public float firstWitchArrowTrigger = 0.8f;

    private bool isFirstWitchArrowTriggerDone;
    public WitchArrow arrow1;
    [Range(0, 1f)] public float secondWitchArrowTrigger = 0.4f;
    private bool isSecondWitchArrowTriggerDone;
    public float secondArrowDelay;
    public WitchArrow arrow2;
    [Header("Enemy Egg Release")] public EnemyEgg enemyEggPrefab;
    public float throwForce;
    public float throwEggTimeGap;
    private float throwEggElapse;
    public List<EnemyEggType> enemyEggTypes;
    [Range(0, 180f)] public float throwAngle; // 向上的夹角

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float half = throwAngle / 2;
        Vector2 left = Quaternion.Euler(0, 0, 90 + half) * Vector3.right;
        Gizmos.DrawRay(transform.position, left);
        Vector2 right = Quaternion.Euler(0, 0, 90 - half) * Vector3.right;
        Gizmos.DrawRay(transform.position, right);
    }

    protected override void Start()
    {
        base.Start();
        states[StateType.Wander] = new WitchWander(this); // idle
        TransitionState(StateType.Wander);
    }

    public void ThrowEnemyEgg()
    {
        float half = throwAngle / 2;
        var dir = Quaternion.Euler(0, 0, Random.Range(90 - half, 90 + half)) * Vector3.right;
        while (dir.magnitude < 0.5) // 力不能太小，不然很僵硬
            dir *= 2;

        var force = dir * throwForce;
        var enemyEgg = Instantiate(enemyEggPrefab, transform.position, Quaternion.identity, transform.parent);
        enemyEgg.Init(enemyEggTypes[Random.Range(0, enemyEggTypes.Count)]);
        enemyEgg.Pushed(force);
    }

    protected override void Update()
    {
        throwEggElapse += Time.deltaTime;
        if (throwEggElapse > throwEggTimeGap)
        {
            throwEggElapse -= throwEggTimeGap;
            ThrowEnemyEgg();
        }

        base.Update();
        if (healthPoint <= maxHealthPoint * firstWitchArrowTrigger && !isFirstWitchArrowTriggerDone)
        {
            isFirstWitchArrowTriggerDone = true;
            arrow1.gameObject.SetActive(true);
            arrow1.onShootDone += TrySpawnSecondArrow;
        }
    }


    private void TrySpawnSecondArrow()
    {
        if (healthPoint <= maxHealthPoint * secondWitchArrowTrigger && !isSecondWitchArrowTriggerDone)
        {
            isSecondWitchArrowTriggerDone = true;
            arrow2.gameObject.SetActive(true);
            arrow2.delay = secondArrowDelay;
        }
    }
}

public class WitchWander : IState
{
    private WitchFSM m;
    private Vector3 targetPos;
    private Vector3 prePos;

    public WitchWander(WitchFSM m)
    {
        this.m = m;
    }

    public void OnEnter()
    {
        RollTargetPos();
    }

    public void OnUpdate()
    {
        if (hasReachedTargetPos)
        {
            RollTargetPos();
        }
    }

    public void OnFixedUpdate()
    {
        m.transform.position =
            Vector3.MoveTowards(m.transform.position, targetPos, m.wanderSpeed * Time.fixedDeltaTime);
    }

    public void OnExit()
    {
    }

    private void RollTargetPos()
    {
        prePos = m.transform.position;
        targetPos = m.wanderPoses[Random.Range(0, m.wanderPoses.Count)].position;
        UpdateFacingDirection();
    }

    private bool hasReachedTargetPos => (targetPos - m.transform.position).magnitude < 1e-3;

    private void UpdateFacingDirection()
    {
        var sign = Mathf.Sign(targetPos.x - prePos.x);
        var scale = m.transform.localScale;
        m.transform.localScale = new Vector3(Mathf.Abs(scale.x) * sign, scale.y, scale.z);
    }
}