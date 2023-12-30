using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhastFSM : FlyEnemyFSM
{
    [Header("Ghast")] public float shootSpeed;
    public FireBall fireBallPrefab;
    public Sprite shootSprite;
    public float resetSpriteTime = 0.3f;
    private Sprite origSprite;
    private SpriteRenderer spriteRenderer;
    public AudioClip chargeSfxSound;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        origSprite = spriteRenderer.sprite;
    }

    protected override void Start()
    {
        base.Start();
        intervalAttackFunc += Shoot;
    }

    private void Shoot()
    {
        AudioManager.instance.Play(chargeSfxSound);
        var position = transform.position;
        var dir = (PlayerFSM.instance.transform.position - position).normalized;
        var fireBall = Instantiate(fireBallPrefab, position, Quaternion.identity);
        fireBall.SetTrajectory(dir * shootSpeed);

        // view
        spriteRenderer.sprite = shootSprite;
        Invoke(nameof(ResetSprite), resetSpriteTime);
    }

    private void ResetSprite() => spriteRenderer.sprite = origSprite;
}