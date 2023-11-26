using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using UnityEngine;

public class Health : Singleton<Health>
{
    public int maxHealthPoint;
    public int healthPoint;
    private SpriteRenderer spriteRenderer;

    private void OnDrawGizmos()
    {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
    }

    private void Start()
    {
        healthPoint = maxHealthPoint;
    }

    public void TakeDamage(int damage)
    {
        healthPoint -= damage;
        HealthUI.instance.UpdateUI(healthPoint);
        spriteRenderer.color = Color.red;
        spriteRenderer.DOColor(Color.white, PlayerFSM.instance.p.showHurtEffectTime);
        PlayerFSM.instance.p.isHurt = true;
        if (healthPoint == 0)
            Kill();
    }

    private void Kill()
    {
        Debug.Log("PlayerDead");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !PlayerFSM.instance.isInvincible)
        {
            TakeDamage(1);
            PlayerFSM.instance.p.hurtDirection = other.transform.position.x > PlayerFSM.instance.transform.position.x
                ? new Vector2(-PlayerFSM.instance.p.hurtDirectionXMultiplier, 1)
                : new Vector2(PlayerFSM.instance.p.hurtDirectionXMultiplier, 1);
        }
    }
}