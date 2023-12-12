using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DamageableStone : MonoBehaviour, ICanBeAttacked
{
    public SpriteRenderer destroyMask;
    private int idx;
    public Sprite[] sprites;


    public void Attacked(Collider2D other = default)
    {
        if (idx < sprites.Length)
            destroyMask.sprite = sprites[idx++];
        else
            Destroy(gameObject);
    }
}