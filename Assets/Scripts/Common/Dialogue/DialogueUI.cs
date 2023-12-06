using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public float openPanelTime;
    public Text text;
    public float showEachCharTimeGap;
    private int idx;
    private bool isBreaking;
    private CanvasGroup canvasGroup;

    protected void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(PlayerFSM.instance.jumpKey))
            isBreaking = true;
    }

    public void ShowDialogue(Dialogue_SO dialogue_SO)
    {
        gameObject.SetActive(true);
        StartCoroutine(_ShowDialogue(dialogue_SO));
    }

    private IEnumerator _ShowDialogue(Dialogue_SO dialogue_SO)
    {
        yield return StartCoroutine(OpenPanel());
        yield return StartCoroutine(ShowCharOneByOne(dialogue_SO.contents[idx++]));
        while (true)
        {
            yield return null;
            if (Input.GetKeyDown(PlayerFSM.instance.jumpKey))
            {
                if (idx < dialogue_SO.contents.Count)
                {
                    yield return StartCoroutine(ShowCharOneByOne(dialogue_SO.contents[idx++]));
                }
                else
                {
                    ClosePanel();
                }
            }
        }
    }

    private IEnumerator ShowCharOneByOne(string str)
    {
        isBreaking = false;
        StringBuilder s = new();
        foreach (var ch in str)
        {
            s.Append(ch);
            text.text = s.ToString();
            if (isBreaking)
                break;
            yield return new WaitForSeconds(showEachCharTimeGap);
        }

        text.text = str;
    }

    private IEnumerator OpenPanel()
    {
        GameManager.instance.state = GameStateType.Dialogue;
        var origScale = transform.localScale;
        transform.localScale = new Vector3(origScale.x, 0, origScale.z);
        transform.DOScaleY(1, openPanelTime);

        canvasGroup.alpha = 0;
        canvasGroup.DOFade(1, openPanelTime);
        yield return new WaitForSeconds(openPanelTime);
    }

    private void ClosePanel()
    {
        var origScale = transform.localScale;
        transform.localScale = new Vector3(origScale.x, 1, origScale.z);
        transform.DOScaleY(0, openPanelTime).OnComplete(() =>
            {
                text.text = "";
                gameObject.SetActive(false);
                GameManager.instance.state = GameStateType.Play;
            }
        );

        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, openPanelTime);
    }
}