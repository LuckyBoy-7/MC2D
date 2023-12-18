using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PhantomYellowFSM : FlyEnemyFSM
{
    public float stabDistance;
    public float stabDuration;
    public float stabTriggerRadius;
    private bool isAttackStored; // 可能已经可以攻击了，但是player不在范围内

    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Attack;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stabTriggerRadius);
    }

    protected override void Update()
    {
        base.Update();
        if ((PlayerFSM.instance.transform.position - transform.position).magnitude < stabTriggerRadius &&
            isAttackStored)
        {
            isAttackStored = false;
            ResetIntervalAttackElapse();
            StartCoroutine(_Attack());
        }
    }

    private void Attack()
    {
        isAttackStored = true;
    }


    private IEnumerator _Attack()
    {
        isSelfControlMovement = true;
        canBeKnockedBack = false;
        var dir = (PlayerFSM.instance.transform.position - transform.position).normalized;
        var origPos = transform.position;
        var targetPos = origPos + dir * stabDistance;
        transform.DOMove(targetPos, stabDuration);
        yield return new WaitForSeconds(stabDuration / 2);
        transform.DOMove(origPos, stabDuration);
        yield return new WaitForSeconds(stabDuration / 2);
        canBeKnockedBack = true;
        isSelfControlMovement = false;
    }
}