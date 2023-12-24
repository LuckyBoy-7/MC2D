using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Timers;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerRoarPrefab : MonoBehaviour
{
    [HideInInspector] public Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        PlayerFSM.instance.onHurt += Kill;
    }

    private void OnDestroy()
    {
        PlayerFSM.instance.onHurt -= Kill;
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            other.GetComponent<EnemyFSM>().roarHurtElapse = Single.PositiveInfinity; // 一进来碰到敌人就把他们的计数器重置
    }
}