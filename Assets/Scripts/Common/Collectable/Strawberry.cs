using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strawberry : MonoBehaviour
{
    public GameObject collectedEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(collectedEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}