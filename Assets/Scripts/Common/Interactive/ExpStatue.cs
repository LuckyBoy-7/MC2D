using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpStatue : MonoBehaviour
{
    private BoxCollider2D box;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            PlayerFSM.instance.PlayAttackEffect(box.ClosestPoint(other.transform.position));
            other.GetComponent<PlayerAttack>().TryPushed();
        }
    }
}
