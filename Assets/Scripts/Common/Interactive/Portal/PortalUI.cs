using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PortalUI : Singleton<PortalUI>
{
    public CanvasGroup hintUI;
    public Text hintText;
    public List<string> unlockedPortals = new();
    private List<Transform> portalItmes = new();
    public Transform portalItemPrefab;
    public int idx;
    public Transform selectionFrame;
    public float selectionFrameMoveDuration;

    protected override void Awake()
    {
        base.Awake();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        PlayerFSM player = PlayerFSM.instance;
        if (Input.GetKeyDown(player.jumpKey) || Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.instance.state = GameStateType.Play;
            gameObject.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(player.upKey) || Input.GetKeyDown(player.downKey))
        {
            idx = (idx - Convert.ToInt32(Input.GetKeyDown(player.upKey)) +
                   Convert.ToInt32(Input.GetKeyDown(player.downKey)) + unlockedPortals.Count) % unlockedPortals.Count;
            selectionFrame.DOMove(transform.GetChild(idx).position, selectionFrameMoveDuration);
        }

        if (Input.GetKeyDown(player.attackKey) && unlockedPortals[idx] != Portal.currentPortal)
        {
            Portal.TeleportTo(unlockedPortals[idx]);
            GameManager.instance.state = GameStateType.Play;
            gameObject.SetActive(false);
        }
    }

    public void RegisterUnlockedPortal(string name)
    {
        unlockedPortals.Add(name);
        var item = Instantiate(portalItemPrefab, transform);
        item.GetChild(0).GetComponent<Text>().text = name;
        portalItmes.Add(item);
        item.SetSiblingIndex(transform.childCount - 2);
    }
}