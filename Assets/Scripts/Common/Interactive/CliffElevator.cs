using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEngine;

public class CliffElevator : MonoBehaviour
{
    public Transform up;
    public Transform down;

    public float moveSpeed;
    public float remainTime;
    private float elapse;
    private bool isPlayerTakingLift;
    private bool isAwait;
    private bool isLocked = true;

    private void Update()
    {
        if (isLocked)
            return;
        if (isAwait)
            return;
        if (isPlayerTakingLift)
        {
            elapse = 0;
            transform.position = Vector3.MoveTowards(transform.position, up.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            elapse += Time.deltaTime;
            if (elapse > remainTime)
            {
                transform.position = Vector3.MoveTowards(transform.position, down.position, moveSpeed * Time.deltaTime);
                if ((transform.position - down.position).magnitude < 1e-3)
                {
                    isAwait = true;
                    elapse = 0;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player") && PlayerFSM.instance.isOnGround && !PlayerFSM.instance.isMovingUp)
        {
            isPlayerTakingLift = true;
            isAwait = false;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.CompareTag("Player"))
            isPlayerTakingLift = false;
    }

    public void UnLock()
    {
        isLocked = false;
    }
}