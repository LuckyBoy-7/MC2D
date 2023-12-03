using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using DG.Tweening;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class IronBar : MonoBehaviour
{
    public float moveDistance;
    public float moveDuration;
    private Vector2 origPos;

    public void Start()
    {
        origPos = transform.position;
    }

    public void Close()
    {
        transform.DOMoveY(transform.position.y + moveDistance, moveDuration);
    }

    public void Open()
    {
        transform.DOMoveY(origPos.y, moveDuration);
    }
}