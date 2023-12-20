using System;
using System.Collections;
using DG.Tweening;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class WitchArrow : MonoBehaviour
{
    public float delay;
    public float reachDuration;
    public float reachBehindPlayerDistance;
    public float rotateCircles;
    public float rotateDuration;
    public float remainTimeAfterShoot;
    private BoxCollider2D box;
    public Action onShootDown;

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
        Vector3 playerPos = PlayerFSM.instance.transform.position;
        Vector3 targetPos = playerPos + (playerPos - transform.position).normalized * reachBehindPlayerDistance;

        var origAngle = Vector3.SignedAngle(Vector3.right, transform.right, Vector3.forward);
        transform.DORotate(new Vector3(0, 0, origAngle + 360 * rotateCircles), rotateDuration,
            RotateMode.FastBeyond360).SetEase(Ease.Linear);

        yield return new WaitForSeconds(rotateDuration);
        box.enabled = true;
        transform.rotation =
            Quaternion.LookRotation(Vector3.forward, Quaternion.Euler(0, 0, 90) * (playerPos - transform.position));

        transform.DOMove(targetPos, reachDuration);
        yield return new WaitForSeconds(reachDuration);
        onShootDown?.Invoke();
        box.enabled = false;
        yield return new WaitForSeconds(remainTimeAfterShoot);
        StartCoroutine(Shoot());
    }
}