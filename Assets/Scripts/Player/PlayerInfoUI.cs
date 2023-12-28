using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Serializable]
public class ItemInfo
{
    public AbilityType type; // 相当于tag
    public bool isUnlocked; // 物体显隐状态
    public Image image; // 物体对那个Image的位置 

    [HideInInspector] public int idx;
    public Sprite[] sprites; // 物体对应的多个sprites，比如剑就有好几种，升级的时候可以切换一个
    public string[] name; // 右边侧边栏对物体介绍的名字
    [TextArea] public string[] description; //
}

public class PlayerInfoUI : Singleton<PlayerInfoUI>
{
    public KeyCode inventoryKey = KeyCode.E;
    public GameObject infoUI;
    public List<ItemInfo> itemInfos;
    public List<Image> images; // 相当于决定了selectionFrame移动的顺序
    public Text nameText;
    public Text descriptionText;
    private int idx;

    public RectTransform selectionFrame;
    public float selectFrameScaleCompensation;
    public float animationDuration;

    private void Update()
    {
        UpdateOpenCloseState();
        if (!infoUI.activeSelf) // 关闭状态
            return;
        var player = PlayerFSM.instance;
        if (Input.GetKeyDown(player.leftKey) || Input.GetKeyDown(player.rightKey))
        {
            int dir = Input.GetKeyDown(player.rightKey) ? 1 : -1;
            idx = (idx + dir + images.Count) % images.Count;
            UpdateUI();
        }
    }

    private void UpdateOpenCloseState()
    {
        var manager = GameManager.instance;
        if (Input.GetKeyDown(inventoryKey))
        {
            if (infoUI.activeSelf)
            {
                manager.state = GameStateType.Play;
                infoUI.SetActive(false);
            }
            else if (manager.state == GameStateType.Play)
            {
                UpdateShowHideState(); // 更新player技能的获取状态
                UpdateUI();
                manager.state = GameStateType.PausePlayer;
                infoUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 主要更新SelectFrame和未获得物品的文本信息
    /// </summary>
    public void UpdateUI()
    {
        MoveSelectionFrame();
        UpdateText();
    }

    private void UpdateText()
    {
        var info = GetItemInfoByImage();
        nameText.text = "???";
        descriptionText.text = "???";
        if (info.isUnlocked) // 解锁了才显示
        {
            nameText.text = info.name[info.idx];
            descriptionText.text = info.description[info.idx];
        }
    }

    private void MoveSelectionFrame()
    {
        var imageTransform = images[idx].GetComponent<RectTransform>();
        var pos = imageTransform.position;
        var scale = imageTransform.sizeDelta * imageTransform.localScale / selectionFrame.localScale +
                    (Vector2.one * selectFrameScaleCompensation);
        selectionFrame.DOMove(pos, animationDuration);
        selectionFrame.DOSizeDelta(scale, animationDuration);
    }

    private void UpdateShowHideState()
    {
        foreach (var itemInfo in itemInfos)
            itemInfo.image.color = itemInfo.isUnlocked ? Color.white : Color.black;
    }

    public void UpdateItemInfoByType(AbilityType type)
    {
        ItemInfo info = GetItemInfoByType(type);
        info.idx += 1;
        info.image.sprite = info.sprites[info.idx];
        nameText.text = info.name[info.idx];
        descriptionText.text = info.description[info.idx];
    }

    public void UnlockInfo(AbilityType type)
    {
        ItemInfo info = GetItemInfoByType(type);
        info.isUnlocked = true;
    }

    private ItemInfo GetItemInfoByImage() => itemInfos.Find(item => item.image == images[idx]);
    private ItemInfo GetItemInfoByType(AbilityType type) => itemInfos.Find(item => item.type == type);
}