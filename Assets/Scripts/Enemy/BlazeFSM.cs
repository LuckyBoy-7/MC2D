using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlazeFSM : FlyEnemyFSM
{
    [Header("Blaze")] 
    public float shootSpeed;
    public FireBall fireBallPrefab;
    public float attackAngle;

    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Shoot;
    }

    private void Shoot()
    {
        var position = transform.position;
        var dir1 = (PlayerFSM.instance.transform.position - position).normalized;
        var dir2 = Quaternion.Euler(0, 0, attackAngle) * dir1;
        var dir3 = Quaternion.Euler(0, 0, -attackAngle) * dir1;
        var fireBall1 = Instantiate(fireBallPrefab, position, Quaternion.identity);
        var fireBall2 = Instantiate(fireBallPrefab, position, Quaternion.identity);
        var fireBall3 = Instantiate(fireBallPrefab, position, Quaternion.identity);
        fireBall1.SetTrajectory(dir1 * shootSpeed);
        fireBall2.SetTrajectory(dir2 * shootSpeed);
        fireBall3.SetTrajectory(dir3 * shootSpeed);
    }

}