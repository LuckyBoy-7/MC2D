using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDeathParticle : MonoBehaviour
{
    public Transform singleParticle;
    public int particleNum = 8;
    public float radius;
    private static float pushedDuration;
    public float burstDuration = 0.5f;
    public float rotateDuration = 1f;
    public float rotateSpeed;

    public float pushedForce;

    private bool isRealeasing;
    private float elapse;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (pushedDuration == 0)
            pushedDuration = GetLengthByName("DeathParticle");
        
        // // Test
        // Pushed(Vector3.right);
    }

    public void Pushed(Vector3 dir = default)
    {
        transform.DOMove(transform.position + dir * pushedForce, pushedDuration);
    }

    public void Release()
    {
        isRealeasing = true;
        for (int i = 0; i < particleNum; i++)
        {
            var dir = Quaternion.Euler(0, 0, 360f * i / particleNum) * Vector3.right;
            var targetPos = dir * radius;
            var particle = Instantiate(singleParticle, transform);
            particle.position = transform.position;
            particle.DOLocalMove(targetPos, burstDuration);
            particle.DOScale(Vector3.zero, rotateDuration).onComplete += () => DeathTransition.instance.TryPlay();
        }
    }

    private void Update()
    {
        if (!isRealeasing)
            return;
        if (elapse > rotateDuration)
            Destroy(gameObject);
        elapse += Time.deltaTime;

        transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));
    }

    public float GetLengthByName(string name)
    {
        float length = 0;
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name.Equals(name))
            {
                length = clip.length;
                break;
            }
        }

        return length;
    }
}