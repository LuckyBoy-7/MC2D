using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class WoodenPlatform : MonoBehaviour
{
    public float shakeTime;

    public float rotateDuration;

    public float flipRemainTime;

    private bool isWorking;
    public BoxCollider2D spikeCollider;


    private void OnCollisionStay2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Player") || isWorking)
            return;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        isWorking = true;
        yield return new WaitForSeconds(shakeTime);
        transform.DORotate(new Vector3(0, 0, 180), rotateDuration).SetId("FlipSpike");
        yield return new WaitForSeconds(rotateDuration);
        yield return new WaitForSeconds(flipRemainTime);
        transform.DORotate(new Vector3(0, 0, 0), rotateDuration);
        isWorking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerAttack") && isWorking)
        {
            if (spikeCollider.IsTouching(other)) // 如果player攻击了平台下的spike，则平台立即复位
            {
                DOTween.Kill("FlipSpike");
                StopAllCoroutines();

                var resetDuration = Mathf.Abs(transform.eulerAngles.z / 180) * rotateDuration;
                transform.DORotate(new Vector3(0, 0, 0), resetDuration).onComplete += () => { isWorking = false; };
            }
        }
    }
}