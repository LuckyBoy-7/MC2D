using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeCrystal : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerFSM.instance.maxHealthPoint += 1;
            PlayerFSM.instance.healthPoint += 1;
            HealthUI.instance.UpdateUI();
            Destroy(gameObject);
        }
    }
}