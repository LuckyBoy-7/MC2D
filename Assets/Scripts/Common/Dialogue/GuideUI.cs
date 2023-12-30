using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GuideUI : Singleton<GuideUI>
{
    public float fadeTime;
    public float showTime;

    [Header("Move")] public GameObject moveGameObject;
    public Text moveText1;
    public Text moveText2;
    public Text moveUpButtonText;
    public Text moveDownButtonText;
    public Text moveLeftButtonText;
    public Text moveRightButtonText;
    [Header("One")] public GameObject oneGameObject;
    public Text oneText1;
    public Text oneText2;
    public Text oneButtonText;
    [Header("Combined")] public GameObject combinedGameObject;
    public Text combinedText1;
    public Text combinedText2;
    public Text combinedButtonText1;
    public Text combinedButtonText2;
    [Header("NoButton")] 
    public GameObject textWithoutButtonGameObject;
    public Text textWithoutButton;

    private CanvasGroup canvasGroup;
    private GameObject curPanel;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void HideContents()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
    }

    /// <summary>
    /// Move
    /// </summary>
    /// <param name="guideType"></param>
    public void ShowContents(string text1, string text2)
    {
        var player = PlayerFSM.instance;
        moveText1.text = text1;
        moveText2.text = text2;
        moveLeftButtonText.text = player.leftKey.ToString();
        moveRightButtonText.text = player.rightKey.ToString();
        moveUpButtonText.text = player.upKey.ToString();
        moveDownButtonText.text = player.downKey.ToString();
        moveGameObject.SetActive(true);
        Fade();
    }

    /// <summary>
    /// One
    /// </summary>
    public void ShowContents(string text1, string text2, KeyCode key)
    {
        oneText1.text = text1;
        oneText2.text = text2;
        oneButtonText.text = key.ToString();
        oneGameObject.SetActive(true);
        Fade();
    }

    /// <summary>
    /// Combined
    /// </summary>
    public void ShowContents(string text1, string text2, KeyCode key1, KeyCode key2)
    {
        combinedText1.text = text1;
        combinedText2.text = text2;
        combinedButtonText1.text = key1.ToString();
        combinedButtonText2.text = key2.ToString();
        combinedGameObject.SetActive(true);
        Fade();
    }
    
    /// <summary>
    /// Combined
    /// </summary>
    public void ShowContents(string text)
    {
        textWithoutButton.text = text;
        textWithoutButtonGameObject.SetActive(true);
        Fade();
    }

    private void Fade()
    {
        // 淡入，展示x秒，淡出，然后setActive为False
        canvasGroup.alpha = 0;
        Sequence seq = DOTween.Sequence();
        seq.Append(canvasGroup.DOFade(1, fadeTime));
        seq.AppendInterval(showTime);
        seq.Append(canvasGroup.DOFade(0, fadeTime));
        seq.Play().onComplete += HideContents;
    }
    
    public void Show(AbilityType abilityType)
    {
        var player = PlayerFSM.instance;
        if (abilityType == AbilityType.Move)
            instance.ShowContents("按住","移动");
        else if (abilityType == AbilityType.Attack)
            instance.ShowContents("点击", "攻击", player.attackKey);
        else if (abilityType == AbilityType.Jump)
            instance.ShowContents("按住", "跳跃", player.jumpKey);
        else if (abilityType == AbilityType.Dash)
            instance.ShowContents("点击", "冲刺", player.dashKey);
        else if (abilityType == AbilityType.Heal)
            instance.ShowContents("按住", "治愈", player.spellKey);
        else if (abilityType == AbilityType.ReleaseArrow)
            instance.ShowContents("点击", "放箭", player.spellKey);
        else if (abilityType == AbilityType.Drop)
            instance.ShowContents("按住", "下砸", player.downKey, player.spellKey);
        else if (abilityType == AbilityType.Roar)
            instance.ShowContents("按住", "上吼", player.upKey, player.spellKey);
        else if (abilityType == AbilityType.SuperDash)
            instance.ShowContents("按住", "超冲", player.superDashKey);
        else if (abilityType == AbilityType.DoubleJump)
            instance.ShowContents("按住", "二段跳", player.jumpKey);
        else if (abilityType == AbilityType.WallSlide)
            instance.ShowContents("蹬墙跳Get");
        else if (abilityType == AbilityType.Bag)
            instance.ShowContents("点击", "打开背包", PlayerInfoUI.instance.inventoryKey);
        
    }
}