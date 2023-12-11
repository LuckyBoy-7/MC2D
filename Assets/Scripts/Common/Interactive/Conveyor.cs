using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    // 前提是没有在天花板上的conveyor
    public bool isHorizontal;
    public float leftForceBooster;
    public float rightForceBooster;
    public bool isVertical;
    public float upForceBooster; // 基本上只有向上移动和左右移动的传送带

    public bool isPlayerStaying;
    private bool isPrePlayerStaying;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerTriggerBox>())
            return;
        if (!isPlayerStaying)
        {
            if (isHorizontal)
                isPlayerStaying = PlayerFSM.instance.isOnGround;
            else if (isVertical)
                isPlayerStaying = PlayerFSM.instance.currentStateType == StateType.WallSlide;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerTriggerBox>())
            return;
        isPlayerStaying = false;
    }

    private void Update()
    {
        if (isPlayerStaying != isPrePlayerStaying) // 新的状态
        {
            var player = PlayerFSM.instance;
            if (isPlayerStaying) // 说明player刚上传送带
            {
                player.moveHorizontalExtraForce += -leftForceBooster + rightForceBooster;
                player.moveVerticalExtraForce += upForceBooster;
            }
            else // 说明player刚离开传送带
            {
                player.moveHorizontalExtraForce -= -leftForceBooster + rightForceBooster;
                player.moveVerticalExtraForce -= upForceBooster;
            }
        }

        isPrePlayerStaying = isPlayerStaying;
    }
}