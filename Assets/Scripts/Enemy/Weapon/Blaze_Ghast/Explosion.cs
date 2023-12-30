using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public AudioClip[] explosionSfxSound;

    private void Start()
    {
        AudioManager.instance.Play(explosionSfxSound);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            PlayerFSM.instance.TryTakeDamage(1, transform.position);
    }

    public void Kill() => Destroy(gameObject);
}