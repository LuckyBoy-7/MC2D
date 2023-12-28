using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerArrowPrefab : MonoBehaviour
{
    public float destroyDelay;
    private Rigidbody2D rigidbody;
    public GameObject effect;

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
        {
            var scale = transform.localScale;
            transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }

        rigidbody.velocity = new Vector2(directX * speed, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            bool success = other.GetComponent<EnemyFSM>().Attacked(PlayerFSM.instance.arrowDamage);
            if (success)
                Instantiate(effect, other.transform.position, Quaternion.identity);
        }
    }
}