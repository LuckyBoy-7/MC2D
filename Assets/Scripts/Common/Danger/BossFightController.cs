using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossFightController : MonoBehaviour
{
    protected bool isFighting;
    protected bool isPassed;

    private void Start()
    {
        ClearPlace();
    }

    protected void Defeated()
    {
        isPassed = true;
        ClearPlace();
    }

    protected void Win()
    {
        ClearPlace();
    }

    private void Update()
    {
        if (isFighting && PlayerFSM.instance.healthPoint <= 0)
        {
            Win();
        }
    }

    protected abstract void ClearPlace();

    protected abstract void FightStart();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPassed && !isFighting)
        {
            isFighting = true;
            FightStart();
        }
    }
}