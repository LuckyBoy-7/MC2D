using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    public Vector2 targetDir;
    public float gravity; // 引力
    public Rigidbody2D rigidbody;
    public AudioClip spinSfxSound;
    private AudioSource spinAudioSource;

    private void Start()
    {
        spinAudioSource = AudioManager.instance.GetAudioSource(spinSfxSound);
    }

    private void FixedUpdate()
    {
        rigidbody.AddForce(targetDir * gravity);
    }

    private void OnBecameInvisible()
    {
        Destroy(spinAudioSource);
        Destroy(gameObject, 10f);
    }
}