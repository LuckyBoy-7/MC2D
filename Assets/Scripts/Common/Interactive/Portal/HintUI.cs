using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HintUI : Singleton<HintUI>
{
    public CanvasGroup canvasGroup;
    public Text hintText;
    public float fadeDuration;

    private Transform fixedPoint;


    public void ChangeText(string newText, float duration)
    {
        hintText.DOFade(0, duration / 2).onComplete += () =>
        {
            hintText.text = newText;
            hintText.DOFade(1, duration / 2);
        };
    }

    private void Update()
    {
        if (fixedPoint)
            transform.position = Camera.main.WorldToScreenPoint(fixedPoint.position);
    }

    public void ChangeText(string newText)
    {
        hintText.text = newText;
    }

    public void FadeIn()
    {
        DOTween.Kill("HintUIFade");
        canvasGroup.DOFade(1, fadeDuration * (1 - canvasGroup.alpha)).SetId("HintUIFade");
    }

    public void FadeOut()
    {
        DOTween.Kill("HintUIFade");
        canvasGroup.DOFade(0, fadeDuration * canvasGroup.alpha).SetId("HintUIFade");
    }

    public void SetFixedPoint(Transform point)
    {
        fixedPoint = point;
    }
}