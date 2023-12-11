using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PhantomFSM : FlyEnemyFSM
{
    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Attack;
    }

    private void Attack()
    {
        LerpVelocity((PlayerFSM.instance.transform.position - transform.position).normalized * moveSpeed);
    }
}