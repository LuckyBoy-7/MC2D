using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class IronTrapDoor : MonoBehaviour
{
    public Transform door;
    public Transform doorSide;
    public float doorOpenDuration;
    public float shakeCount;
    public float shakeTimeGap;
    public float shakeMagnitude;
    private Vector2 origPos;
    private BoxCollider2D collider;
    public float resetTime;

    public AudioClip openSfxSound;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        origPos = doorSide.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && PlayerFSM.instance.isOnGround)
            Shake();
    }

    public void Shake()
    {
        StartCoroutine(_Shake());
    }

    public IEnumerator _Shake()
    {
        for (int i = 0; i < shakeCount; i++)
        {
            doorSide.transform.position += (Vector3)Random.insideUnitCircle * shakeMagnitude;
            yield return new WaitForSeconds(shakeTimeGap);
        }

        doorSide.transform.position = origPos;
        door.DORotate(new Vector3(0, 0, 0), doorOpenDuration);
        collider.enabled = false;
        AudioManager.instance.Play(openSfxSound);

        yield return new WaitForSeconds(resetTime);
        door.DORotate(new Vector3(90, 0, 0), doorOpenDuration).onComplete += () => { collider.enabled = true; };
    }
}