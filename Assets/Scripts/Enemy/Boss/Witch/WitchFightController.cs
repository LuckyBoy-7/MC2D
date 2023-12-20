using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WitchFightController : BossFightController
{
    public WitchFSM witch;
    public Transform witchSpawnPoint;
    [Range(0, 1f)] public float firstWitchArrowTrigger = 0.8f;
    private bool isFirstWitchArrowTriggerDone;
    public WitchArrow arrow1;
    [Range(0, 1f)] public float secondWitchArrowTrigger = 0.4f;
    private bool isSecondWitchArrowTriggerDone;
    public float secondArrowDelay;
    public WitchArrow arrow2;

    private void Update()
    {
        witch.onKill += Defeated;
        if (witch.healthPoint <= witch.maxHealthPoint * firstWitchArrowTrigger && !isFirstWitchArrowTriggerDone)
        {
            isFirstWitchArrowTriggerDone = true;
            arrow1.gameObject.SetActive(true);
            arrow1.onShootDown += TrySpawnSecondArrow;
        }
    }

    protected override void ClearPlace()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);  
        }
    }

    protected override void FightStart()
    {
        witch.Reset();
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }

        arrow1.gameObject.SetActive(false);
        arrow2.gameObject.SetActive(false);
    }

    private void TrySpawnSecondArrow()
    {
        if (witch.healthPoint <= witch.maxHealthPoint * secondWitchArrowTrigger && !isSecondWitchArrowTriggerDone)
        {
            isSecondWitchArrowTriggerDone = true;
            arrow2.gameObject.SetActive(true);
            arrow2.delay = secondArrowDelay;
        }
    }
}