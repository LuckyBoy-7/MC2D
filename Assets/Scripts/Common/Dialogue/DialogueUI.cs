using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DialogueUI : Singleton<DialogueUI>
{
    public float openPanelDuration;
    public Text text;
    public float showEachCharTimeGap;
    private bool isSkipping;

    public CanvasGroup canvasGroup;
    public GameObject dialoguePanel;

    private void Update()
    {
        if (Input.GetKeyDown(PlayerFSM.instance.jumpKey))
            isSkipping = true;
    }

    public void ShowDialogue(Dialogue_SO dialogue_SO)
    {
        if (dialogue_SO.contents.Count == 0)  // 保险措施
            return;
        dialoguePanel.SetActive(true);
        StartCoroutine(_ShowDialogue(dialogue_SO));
    }

    private IEnumerator _ShowDialogue(Dialogue_SO dialogue_SO)
    {
        yield return OpenPanel();
        foreach (var content in dialogue_SO.contents)
        {
            yield return ShowCharOneByOne(content);
            while (!Input.GetKeyDown(PlayerFSM.instance.jumpKey))
                yield return null;
        }
        ClosePanel();
    }

    private IEnumerator ShowCharOneByOne(string str)
    {
        isSkipping = false;

        StringBuilder s = new();
        foreach (var ch in str)
        {
            s.Append(ch);
            text.text = s.ToString();
            if (isSkipping)
                break;
            yield return new WaitForSeconds(showEachCharTimeGap);
        }

        text.text = str;
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
                text.text = "";
                dialoguePanel.SetActive(false);
                GameManager.instance.state = GameStateType.Play;
            }
        );
    }
}