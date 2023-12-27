using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class HimSpike : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rigidbody;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject,5);
    }

    private void Update()
    {
        boxCollider.size = new Vector2(spriteRenderer.size.x, boxCollider.size.y);
        boxCollider.offset = new Vector2(boxCollider.size.x / 2, boxCollider.offset.y);
    }

    public void SetDirUp()
    {
        transform.rotation = Quaternion.Euler(0, 0, 90);
    }
    
    public void SetDir(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void Resize(float to, float duration)
    {
        var sr = spriteRenderer;
        DOTween.To(() => sr.size.x, x => sr.size = new Vector2(x, sr.size.y), to, duration);
    }

    public void Resize(float to)
    {
        var sr = spriteRenderer;
        sr.size = new Vector2(to, sr.size.y);
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    public void TurnOnTrigger() => boxCollider.enabled = true;
    public void TurnOffTrigger() => boxCollider.enabled = false;

    public void Pushed(Vector2 force)
    {
        rigidbody.velocity = force;
    }
}