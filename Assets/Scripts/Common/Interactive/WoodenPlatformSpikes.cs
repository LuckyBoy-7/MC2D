using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodenPlatformSpikes : MonoBehaviour, ICanBeAttacked
{
    private WoodenPlatform platform;

    private void Awake()
    {
        platform = transform.parent.GetComponent<WoodenPlatform>();
    }

    public void Attacked(Collider2D other = default)
    {
        platform.TryReset();
    }
}
