using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.Serialization;

public class WitchArrow : MonoBehaviour
{
    public float delay;
    public float reachDuration;
    public float reachBehindPlayerDistance;
    public float rotateCircles;
    public float rotateDuration;
    public float remainTimeAfterShoot;
    private BoxCollider2D box;
    public Action onShootDone;

    public AudioClip shootSfxSound;

    private void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        box.enabled = false;
    }

    private void OnEnable()
    {
        StartCoroutine(Shoot());
    }

    private IEnumerator Shoot()
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(_Shoot());
    }

    private IEnumerator _Shoot()
    {
        // 快速自转n圈
        var origAngle = Vector3.SignedAngle(Vector3.right, transform.right, Vector3.forward);
        transform.DORotate(new Vector3(0, 0, origAngle + 360 * rotateCircles), rotateDuration,
            RotateMode.FastBeyond360).SetEase(Ease.Linear);
        yield return new WaitForSeconds(rotateDuration);
        // 瞄准player射出
        box.enabled = true;
        Vector3 playerPos = PlayerFSM.instance.transform.position;
        Vector3 targetPos = playerPos + (playerPos - transform.position).normalized * reachBehindPlayerDistance;
        transform.rotation =
            Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 90) * (playerPos - transform.position));
        transform.DOMove(targetPos, reachDuration);
        AudioManager.instance.Play(shootSfxSound);
        yield return new WaitForSeconds(reachDuration);
        //
        onShootDone?.Invoke();
        box.enabled = false;
        yield return new WaitForSeconds(remainTimeAfterShoot);
        StartCoroutine(Shoot());
    }
}