using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonArrow : MonoBehaviour
{
    public Transform massCenter;
    public Rigidbody2D rigidbody;
    public float activeTime = 10;

    private bool isOnPlatform => rigidbody.bodyType == RigidbodyType2D.Kinematic;
    public AudioClip[] attackSfxSound;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        AudioManager.instance.Play(attackSfxSound);
        rigidbody.centerOfMass = massCenter.localPosition;
    }

    private void Update()
    {
        if (isOnPlatform)
            return;
        var dir = rigidbody.velocity.normalized;
        var angle = Vector3.SignedAngle(Vector3.right, dir, Vector3.forward);
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetTrajectory(Vector2 force)
    {
        rigidbody.velocity = force; // y方向补偿力量的同时，高度差越大，补的越多
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOnPlatform)
        {
            PlayerFSM.instance.TryTakeDamage(1, transform.position);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            Destroy(gameObject, activeTime);
            rigidbody.velocity = Vector2.zero;
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }
}