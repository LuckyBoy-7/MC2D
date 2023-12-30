using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IronDoor : MonoBehaviour
{
    public Transform door;
    public float doorOpenDuration;
    public int dir;
    public AudioClip openSfxSound;
    private bool isDone;

    public void Open()
    {
        if (isDone)
            return;
        isDone = true;
        door.DORotate(new Vector3(0, 90 - dir * 90, 0), doorOpenDuration);
        AudioManager.instance.Play(openSfxSound);
        Destroy(GetComponent<BoxCollider2D>());
    }
}