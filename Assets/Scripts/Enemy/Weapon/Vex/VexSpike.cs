using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VexSpike : MonoBehaviour
{
    private Rigidbody2D rigidbody;
    public GameObject spikeExplosion;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, 20f); // 20s后还没碰到墙壁就自毁 
    }

    public void SetTrajectory(Vector2 force)
    {
        rigidbody.velocity = force;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            var pos = other.ClosestPoint(transform.position);
            Instantiate(spikeExplosion, pos, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
