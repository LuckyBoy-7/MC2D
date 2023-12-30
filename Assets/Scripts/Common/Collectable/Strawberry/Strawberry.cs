using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Strawberry : MonoBehaviour
{
    public GameObject collectedEffect;

    public AudioClip collectedSfxSound;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Instantiate(collectedEffect, transform.position, Quaternion.identity);
            AudioManager.instance.Play(collectedSfxSound);
            Destroy(gameObject);
        }
    }
}