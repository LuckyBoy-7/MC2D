using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : Singleton<HealthUI>
{
    public HeartUI heartPrefabs;
    private List<HeartUI> hearts = new();

    private void Start()
    {
        for (int i = 0; i < Health.instance.maxHealthPoint; i++)
            hearts.Add(Instantiate(heartPrefabs, transform));
    }

    public void UpdateUI(int healthPoint)
    {
        for (int i = 0; i < healthPoint; i++)
            hearts[i].TryFulfill();
        for (int i = healthPoint; i < hearts.Count; i++)
            hearts[i].TryBreak();
    }
}