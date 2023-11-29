using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EmeraldEmitter : MonoBehaviour
{
    public Rigidbody2D emeraldPrefab;
    public int num;
    public float speed;

    private void Start()
    {
        for (int i = 0; i < num; i++)
        {
            var rb = Instantiate(emeraldPrefab, transform.position, Quaternion.identity);
            var randomDirection = Random.insideUnitCircle.normalized;
            rb.velocity = randomDirection * speed;
        }

        Destroy(gameObject);
    }
}