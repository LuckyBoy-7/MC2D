using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

[Serializable]
public class SwordAttackEffectAnimationClip
{
    public List<Sprite> sprites;
}

public class SwordAttackEffect : MonoBehaviour
{
    public float eachFrameTime;
    
    public List<SwordAttackEffectAnimationClip> clips1, clips2;
    public SpriteRenderer spriteRenderer1, spriteRenderer2;
    private int idx1, idx2, i1, i2; // idx指向对应动画片段，i指向对应帧

    public void Start()
    {
        idx1 = Random.Range(0, clips1.Count);
        idx2 = Random.Range(0, clips2.Count);
        StopAllCoroutines();
        StartCoroutine(NextFrame());
    }


    private IEnumerator NextFrame()
    {
        while (true)
        {
            if (i1 < clips1[idx1].sprites.Count)
                spriteRenderer1.sprite = clips1[idx1].sprites[i1++];
            if (i2 < clips2[idx2].sprites.Count)
                spriteRenderer2.sprite = clips2[idx2].sprites[i2++];
            if (i1 >= clips1[idx1].sprites.Count && i2 >= clips2[idx2].sprites.Count)
                break;
            yield return new WaitForSeconds(eachFrameTime);
        }
        Destroy(gameObject);
    }
}