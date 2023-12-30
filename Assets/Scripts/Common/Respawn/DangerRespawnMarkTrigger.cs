using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerRespawnMarkTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerFSM.instance.dangerRespawnPoint = transform;
    }
}