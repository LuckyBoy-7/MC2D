using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class TNT : MonoBehaviour
{
    public Explosion explosion;
    public float blinkDuration;
    public int blinkCount;
    public SpriteRenderer mask;


    private void Start()
    {
        var duration = blinkDuration / blinkCount / 2;
        var s = DOTween.Sequence();
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
        Destroy(gameObject);
    }
}