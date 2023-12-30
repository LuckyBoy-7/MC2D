using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ExpStatue : MonoBehaviour, ICanBeAttacked
{
    [Range(0, 1f)] public float minAlpha = 0.3f;
    public int expCollectedMaxCount = 3;
    private int curCount;
    private SpriteRenderer spriteRenderer;
    public AudioClip expSfxSound;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Attacked(Collider2D other = default)
    {
        if (curCount < expCollectedMaxCount)
        {
            PlayerFSM.instance.UpdateExp(1);
            curCount += 1;
            var alpha = 1 - (1 - minAlpha) / expCollectedMaxCount * curCount;
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            AudioManager.instance.Play(expSfxSound);
        }
    }
}