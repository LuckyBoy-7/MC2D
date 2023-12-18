using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhantomBlueFSM : FlyEnemyFSM
{

    [Header("PhantomBlue")] 
    public Boomerang boomerangPrefab;
    public float boomerangThrowForce;
    public float boomerangPullForce;
    public float randomPullRadius;  // 在enemy周围选取一个点，让回旋镖扔出去后一直受到这个方向的力

    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Attack;
    }

    private void Attack()
    {
        var position = PlayerFSM.instance.transform.position;
        var boomerang = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);
        boomerang.rigidbody.AddForce((position - transform.position) *boomerangThrowForce, ForceMode2D.Impulse);  // 不是单位向量，player越远，飞的越快
        boomerang.gravity = boomerangPullForce;
        boomerang.targetDir = ((Vector2)transform.position + Random.insideUnitCircle * randomPullRadius - (Vector2)position).normalized;
    }
}