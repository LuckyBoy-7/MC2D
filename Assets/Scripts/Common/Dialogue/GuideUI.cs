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
}