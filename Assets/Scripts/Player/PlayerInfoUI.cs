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
    public Image image;
    public string name;
    [TextArea] public string description;
    public InventoryType type;
    public bool isUnlocked;
}

public class PlayerInfoUI : MonoBehaviour
{
    public List<ItemInfo> itemInfos;
    public List<Image> images;
    public Text nameText;
    public Text descriptionText;
    private int idx;

    public RectTransform selectionFrame;
    public float selectFrameScaleCompensation;
    public float animationDuration;

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        UpdateUI();
    }

    private void Update()
    {
        var player = PlayerFSM.instance;
        if (Input.GetKeyDown(player.leftKey) || Input.GetKeyDown(player.rightKey))
        {
            int dir = Input.GetKeyDown(player.rightKey) ? 1 : -1;
            idx = (idx + dir + images.Count) % images.Count;
            UpdateUI();
        }
    }

    /// <summary>
    /// 主要更新SelectFrame和未获得物品的显隐情况
    /// </summary>
    public void UpdateUI()
    {
        UpdateShowHideState();
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
            nameText.text = info.name;
            descriptionText.text = info.description;
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
        {
            var color = Color.black;
            if (itemInfo.isUnlocked)
                color = Color.white;
            itemInfo.image.color = color;
        }
    }

    private ItemInfo GetItemInfoByImage() => itemInfos.Find(item => item.image == images[idx]);
}