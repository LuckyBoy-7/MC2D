using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CrumbleStone : MonoBehaviour
{
    public float delay;

    public SpriteRenderer crumbleMask;
    public float crumbleDuration;
    public Sprite[] crumbleSprites;
    public Action onDestroy;  // 可能会被调用，但大多时候只是个陷阱，不是一个整体

    public IEnumerator Collapse()
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(ShowCrumbleAnimation());
        Destroy(gameObject);
        onDestroy?.Invoke();
    }

    private IEnumerator ShowCrumbleAnimation()
    {
        float timeGap = crumbleDuration / crumbleSprites.Length;
        int i = 0;
        while (i < crumbleSprites.Length)
        {
            crumbleMask.sprite = crumbleSprites[i++];
            yield return new WaitForSeconds(timeGap);
        }
    }
}