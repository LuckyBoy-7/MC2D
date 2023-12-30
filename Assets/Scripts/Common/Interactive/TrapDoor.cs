using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrapDoor : MonoBehaviour
{
    public Transform door;
    public Transform doorSide;
    public float doorOpenDuration;
    public float shakeCount;
    public float shakeTimeGap;
    public float shakeMagnitude;
    private Vector2 origPos;
    private BoxCollider2D collider;
    
    public AudioClip openSfxSound;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        origPos = doorSide.transform.position;
    }

    public void Attacked()
    {
        door.DORotate(new Vector3(0, 0, 0), doorOpenDuration);
        AudioManager.instance.Play(openSfxSound);
        Destroy(collider);
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
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
    }
}