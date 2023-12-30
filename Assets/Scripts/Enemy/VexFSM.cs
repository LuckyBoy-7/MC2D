using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VexFSM : FlyEnemyFSM
{
    [Header("Blaze")] public float shootSpeed;
    public VexSpike spikePrefab;
    public Transform spikeWeapon;

    public AudioClip[] attackSfxSound;

    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Shoot;
    }

    private void Shoot()
    {
        AudioManager.instance.Play(attackSfxSound);
        var dir = (PlayerFSM.instance.transform.position - spikeWeapon.transform.position).normalized;
        Instantiate(spikePrefab, spikeWeapon.position, spikeWeapon.rotation).SetTrajectory(dir * shootSpeed);
    }

    protected override void Update()
    {
        base.Update();
        // 因为这里的图片是向上的，所以不用提前转90度
        var dir = (PlayerFSM.instance.transform.position - spikeWeapon.transform.position).normalized;
        spikeWeapon.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
    }
}