using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleTorch : MonoBehaviour
{
    public bool hasTriggered;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasTriggered)
            return;
        GetComponent<Animator>().Play("TempleTorchTrigger");
        hasTriggered = true;
    }
}
