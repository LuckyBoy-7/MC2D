using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ChoiceUI : Singleton<ChoiceUI>
{
    [Header("Content")] public float openPanelDuration;
    public Text hintText;
    public Text numberText;
    public string hintContent;
    public float showEachCharTimeGap;

    public CanvasGroup canvasGroup;
    public GameObject dialoguePanel;
    [Header("Choice")] public int idx;
    public Image[] buttonImages;  // 左边是右边否
    public Sprite buttonSprite;
    public Sprite selectedButtonSprite;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            Show();
    }

    public void Show()
    {
        GameManager.instance.state = GameStateType.PausePlayer;
        dialoguePanel.SetActive(true);
        numberText.text = Smithy.instance.curCost.ToString();
        StartCoroutine(_Show());
    }

    private IEnumerator _Show()
    {
        yield return OpenPanel();
        yield return ShowCharOneByOne(hintContent);
        PlayerFSM player = PlayerFSM.instance;
        while (true)
        {
            yield return null;
            idx = (idx + buttonImages.Length
                   - Convert.ToInt32(Input.GetKeyDown(player.leftKey)) 
                   + Convert.ToInt32(Input.GetKeyDown(player.rightKey))) %  buttonImages.Length;
            UpdateUI();
            
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(player.jumpKey))  // 表示选择
            {
                if (idx == 0)
                    Smithy.instance.OnChooseYes();
                else if (idx == 1)
                    Smithy.instance.OnChooseNo();
                else
                    throw new Exception("除了是和否还能有别的选项？");
                
                ClosePanel();
                yield break;
            }
        }
    }

    public void UpdateUI()
    {
        buttonImages[idx].sprite = selectedButtonSprite;
        buttonImages[(idx + 1) % 2].sprite = buttonSprite;
    }

    private IEnumerator ShowCharOneByOne(string str)
    {
        StringBuilder s = new();
        foreach (var ch in str)
        {
            s.Append(ch);
            hintText.text = s.ToString();
            yield return new WaitForSeconds(showEachCharTimeGap);
        }

        hintText.text = str;
    }

    private IEnumerator OpenPanel()
    {
        GameManager.instance.state = GameStateType.PausePlayer;

        // 伸缩
        var scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, 0, scale.z);
        transform.DOScaleY(1, openPanelDuration);

        // 淡入
        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, openPanelDuration);
        yield return new WaitForSeconds(openPanelDuration);
    }

    private void ClosePanel()
    {
        // 伸缩
        var scale = transform.localScale;
        transform.localScale = new Vector3(scale.x, 1, scale.z);
        transform.DOScaleY(0, openPanelDuration);

        // 淡出
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, openPanelDuration).OnComplete(() =>
            {
                hintText.text = "";
                dialoguePanel.SetActive(false);
                // choice的closePanel不用设置游戏状态，因为选择完后面一定会跟一个smithy的对话
                // GameManager.instance.state = GameStateType.Play;
            }
        );
    }
}