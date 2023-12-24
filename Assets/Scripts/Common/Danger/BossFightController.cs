using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossFightController : MonoBehaviour
{
    public bool isFighting;
    public bool isPassed;
    public GameObject[] targetsToActivate;
    public List<EnemyFSM> bosses = new();
    public List<IronBar> ironBars;


    protected void Defeated()
    {
        isPassed = true;
        isFighting = false;
        ClearPlace();
    }

    protected void Win()
    {
        isFighting = false;
        ClearPlace();
    }

    private void Update()
    {
        if (isFighting && PlayerFSM.instance.healthPoint <= 0)
        {
            Win();
        }

        if (isFighting && bosses.All(boss => boss.healthPoint <= 0))
        {
            Defeated();
        }
    }

    private void ClearPlace()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.CompareTag("IronBars"))
                continue;
            var enemy = child.GetComponent<EnemyFSM>();
            if (enemy)
            {
                if (enemy.isBoss)
                {
                    child.gameObject.SetActive(false);
                    enemy.Reset();
                }
                else // 清除小怪
                    Destroy(enemy.gameObject);
            }
        }

        foreach (var ironBar in ironBars)
        {
            ironBar.Open();
        }
    }

    private void FightStart()
    {
        foreach (var obj in targetsToActivate)
        {
            obj.SetActive(true);
        }

        foreach (var ironBar in ironBars)
        {
            ironBar.Close();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPassed && !isFighting)
        {
            isFighting = true;
            FightStart();
        }
    }
}