using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class Lever : MonoBehaviour, ICanBeAttacked
{
    public Transform lever;
    public float leverRotateDuration;
    public UnityEvent onAttacked;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack"))
        {
            Attacked();
            onAttacked?.Invoke();
        }
    }

    public void Attacked(Collider2D other = default)
    {
        var player = PlayerFSM.instance;
        var dir = Mathf.Sign(transform.position.x - player.transform.position.x);
        lever.DORotate(new Vector3(0, 0, -dir * 30), leverRotateDuration).onComplete += () =>
            lever.DORotate(new Vector3(0, 0, 0), leverRotateDuration);
    }
}