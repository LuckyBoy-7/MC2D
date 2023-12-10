using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LifeFruit : MonoBehaviour
{
    public float popUpForce;

    private void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(Random.insideUnitCircle.normalized * popUpForce, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerTriggerBox>())
        {
            PlayerFSM.instance.GetExtraHealth();
            Destroy(gameObject);
        }
    }
}