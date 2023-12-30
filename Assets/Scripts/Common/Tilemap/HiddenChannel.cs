using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenChannel : Singleton<HiddenChannel>
{
    public float fadeDuration;
    private Tilemap tilemap;

    protected override void Awake()
    {
        base.Awake();
        tilemap = GetComponent<Tilemap>();
    }

    public void FadeTo(float target)
    {
        StopAllCoroutines();
        StartCoroutine(FadeTo_(target));
    }

    private IEnumerator FadeTo_(float target)
    {
        var speed = Mathf.Abs(target - tilemap.color.a) / fadeDuration;
        while (Mathf.Abs(target - tilemap.color.a) > 1e-5)
        {
            var newAlpha = Mathf.MoveTowards(tilemap.color.a, target, speed * Time.deltaTime);
            tilemap.color = new Color(tilemap.color.r, tilemap.color.g, tilemap.color.b, newAlpha);
            yield return null;
        }
    }
}