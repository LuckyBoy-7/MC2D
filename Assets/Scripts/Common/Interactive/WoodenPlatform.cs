using System.Collections;
using DG.Tweening;
using UnityEngine;

public class WoodenPlatform : MonoBehaviour
{
    public float shakeTime;

    public float rotateDuration;

    public float flipRemainTime;

    private bool isWorking;

    public AudioClip flipSfxSound;
    public AudioClip flipBackSfxSound;
    private void OnCollisionStay2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Player") || isWorking)
            return;
        StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        isWorking = true;
        AudioManager.instance.Play(flipSfxSound);
        yield return new WaitForSeconds(shakeTime);
        transform.DORotate(new Vector3(0, 0, 180), rotateDuration).SetId("FlipSpike");
        yield return new WaitForSeconds(rotateDuration);
        yield return new WaitForSeconds(flipRemainTime);
        AudioManager.instance.Play(flipBackSfxSound);
        transform.DORotate(new Vector3(0, 0, 0), rotateDuration);
        isWorking = false;
    }


    public void TryReset(Collider2D other = default)
    {
        if (isWorking)  // 如果player攻击了正在反转的平台下的spike，则这个函数会被触发，平台立即复位
        {
            DOTween.Kill("FlipSpike");
            StopAllCoroutines();

            var resetDuration = Mathf.Abs(transform.eulerAngles.z / 180) * rotateDuration;
            transform.DORotate(new Vector3(0, 0, 0), resetDuration).onComplete += () => { isWorking = false; };
            AudioManager.instance.Play(flipBackSfxSound);
        }
    }
}