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
    public Sprite[] sprites; // 物体对应的多个sprites，比如剑就有好几种，升级的时候可以切换一个
    [HideInInspector] public int spriteIdx; // 物体对应的多个sprites，比如剑就有好几种，升级的时候可以切换一个


    public string[] name; // 右边侧边栏对物体介绍的名字
    [HideInInspector] public int nameIdx;
    [TextArea] public string[] description; //
    [HideInInspector] public int descriptionIdx;
}

public class PlayerInfoUI : Singleton<PlayerInfoUI>
{
    public List<ItemInfo> itemInfos;
    public List<Image> images; // 相当于决定了selectionFrame移动的顺序
    public Text nameText;
    public Text descriptionText;
    private int idx;

    public RectTransform selectionFrame;
    public float selectFrameScaleCompensation;
    public float animationDuration;

    private void OnEnable()
    {
        UpdateShowHideState();  // 更新player技能的获取状态
    }

    private void Start()
    {
        Canvas.ForceUpdateCanvases(); // 因为layout更新不及时
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
            nameText.text = info.name[info.nameIdx];
            descriptionText.text = info.description[info.descriptionIdx];
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

    public void UpdateItemInfoByType(AbilityType type)
    {
        ItemInfo info = GetItemInfoByType(type);
        info.image.sprite = info.sprites[++info.spriteIdx];
        nameText.text = info.name[++info.nameIdx];
        descriptionText.text = info.description[++info.descriptionIdx];
    }
    
    public void UnlockInfo(AbilityType type)
    {
        ItemInfo info = GetItemInfoByType(type);
        info.isUnlocked = true;
    }

    private ItemInfo GetItemInfoByImage() => itemInfos.Find(item => item.image == images[idx]);
    private ItemInfo GetItemInfoByType(AbilityType type) => itemInfos.Find(item => item.type == type);
}