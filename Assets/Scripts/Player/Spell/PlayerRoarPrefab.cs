using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class PlayerRoarPrefab : MonoBehaviour
{
    public void Kill()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            other.GetComponent<EnemyFSM>().roarHurtElapse = Single.PositiveInfinity;  // 一进来碰到敌人就把他们的计数器重置
    }
}