using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TrapRoomEnemyInfo
{
    public EnemyFSM enemy;
    public Transform spawnTransform;
}

[Serializable]
public class TrapRoomWave
{
    public List<TrapRoomEnemyInfo> enemyInfos;
}

public class TrapRoom : MonoBehaviour
{
    public List<IronBar> controlledIronBars;
    public List<TrapRoomWave> waves;
    private bool isTrapping; // 表示当前是否正在困住player，不然player如果在陷阱房死了，就少一个状态
    private bool isOver; // player是否已经打过了

    private void Start()
    {
        PlayerFSM.instance.OnRevive += TryReset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isTrapping || isOver)
            return;
        StartCoroutine(TrapPlayer());
    }

    private IEnumerator TrapPlayer()
    {
        isTrapping = true;
        foreach (var controlledIronBar in controlledIronBars)
            controlledIronBar.Close();
        foreach (var wave in waves)
        {
            int deadEnemyNum = 0;
            foreach (var trapRoomEnemyInfo in wave.enemyInfos)
            {
                var enemy = Instantiate(trapRoomEnemyInfo.enemy, trapRoomEnemyInfo.spawnTransform.position,
                    Quaternion.identity, transform);
                enemy.onKill += () => { deadEnemyNum += 1; };
            }

            while (deadEnemyNum < wave.enemyInfos.Count)
            {
                yield return null;
            }
        }

        foreach (var controlledIronBar in controlledIronBars)
            controlledIronBar.Open();
        isOver = true;
        isTrapping = false;
    }

    public void TryReset()
    {
        if (isTrapping) // 说明是在这个陷阱房里死的
        {
            StopAllCoroutines();
            isTrapping = false;
            foreach (var controlledIronBar in controlledIronBars)
                controlledIronBar.Open();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<EnemyFSM>())
                    Destroy(child.gameObject);
            }
        }
    }
}