using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class DripStoneTrigger : MonoBehaviour
{
    private bool hasTriggered;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            GetComponentInChildren<DripStone>().Drip();
        }
    }
}
