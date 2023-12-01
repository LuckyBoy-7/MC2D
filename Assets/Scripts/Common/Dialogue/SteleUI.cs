using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DG.Tweening;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

/// <summary>
/// 位置一开始已经用verticalLayout调过了，现在我们只要保存他们的状态即可
/// </summary>
public class SteleUI : Singleton<SteleUI>
{
    private List<Vector3> poses;
    public float fadeTime;
    public float showEachCharTimeGap;

    public Text header;
    private Vector2 headerOrigPos;

    public Text content1;
    public Text content2;

    private Vector2 origPos;
    private float spacing;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        origPos = transform.position;
        headerOrigPos = header.transform.position;
        spacing = headerOrigPos.y - content1.transform.position.y;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void HideContents()
    {
        transform.DOMove(transform.position + Vector3.down * spacing, fadeTime);
        canvasGroup.DOFade(0, fadeTime).onComplete += Reset;
    }

    private void Reset()
    {
        StopAllCoroutines();
        transform.position = origPos; 
        header.text = "";
        content1.text = "";
        content2.text = "";
        canvasGroup.alpha = 1;
    }

    public void ShowContents(List<string> contents)
    {
        // 首先是标题淡入
        header.text = contents[0];
        header.DOColor(new Color(1, 1, 1, 1), fadeTime);
        header.transform.position = content1.transform.position;
        header.transform.DOMove(headerOrigPos, fadeTime);
        StartCoroutine(ShowCharOneByOne(contents));
    }

    private IEnumerator ShowCharOneByOne(List<string> contents)
    {
        yield return new WaitForSeconds(fadeTime);
        foreach (var c in contents[1])
        {
            content1.text += c;
            yield return new WaitForSeconds(showEachCharTimeGap);
        }

        if (contents.Count >= 3)
        {
            foreach (var c in contents[2])
            {
                content2.text += c;
                yield return new WaitForSeconds(showEachCharTimeGap);
            }
        }
    }
}