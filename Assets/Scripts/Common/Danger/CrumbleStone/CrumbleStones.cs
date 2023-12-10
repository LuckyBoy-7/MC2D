using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumbleStones : MonoBehaviour
{
    public CrumbleStone[] crumbleStones;
    public float delay;
    private bool isTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            foreach (var stone in crumbleStones)
            {
                stone.onDestroy += () =>
                {
                    if (gameObject != null)
                        Destroy(gameObject);
                };
                stone.delay = delay;
                StartCoroutine(stone.Collapse());
            }
        }
    }
}