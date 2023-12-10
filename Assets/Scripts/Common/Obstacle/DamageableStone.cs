using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DamageableStone : MonoBehaviour
{
    public SpriteRenderer destroyMask;
    private int idx;
    public Sprite[] sprites;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            if (idx < sprites.Length)
                destroyMask.sprite = sprites[idx++];
            else
                Destroy(gameObject);
        }
    }
}