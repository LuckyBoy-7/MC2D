using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExpUI : Singleton<PlayerExpUI>
{
    public Image image;
    public float changeAmountDuration;

    public void UpdatePlayerExpUI()
    {
        var player = PlayerFSM.instance;
        image.DOFillAmount((float)player.curExp / player.maxExp, changeAmountDuration);
    }
}
