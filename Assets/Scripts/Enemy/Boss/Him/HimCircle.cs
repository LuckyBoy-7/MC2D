using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DG.Tweening;
using UnityEngine;

public class HimCircle : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private EdgeCollider2D edgeCollider;

    private Color origColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    private void Start()
    {
        origColor = spriteRenderer.color;
    }

    public void Resize(float to, float duration)
    {
        transform.DOScale(Vector3.one * to, duration).SetId("HimCircleScale");
    }

    public void TurnOnTrigger() => edgeCollider.enabled = true;
    public void TurnOffTrigger() => edgeCollider.enabled = false;

    public void ChangeColor(Color color, float duration)
    {
        spriteRenderer.DOColor(color, duration);
    }

    public void Reset()
    {
        spriteRenderer.color = origColor;
        transform.localScale = Vector3.zero;
        DOTween.Kill("HimCircleScale");
    }
}