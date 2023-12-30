using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public Rigidbody2D rigidbody;
    public Explosion explosionPrefab;

    public AudioClip shootSfxSound;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        AudioManager.instance.Play(shootSfxSound);
        Destroy(gameObject, 15);
    }


    public void SetTrajectory(Vector2 force)
    {
        rigidbody.velocity = force; // y方向补偿力量的同时，高度差越大，补的越多
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            Destroy(gameObject);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
    }
}