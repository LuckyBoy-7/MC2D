using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IronDoor : MonoBehaviour
{
    public Transform door;
    public float doorOpenDuration;
    public int dir;

    public void Open()
    {
        door.DORotate(new Vector3(0, 90 - dir * 90, 0), doorOpenDuration);
        Destroy(GetComponent<BoxCollider2D>());
    }
}