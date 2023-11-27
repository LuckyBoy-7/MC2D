using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerArrow : MonoBehaviour
{
    public float destroyDelay;
    private Rigidbody2D rigidbody;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, destroyDelay);
    }

    public void SetMove(float directX, float speed)
    {
        if (directX < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        rigidbody.velocity = new Vector2(directX * speed, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
            other.GetComponent<EnemyFSM>().Attacked(PlayerFSM.instance.arrowDamage);
    }
}