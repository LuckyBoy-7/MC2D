using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Health : Singleton<Health>
{
    public int maxHealthPoint;
    public int healthPoint;

    private void Start()
    {
        healthPoint = maxHealthPoint;
    }

    public void TakeDamage(int damage)
    {
        healthPoint -= 1;   
        HealthUI.instance.UpdateUI(healthPoint);
        if (healthPoint == 0)
            Kill();
    }

    private void Kill()
    {
        Debug.Log("PlayerDead");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            TakeDamage(1);
    }
}