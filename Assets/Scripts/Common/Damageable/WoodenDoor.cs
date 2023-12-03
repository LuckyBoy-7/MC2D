using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WoodenDoor : MonoBehaviour
{
    public Transform door;
    public float doorOpenDuration;

    public void Attacked()
    {
        var player = PlayerFSM.instance;
        var dir = Mathf.Sign(transform.position.x - player.transform.position.x);
        door.DORotate(new Vector3(0, 90 - dir * 90, 0), doorOpenDuration);
        Destroy(GetComponent<BoxCollider2D>());
    }
}