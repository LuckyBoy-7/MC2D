using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    private Image image;
    // public Sprite fullHeart;
    // public Sprite emptyHeart;

    private void Awake()
    {
        image = transform.GetChild(0).GetComponent<Image>();
    }

    public void TryFulfill()
    {
        image.enabled = true;
        // image.sprite = fullHeart;
    }

    public void TryBreak()
    {
        image.enabled = false;
        // image.sprite = emptyHeart;
    }
}
