using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WitchEnemiesSpawner : MonoBehaviour
{
    public EdgeCollider2D ceiling;
    public float spawnTimeGap;
    public EnemyEgg egg;
    public Sprite[] enemiesSprites;
    private float elapse;

    private void Update()
    {
        elapse += Time.deltaTime;
        if (elapse < spawnTimeGap)
            return;
        elapse -= spawnTimeGap;

        // 在随机位置生成一个随机敌人
        float x = Random.Range(ceiling.bounds.min.x, ceiling.bounds.max.x);
        float y = Random.Range(ceiling.bounds.min.y, ceiling.bounds.max.y);
        var randomPos = new Vector3(x, y);
        Instantiate(egg, randomPos, Quaternion.identity, transform.parent).Init(enemiesSprites[Random.Range(0, enemiesSprites.Length)]);
    }
}