using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RespawnHintUI : Singleton<RespawnHintUI>
{
    public float fadeDuration;
    public float remainDuration;
    private Text text;

    protected override void Awake()
    {
        base.Awake();
        text = GetComponent<Text>();
    }

    public void Show()
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
        DOTween.Kill("RespawnHintUI");
        DOVirtual.DelayedCall(remainDuration, Hide).SetId("RespawnHintUI");
    }

    public void Hide()
    {
        text.DOFade(0, fadeDuration).SetId("RespawnHintUI");
    }
}