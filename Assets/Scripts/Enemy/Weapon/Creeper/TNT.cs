using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class TNT : MonoBehaviour
{
    public Explosion explosion;
    public float blinkDuration;
    public int blinkCount;
    public SpriteRenderer mask;

    public float delay;
    public float protectionTime = 0.1f;  // 防止一生成就碰到player了
    private float protectionElapse;
    private Rigidbody2D rigidbody;
    private Sequence s;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }


    private void Start()
    {
        var duration = blinkDuration / blinkCount / 2;
        s = DOTween.Sequence();
        s.AppendInterval(delay);
        for (int i = 0; i < blinkCount; i++)
        {
            s.Append(mask.DOFade(1, duration));
            s.Append(mask.DOFade(0, duration));
        }

        s.AppendCallback(Explode);
    }

    private void Explode()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        s.Kill();
        Destroy(gameObject);
    }

    public void Pushed(Vector2 force)
    {
        rigidbody.velocity = force;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerTriggerBox>() && protectionElapse > protectionTime)
        {
            Explode();
        }
    }

    private void Update()
    {
        protectionElapse += Time.deltaTime;
    }

    public void SetFloat()
    {
        rigidbody.gravityScale = 0;
        rigidbody.velocity = Vector2.zero;
    }
}