using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : Singleton<HealthUI>
{
    private List<HeartUI> hearts = new();
    public HeartUI heartPrefab;
    public Transform heartContainer;
    private List<HeartUI> extraHearts = new();
    public HeartUI extraHeartPrefab;
    public Transform extraHeartContainer;


    private void Start()
    {
        for (int i = 0; i < PlayerFSM.instance.maxHealthPoint; i++)
            hearts.Add(Instantiate(heartPrefab, heartContainer));
    }

    public void UpdateUI()
    {
        int healthPoint = PlayerFSM.instance.healthPoint;
        while (healthPoint > hearts.Count) // 意味着得到了生命水晶，UIHeart++
        {
            hearts.Insert(0, Instantiate(heartPrefab, heartContainer));
        }


        for (int i = 0; i < healthPoint; i++)
            hearts[i].TryFulfill();
        for (int i = healthPoint; i < hearts.Count; i++)
            hearts[i].TryBreak();
        int extraHealthPoint = PlayerFSM.instance.extraHealthPoint;
        while (extraHearts.Count < extraHealthPoint)
            extraHearts.Add(Instantiate(extraHeartPrefab, extraHeartContainer));
        while (extraHearts.Count > extraHealthPoint)
        {
            // pop
            var extraHeart = extraHearts[^1];
            extraHearts.RemoveAt(extraHearts.Count - 1);
            Destroy(extraHeart.gameObject);
        }
    }
}