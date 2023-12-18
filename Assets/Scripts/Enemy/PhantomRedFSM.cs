using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomRedFSM : FlyEnemyFSM
{
    [Header("Blaze")] public float shootSpeed;
    public FireBall fireBallPrefab;

    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Shoot;
    }

    private void Shoot()
    {
        var position = transform.position;
        var startDir = Quaternion.Euler(0, 0, 45) * Vector3.right;
        for (int i = 0; i < 4; i++)
        {
            var dir = Quaternion.Euler(0, 0, 90 * i) * startDir;
            var fireBall = Instantiate(fireBallPrefab, position, Quaternion.identity);
            fireBall.SetTrajectory(dir * shootSpeed);
        }
    }
}