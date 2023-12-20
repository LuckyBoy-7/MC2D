using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PortalUI : Singleton<PortalUI>
{
    private List<string> unlockedPortals = new();
    public CanvasGroup canvasGroup;
    private bool isShowing;
    public float fadeDuration = 0.2f;
    public Transform destinationBarPrefab; // 传送点的一个个bar
    public Transform selectionFrame;
    public float selectionFrameMoveDuration;
    private List<Transform> portalItmes = new();
    private int idx; // 当前frame指向的destination

    private bool canOperate;


    private void Update()
    {
        if (!canOperate)
            return;
        PlayerFSM player = PlayerFSM.instance;
        if (Input.GetKeyDown(player.attackKey) || Input.GetKeyDown(KeyCode.Escape))
            Hide();
        if (!isShowing)
            return;

        // 现在在地图界面
        // 光标移动
        if (Input.GetKeyDown(player.upKey) || Input.GetKeyDown(player.downKey))
        {
            idx = (idx
                   - Convert.ToInt32(Input.GetKeyDown(player.upKey))
                   + Convert.ToInt32(Input.GetKeyDown(player.downKey))
                   + unlockedPortals.Count) % unlockedPortals.Count;
            selectionFrame.DOMove(transform.GetChild(idx).position, selectionFrameMoveDuration);
        }

        // 选择传送(并且传送的不是当前位置)
        if ((Input.GetKeyDown(player.jumpKey) || Input.GetKeyDown(KeyCode.Return)) &&
            unlockedPortals[idx] != Portal.currentPortal)
        {
            Hide();
            Portal.TeleportTo(unlockedPortals[idx]);
        }
    }

    public void RegisterUnlockedPortal(string name)
    {
        var bar = Instantiate(destinationBarPrefab, transform);
        unlockedPortals.Add(name);
        portalItmes.Add(bar);
        bar.GetChild(0).GetComponent<Text>().text = name;
        bar.SetSiblingIndex(transform.childCount - 2);  // 因为ui是下面盖上面的，所以新生成的bar要设置到frame的上层
    }

    public void Show()
    {
        GameManager.instance.state = GameStateType.PausePlayer;
        canvasGroup.DOFade(1, fadeDuration);
        isShowing = true;
        StartCoroutine(DelayedOneFrame());
    }

    public void Hide()
    {
        GameManager.instance.state = GameStateType.Play;
        canvasGroup.DOFade(0, fadeDuration);
        isShowing = false;
        StartCoroutine(DelayedOneFrame());
    }

    private IEnumerator DelayedOneFrame()
    {
        canOperate = false;
        yield return null; // 等待一帧
        yield return new WaitForEndOfFrame();
        canOperate = true;
    }
}